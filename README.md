# PosePad

[中文说明](README.zh-CN.md) | [日本語](README.ja-JP.md)

PosePad is a Dalamud plugin for Final Fantasy XIV that turns commonly used poses such as standing pose 1, 2, 3 and sitting pose 1, 2, 3, along with emotes and expressions, into a clean button panel for GPose, screenshots, performances, and roleplay workflows. Many animation mods rely on these poses, and typing timeline IDs by hand every time is cumbersome, so PosePad is built to make that workflow fast and direct. It also supports optional Penumbra integration so active emote mods can show up in a dedicated Mod Actions tab and be triggered directly.

## Features

- Common pose buttons for standing, sitting, lying, and other curated timeline-based poses
- Emote tab loaded from in-game data
- Expression tab separated from regular emotes
- Favorites for emotes and expressions
- Optional Penumbra integration that reads active emote mods into a Mod Actions tab
- Search and filtering
- Recent emotes list
- Cancel Action button to restore the actor to the default state
- GPose-friendly UI with optional auto-open on entering GPose
- English, Simplified Chinese, and Japanese UI

## Commands

- `/posepad`
- `/posspad`

## Scope

Included in the current release:

- Local pose and emote panel
- Local configuration
- GPose-safe timeline playback behavior
- Optional Penumbra integration for reading active emote mods and exposing one-click Mod Actions

Not included in the current release:

- Reliable automatic pose-variant splitting for every Penumbra mod
- Runtime dependency on Brio

## Development

Current local target:

- `Dalamud.NET.Sdk 15.0.0`
- Dalamud API `15`
- default dev hooks path: `$(AppData)\XIVLauncherCN\addon\Hooks\dev`

Build:

```powershell
dotnet build
dotnet build -c Release
```

Release package output:

```text
bin/Release/PosePad/latest.zip
```

## Loading As A Dev Plugin

1. Open `/xlsettings`
2. Go to `Experimental`
3. Add the full path to `bin/Debug/PosePad.dll`
4. Open `/xlplugins`
5. Enable PosePad from the dev plugin list
