### ETS2LA C# 3.4.0
- Move `ETS2LA.Telemetry` to `ETS2LA.Game.Telemetry`, then replace `ETS2LA.Telemetry` with our anonymous telemetry code.
- Update to latest `TruckLib`. Support for game version 1.60.
- Added support for installing SDKs via the settings. This includes 1.60 SDKs for both Windows and Linux.
  - There are some usability problems on Linux, it does work though. Please read the text on the page for details.
- **Drk** - Optimize memory maps in `ETS2LA.Game.SDK` and `ETS2LA.Game.Telemetry` to improve performance.
- **Drk** - Fix `NullReferenceException` in `ETS2LA.Overlay.AR`.
- **Drk** - Resolve Steam game installs using AppID instead of game name in `ETS2LA.Game`.
- **Drk** - Fix `ETS2LA.Audio` memory leak and broken loop condition.

---
<!-- Content inside ETS2LA will be cutoff at the line above, do not place lines inside the changelog. -->

> [!IMPORTANT]
> If you're not in the ETS2LA beta program, please take a look at https://ets2la.com/download. This version is not what you're looking for!


<sub>ETS2LA is version specific, make sure you use a supported version!  
Older versions are not kept compatible with server side changes.</sub>
<!-- Please include a link to the latest working version for each game version. -->
<!-- 1.59 and 1.60 share the same map data version -->
| Game Version  | ETS2LA Version |
| ------------- | -------------- |
| **1.60**      | [**≥ 3.4.0**](https://github.com/ETS2LA/Euro-Truck-Simulator-2-Lane-Assist/releases/latest) |
| **1.59**      | [**≥ 3.2.0**](https://github.com/ETS2LA/Euro-Truck-Simulator-2-Lane-Assist/releases/latest) |

<sub>If you're running your game in Proton, please install the Windows version inside the proton instance.  
Press **_Assets_** below to download the installer.</sub>
| Operating System  |      Installer File       |
| ----------------- | ------------------------- |
| Windows           | `ETS2LA-win-*.msi`        |
| Linux             | `ETS2LA-linux-*.AppImage` |
| MacOS             | `ETS2LA-macos-*.pkg`      |