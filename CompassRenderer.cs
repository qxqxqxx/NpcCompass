using System.Collections.Generic;
using ExposureUnnoticed2.Object3D.Player.Scripts;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NpcCompass;

internal static class CompassRenderer
{
    private const int TriangleTexSize = 32;
    private const float TriangleDrawSize = 10f;

    private static readonly Color DiscColor = new Color(0f, 0f, 0f, 0.4f);
    private static readonly Color RingColor = Color.white;
    private static readonly Color FixedNpcColor = Color.white;
    private static readonly Color OtherNpcColor = new Color(1f, 0.65f, 0f, 1f);
    private static readonly Color PlayerColor = new Color(0f, 1f, 0.4f, 1f);

    private static Texture2D discTexture;
    private static Texture2D triangleTexture;
    private static int discTextureRadius;

    public static void Draw()
    {
        if (!NpcScanner.IsActive)
        {
            return;
        }

        Camera cam = Camera.main;
        if (IsUnityNull(cam))
        {
            return;
        }

        PlayerController player = PlayerController.Instance;
        if (IsUnityNull(player))
        {
            return;
        }

        Transform playerTransform = player.transform;
        if (IsUnityNull(playerTransform))
        {
            return;
        }

        EnsureTextures();

        int radius = discTextureRadius;
        Vector2 center = GetCompassCenter(radius);
        float maxRange = Mathf.Max(0.01f, Plugin.MaxRange.Value);
        float pixelsPerUnit = radius / maxRange;

        Vector3 camForward = cam.transform.forward;
        Vector3 camForwardXZ = new Vector3(camForward.x, 0f, camForward.z);
        if (camForwardXZ.sqrMagnitude < 0.0001f)
        {
            return;
        }
        camForwardXZ.Normalize();
        Vector3 camRightXZ = new Vector3(camForwardXZ.z, 0f, -camForwardXZ.x);

        DrawDisc(center, radius);

        IReadOnlyList<NpcSnapshot> snapshot = NpcScanner.LatestSnapshot;
        Vector3 playerPos = playerTransform.position;
        for (int i = 0; i < snapshot.Count; i++)
        {
            DrawNpc(snapshot[i], playerPos, camForwardXZ, camRightXZ, center, pixelsPerUnit, maxRange);
        }

        DrawPlayerMarker(playerTransform.forward, camForwardXZ, camRightXZ, center);
    }

    public static void DisposeTextures()
    {
        if (!IsUnityNull(discTexture))
        {
            Object.Destroy(discTexture);
        }
        if (!IsUnityNull(triangleTexture))
        {
            Object.Destroy(triangleTexture);
        }
        discTexture = null;
        triangleTexture = null;
        discTextureRadius = 0;
    }

    private static void EnsureTextures()
    {
        int targetRadius = Mathf.Clamp(Mathf.RoundToInt(Plugin.Radius.Value), 20, 500);
        if (!IsUnityNull(discTexture) && !IsUnityNull(triangleTexture) && discTextureRadius == targetRadius)
        {
            return;
        }

        DisposeTextures();
        discTextureRadius = targetRadius;
        discTexture = GenerateDiscTexture(targetRadius);
        triangleTexture = GenerateTriangleTexture();
    }

    private static Texture2D GenerateDiscTexture(int radius)
    {
        int size = radius * 2;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.hideFlags = HideFlags.HideAndDontSave;

        float cx = size * 0.5f - 0.5f;
        float cy = size * 0.5f - 0.5f;
        float fRadius = radius;
        Color transparent = new Color(0f, 0f, 0f, 0f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                Color c;
                if (dist < fRadius - 1.5f)
                {
                    c = DiscColor;
                }
                else if (dist < fRadius - 0.5f)
                {
                    c = RingColor;
                }
                else if (dist < fRadius + 0.5f)
                {
                    float a = fRadius + 0.5f - dist;
                    c = new Color(RingColor.r, RingColor.g, RingColor.b, a);
                }
                else
                {
                    c = transparent;
                }
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateTriangleTexture()
    {
        int size = TriangleTexSize;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.hideFlags = HideFlags.HideAndDontSave;

        float cx = size * 0.5f - 0.5f;
        float maxHalfWidth = size * 0.4f;
        Color transparent = new Color(0f, 0f, 0f, 0f);

        for (int y = 0; y < size; y++)
        {
            float distFromApex = (size - 1) - y;
            float t = distFromApex / (size - 1f);
            float halfWidth = maxHalfWidth * t;

            for (int x = 0; x < size; x++)
            {
                float distFromCenter = Mathf.Abs(x - cx);
                float edgeDist = halfWidth - distFromCenter;
                Color c;
                if (edgeDist > 1f)
                {
                    c = Color.white;
                }
                else if (edgeDist > 0f)
                {
                    c = new Color(1f, 1f, 1f, edgeDist);
                }
                else
                {
                    c = transparent;
                }
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        return tex;
    }

    private static Vector2 GetCompassCenter(int radius)
    {
        float w = Screen.width;
        float h = Screen.height;
        float mx = Plugin.MarginX.Value;
        float my = Plugin.MarginY.Value;

        switch (Plugin.Anchor.Value)
        {
            case CompassAnchor.TopLeft:
                return new Vector2(mx + radius, my + radius);
            case CompassAnchor.TopRight:
                return new Vector2(w - mx - radius, my + radius);
            case CompassAnchor.BottomRight:
                return new Vector2(w - mx - radius, h - my - radius);
            case CompassAnchor.BottomLeft:
                return new Vector2(mx + radius, h - my - radius);
            case CompassAnchor.TopCenter:
                return new Vector2(w * 0.5f, my + radius);
            case CompassAnchor.MiddleLeft:
                return new Vector2(mx + radius, h * 0.5f);
            case CompassAnchor.MiddleRight:
            default:
                return new Vector2(w - mx - radius, h * 0.5f);
        }
    }

    private static void DrawDisc(Vector2 center, int radius)
    {
        Rect rect = new Rect(center.x - radius, center.y - radius, radius * 2, radius * 2);
        Color oldColor = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(rect, discTexture);
        GUI.color = oldColor;
    }

    private static void DrawNpc(NpcSnapshot npc, Vector3 playerPos, Vector3 camFwd, Vector3 camRight, Vector2 center, float pixelsPerUnit, float maxRange)
    {
        float heightDiff = Mathf.Abs(npc.Position.y - playerPos.y);
        if (heightDiff > Mathf.Max(0f, Plugin.MaxHeightDiff.Value))
        {
            return;
        }

        Vector3 relPos = npc.Position - playerPos;
        float localX = Vector3.Dot(relPos, camRight);
        float localZ = Vector3.Dot(relPos, camFwd);

        float distSq = localX * localX + localZ * localZ;
        if (distSq > maxRange * maxRange)
        {
            return;
        }

        float screenX = center.x + localX * pixelsPerUnit;
        float screenY = center.y - localZ * pixelsPerUnit;

        float fwdLocalX = Vector3.Dot(npc.Forward, camRight);
        float fwdLocalZ = Vector3.Dot(npc.Forward, camFwd);
        float angleDeg = Mathf.Atan2(fwdLocalX, fwdLocalZ) * Mathf.Rad2Deg;

        Color color = npc.FixedType == NpcFixedType.Pinpon
            ? FixedNpcColor
            : OtherNpcColor;
        DrawTriangleAt(new Vector2(screenX, screenY), angleDeg, color);
    }

    private static void DrawPlayerMarker(Vector3 playerForward, Vector3 camFwd, Vector3 camRight, Vector2 center)
    {
        float fwdLocalX = Vector3.Dot(playerForward, camRight);
        float fwdLocalZ = Vector3.Dot(playerForward, camFwd);
        float angleDeg = Mathf.Atan2(fwdLocalX, fwdLocalZ) * Mathf.Rad2Deg;
        DrawTriangleAt(center, angleDeg, PlayerColor);
    }

    private static void DrawTriangleAt(Vector2 screenPos, float angleDeg, Color color)
    {
        float half = TriangleDrawSize * 0.5f;
        Rect rect = new Rect(screenPos.x - half, screenPos.y - half, TriangleDrawSize, TriangleDrawSize);

        Matrix4x4 oldMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angleDeg, screenPos);
        Color oldColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, triangleTexture);
        GUI.color = oldColor;
        GUI.matrix = oldMatrix;
    }

    private static bool IsUnityNull(Object obj)
    {
        return (object)obj == null || obj == null;
    }
}
