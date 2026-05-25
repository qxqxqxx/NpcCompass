using System;
using System.Collections.Generic;
using ExposureUnnoticed2.Object3D.NPC.Script;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NpcCompass;

internal static class NpcScanner
{
    private static readonly List<NpcSnapshot> snapshots = new List<NpcSnapshot>();
    private static float lastSummaryLogTime = -999f;
    private static float lastErrorLogTime = -999f;
    private static bool isActive;
    private static bool isManagerReady;
    private static float lastScanTime;

    public static IReadOnlyList<NpcSnapshot> LatestSnapshot => snapshots;
    public static bool IsActive => isActive;
    public static bool IsManagerReady => isActive && isManagerReady;
    public static float LastScanTime => lastScanTime;

    public static void SetActive(bool active)
    {
        if (isActive == active)
        {
            return;
        }

        isActive = active;
        if (!active)
        {
            snapshots.Clear();
            isManagerReady = false;
        }

        lastSummaryLogTime = -999f;
    }

    public static void Reset()
    {
        snapshots.Clear();
        lastSummaryLogTime = -999f;
        lastErrorLogTime = -999f;
        isManagerReady = false;
        lastScanTime = 0f;
    }

    public static void Scan()
    {
        if (!isActive)
        {
            return;
        }

        try
        {
            ScanInternal();
        }
        catch (Exception ex)
        {
            if (Time.unscaledTime - lastErrorLogTime >= 5f)
            {
                lastErrorLogTime = Time.unscaledTime;
                Plugin.Log.LogWarning($"[NpcCompass] Scan failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static void ScanInternal()
    {
        NpcManager manager = NpcManager.Instance;
        if (manager == null || manager.ExistNpcList == null)
        {
            snapshots.Clear();
            isManagerReady = false;
            MaybeLogSummary(0);
            return;
        }

        isManagerReady = true;
        snapshots.Clear();
        int npcCount = manager.ExistNpcList.Count;

        for (int i = 0; i < npcCount; i++)
        {
            NpcController npc;
            try
            {
                npc = manager.ExistNpcList[i];
            }
            catch
            {
                continue;
            }

            if (TryCapture(npc, out NpcSnapshot snap))
            {
                snapshots.Add(snap);
            }
        }

        lastScanTime = Time.unscaledTime;
        MaybeLogSummary(snapshots.Count);
    }

    private static bool TryCapture(NpcController npc, out NpcSnapshot snapshot)
    {
        snapshot = default;

        if (npc == null)
        {
            return false;
        }

        try
        {
            if (!npc.IsExistObject || !npc.IsAlive)
            {
                return false;
            }

            Transform t = npc.Transform;
            if (IsUnityNull(t))
            {
                return false;
            }

            NpcFixedType fixedType = ReadFixedType(npc);
            float strangeness = ReadStrangeness(npc);
            snapshot = new NpcSnapshot(npc.id, t.position, t.forward, fixedType, strangeness);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static NpcFixedType ReadFixedType(NpcController npc)
    {
        try
        {
            NpcComponentAccessor nca = npc.Nca;
            if (nca == null)
            {
                return NpcFixedType.Unknown;
            }

            NpcStateModel sm = nca.StateModel;
            if (sm == null)
            {
                return NpcFixedType.Unknown;
            }

            return (NpcFixedType)(int)sm.CurrentFixedType;
        }
        catch
        {
            return NpcFixedType.Unknown;
        }
    }

    private static float ReadStrangeness(NpcController npc)
    {
        try
        {
            float v = npc.StrangenessValue;
            if (!float.IsFinite(v))
            {
                return 0f;
            }
            return Mathf.Clamp01(v);
        }
        catch
        {
            return 0f;
        }
    }

    private static void MaybeLogSummary(int count)
    {
        if (Time.unscaledTime - lastSummaryLogTime < Mathf.Max(1f, Plugin.SummaryLogIntervalSeconds.Value))
        {
            return;
        }

        lastSummaryLogTime = Time.unscaledTime;

        if (count == 0)
        {
            Plugin.Log.LogDebug($"[NpcCompass] NPCs=0, managerReady={isManagerReady}");
            return;
        }

        int pinponCount = 0;
        int conbiniCount = 0;
        float strangenessMax = 0f;
        float strangenessSum = 0f;
        for (int i = 0; i < snapshots.Count; i++)
        {
            NpcFixedType t = snapshots[i].FixedType;
            if (t == NpcFixedType.Pinpon) pinponCount++;
            else if (t == NpcFixedType.Conbini) conbiniCount++;

            float st = snapshots[i].Strangeness;
            strangenessSum += st;
            if (st > strangenessMax) strangenessMax = st;
        }
        int otherCount = count - pinponCount - conbiniCount;
        float strangenessAvg = strangenessSum / snapshots.Count;

        NpcSnapshot s = snapshots[0];
        Plugin.Log.LogDebug(
            $"[NpcCompass] NPCs={count} (Pinpon={pinponCount}, Conbini={conbiniCount}, Other={otherCount}), " +
            $"StrangenessMax={strangenessMax:F2} Avg={strangenessAvg:F2}, " +
            $"managerReady={isManagerReady}, " +
            $"sample: id={s.Id} pos=({s.Position.x:F2}, {s.Position.y:F2}, {s.Position.z:F2}) " +
            $"fwd=({s.Forward.x:F2}, {s.Forward.y:F2}, {s.Forward.z:F2}) type={s.FixedType} strangeness={s.Strangeness:F2}");
    }

    private static bool IsUnityNull(Object obj)
    {
        return (object)obj == null || obj == null;
    }
}
