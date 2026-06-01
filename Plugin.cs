using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace NpcCompass;

public enum CompassAnchor
{
    TopRight,
    TopLeft,
    BottomRight,
    BottomLeft,
    TopCenter,
    MiddleRight,
    MiddleLeft,
}

[BepInPlugin(MyPluginInfo.PluginGuid, MyPluginInfo.PluginName, MyPluginInfo.PluginVersion)]
public sealed class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;
    internal static ConfigEntry<bool> Enabled;
    internal static ConfigEntry<KeyCode> ToggleKey;
    internal static ConfigEntry<float> SummaryLogIntervalSeconds;
    internal static ConfigEntry<CompassAnchor> Anchor;
    internal static ConfigEntry<float> MarginX;
    internal static ConfigEntry<float> MarginY;
    internal static ConfigEntry<float> Radius;
    internal static ConfigEntry<float> MaxRange;
    internal static ConfigEntry<float> MaxHeightDiff;
    internal static ConfigEntry<bool> ShowPinponNpcs;

    public override void Load()
    {
        Log = base.Log;

        Enabled = Config.Bind("General", "Enabled", true, "Whether NPC compass scanning is enabled when the plugin loads.");
        ToggleKey = Config.Bind("Input", "ToggleKey", KeyCode.F8, "Keyboard key used to toggle the compass.");
        SummaryLogIntervalSeconds = Config.Bind("Diagnostics", "SummaryLogIntervalSeconds", 5f, "Seconds between debug summary logs (only visible when BepInEx LogLevel includes Debug).");

        Anchor = Config.Bind("Display", "Anchor", CompassAnchor.MiddleRight, "Screen anchor for the compass.");
        MarginX = Config.Bind("Display", "MarginX", 20f, "Horizontal pixel margin from the anchor.");
        MarginY = Config.Bind("Display", "MarginY", 20f, "Vertical pixel margin from the anchor.");
        Radius = Config.Bind("Display", "Radius", 100f, "Compass radius in pixels. Change requires game restart to regenerate textures.");
        MaxRange = Config.Bind("Display", "MaxRange", 30f, "World units beyond which NPCs are not drawn on the compass.");
        MaxHeightDiff = Config.Bind("Display", "MaxHeightDiff", 3f, "Hide NPCs whose Y position differs from the player by more than this many world units (filters other floors).");
        ShowPinponNpcs = Config.Bind("Display", "ShowPinponNpcs", false, "Show 'pinpon' NPCs (white triangles, indoor NPCs that answer the doorbell) on the compass. Hidden by default.");

        AddComponent<NpcCompassBehaviour>();
        Log.LogInfo($"{MyPluginInfo.PluginName} {MyPluginInfo.PluginVersion} loaded.");
    }
}
