using System;
using UnityEngine;

namespace NpcCompass;

public sealed class NpcCompassBehaviour : MonoBehaviour
{
    public NpcCompassBehaviour(IntPtr handle) : base(handle)
    {
    }

    private void Awake()
    {
        NpcScanner.Reset();
        NpcScanner.SetActive(Plugin.Enabled.Value);
    }

    private void Update()
    {
        if (Input.GetKeyDown(Plugin.ToggleKey.Value))
        {
            bool next = !NpcScanner.IsActive;
            NpcScanner.SetActive(next);
            Plugin.Enabled.Value = next;
            Plugin.Log.LogDebug($"[NpcCompass] Scanner {(next ? "enabled" : "disabled")}.");
        }
    }

    private void LateUpdate()
    {
        NpcScanner.Scan();
    }

    private void OnGUI()
    {
        CompassRenderer.Draw();
    }

    private void OnDestroy()
    {
        CompassRenderer.DisposeTextures();
    }
}
