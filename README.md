# NPC Compass

**English** | [中文](README.zh-CN.md)

A BepInEx plugin that overlays a circular compass on the game screen, showing real-time positions and orientations of NPCs in the current scene.

![Demo](demo.png)

## Features

- Circular compass anchored to the right edge of the screen
- Camera-relative orientation: what's in front of your camera is at the top of the compass
- Renders the player and surrounding NPCs as triangles indicating both position and facing direction
- NPC color reflects current suspicion level (strangeness) — white when calm, deep red when highly alarmed
- Automatic filtering: NPCs too far away or on a different floor are not drawn

### Compass elements

| Element | Color | Meaning |
|---|---|---|
| Player | Bright green triangle | Always at the center; apex points where the player body faces |
| Regular NPC | White → red gradient triangle | All non-doorbell NPCs (pedestrians, sitting NPCs, drivers, store clerks, etc.). Color reflects current strangeness: white = 0 (calm), deep red = 1 (highly alarmed) |
| Pinpon NPC | Light gray triangle | Indoor NPCs that answer when you ring the doorbell |

The triangle apex always points in the direction the NPC is currently facing.

## Usage

Press **F8** to toggle the compass on/off.

That's it. The compass updates passively every frame.

## Installation

### Prerequisite: BepInEx 6 (IL2CPP)

This plugin requires **BepInEx 6.0.0-be.735** or a compatible bleeding-edge 6.x build (IL2CPP, x64).

The tested build is `BepInEx 6.0.0-be.735` (commit `5fef3570`). Other bleeding-edge 6.x builds will likely work as long as the IL2CPP interop API hasn't changed.

If the game directory already has a `BepInEx/` folder and other BepInEx plugins run fine, skip this step. Otherwise:

1. Download from the [BepInEx bleeding-edge artifacts](https://builds.bepinex.dev/projects/bepinex_be) — pick `BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.XXX+...zip`
2. Extract into the game root (the folder containing `SecretFlasherManaka.exe`)
3. Launch the game once so BepInEx generates the full `BepInEx/` folder structure, then quit

### Install the plugin

1. Open the game root, go into `BepInEx/plugins/` (create it if missing)
2. Drop `NpcCompass.dll` into it

   You can also nest it: `BepInEx/plugins/NpcCompass/NpcCompass.dll`. BepInEx scans recursively, both layouts work.

3. Launch the game and enter a scene with NPCs. Press F8 — the compass should appear on the right side of the screen.
4. On first launch, the config file `BepInEx/config/com.local.npccompass.cfg` is auto-generated.

### Verify installation

Open `BepInEx/LogOutput.log` (or watch the BepInEx console). You should see:

```
[Info: NPC Compass] NPC Compass 0.5.0 loaded.
```

If this line is missing, the plugin was not loaded — usually because the BepInEx version is wrong (must be **IL2CPP** 6.x) or the dll is in the wrong location.

### Uninstall

Delete `NpcCompass.dll`. The config file `BepInEx/config/com.local.npccompass.cfg` is optional to delete.

## Configuration

After the first launch, the config file is auto-generated. All options:

| Key | Default | Description |
|---|---|---|
| `[General] Enabled` | `true` | Whether the compass is shown at startup |
| `[Input] ToggleKey` | `F8` | Key to toggle the compass |
| `[Display] Anchor` | `MiddleRight` | Compass position. Options: `MiddleRight` / `MiddleLeft` / `TopRight` / `TopLeft` / `BottomRight` / `BottomLeft` / `TopCenter` |
| `[Display] MarginX` | `20` | Horizontal pixel offset from the anchor |
| `[Display] MarginY` | `20` | Vertical pixel offset from the anchor |
| `[Display] Radius` | `100` | Compass radius in pixels. **Requires game restart to take effect** |
| `[Display] MaxRange` | `30` | NPCs farther than this many world units (≈ meters) are not drawn |
| `[Display] MaxHeightDiff` | `3` | NPCs whose Y position differs from the player by more than this are not drawn (filters other floors) |

Changes apply on next game launch (except `Radius` which always requires a restart).

## Compatibility

- **Game**: Secret Flasher Manaka v1.1.3
- **BepInEx**: 6.0.0-be.735 (tested) — likely works with any bleeding-edge 6.x IL2CPP build
- **Unity**: 2022.3.62f2 (game runtime)

Future game updates should continue to work as long as `NpcManager.ExistNpcList` and `NpcController` interfaces remain unchanged.

## Troubleshooting

**Compass doesn't appear**

- Make sure the scene has NPCs (main menu / loading / cutscenes don't show NPCs)
- Try pressing F8 (it may have been toggled off last session)
- Check `BepInEx/LogOutput.log` for `NPC Compass` lines. If there's no `loaded` line, the plugin wasn't loaded — usually wrong BepInEx version.

**Compass is in the wrong position**

- Edit `Anchor` / `MarginX` / `MarginY` in the config file

**Too many NPCs on the compass, cluttered**

- Lower `MaxRange` (default 30, try 15–20)
- Lower `MaxHeightDiff` (default 3, try 2 on complex terrain)

**Want to see debug logs**

- Edit `BepInEx/config/BepInEx.cfg`, find the `[Logging.Console]` section, and add `Debug` to `LogLevels`
- Restart the game. You'll see F8 toggle feedback and a periodic NPC stats summary every 5 seconds.

## Privacy

- Fully local; no network access
- Does not read or write save files
- Collects no user data

## Known limitations

- Changing `Radius` requires a game restart (textures are generated at startup)
- `MaxHeightDiff` is a hard threshold; complex multi-level terrain (ramps, mezzanines) may filter incorrectly
- Does not show NPC names or status details — only position and orientation
