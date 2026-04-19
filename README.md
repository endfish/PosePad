# PosePad

PosePad is a Dalamud plugin for quickly triggering common poses and emotes from a button panel instead of memorizing IDs.

## V1 Scope

V1 currently includes:

- A main window with tabs for common actions, emotes, and favorites
- Search and filtering
- A recent actions panel showing the latest 15 entries
- Local configuration for window state, favorites, recents, and UI language
- Optional UI language selection: English, Chinese, Japanese

V1 explicitly does **not** guarantee:

- Automatic action-mod detection
- Automatic Penumbra mod mapping
- Any runtime dependency on Penumbra or Brio

Penumbra and Brio were used only as public references for research and architecture direction. PosePad V1 runs without either plugin installed.

## Current Technical Target

This repository is currently aligned to the working local development environment used on this machine:

- `Dalamud.NET.Sdk 14.0.0`
- Dalamud API 14
- default hooks path: `$(AppData)\XIVLauncherCN\addon\Hooks\dev`

This matches the user's current working Dalamud plugin setup.

## Project Structure

The project follows the standard Dalamud plugin layout and stays close to `SamplePlugin`:

- `Plugin.cs`: plugin entrypoint, service wiring, command registration, window system
- `PosePad.json`: plugin metadata template
- `Configuration/`: plugin configuration and window state persistence
- `Models/`: action models and execution result types
- `Services/`: data loading, catalog building, execution services
- `Windows/`: main UI, settings UI, window state tracking
- `Integrations/Penumbra/`: optional integration extension point only
- `Data/CommonActions.json`: structured source for common timeline-based actions

## Local Development

### Prerequisites

- XIVLauncher and Dalamud installed
- A working Dalamud dev environment
- .NET SDK installed
- If your Dalamud dev hooks are not under the default AppData path, set `DALAMUD_HOME`

### Build

If your hooks are in the default `XIVLauncherCN` AppData location, a normal build is enough:

```powershell
dotnet build
```

If your Dalamud installation lives in a custom location, set `DALAMUD_HOME` first:

```powershell
$env:DALAMUD_HOME='C:\Users\<YourUser>\AppData\Roaming\XIVLauncherCN\addon\Hooks\dev'
dotnet build
```

Release build:

```powershell
$env:DALAMUD_HOME='C:\Users\<YourUser>\AppData\Roaming\XIVLauncherCN\addon\Hooks\dev'
dotnet build -c Release
```

## Loading in Dalamud Dev Plugin

1. Open Dalamud settings with `/xlsettings`
2. Go to `Experimental`
3. Add the full path to [PosePad.dll](</d:/Work/FF14/Git/PosePad/bin/Debug/PosePad.dll>)
4. Open `/xlplugins`
5. Go to `Dev Tools > Installed Dev Plugins`
6. Enable `PosePad`

Once loaded, use:

```text
/posepad
```

to open the main window.

## GPose Behavior

PosePad keeps its UI visible in GPose by default.

- This uses the public Dalamud UI builder GPose visibility flag
- It does not require Brio integration
- You can toggle it in the settings window

## Notes About Actions

- `Common Actions` are treated as timeline-based pose entries and are sourced from [docs/常用动作编号对应.md](</d:/Work/FF14/Git/PosePad/docs/常用动作编号对应.md>) through the structured `Data/CommonActions.json`
- `Emotes` are read from game data at runtime
- `Favorites` currently apply to emotes

## Known Limits in V1

- Common action names are currently curated from the provided document and not auto-localized
- Emote naming follows available game data in the current client environment
- Penumbra integration is a placeholder extension point only until a stable public changed-item mapping strategy is confirmed
