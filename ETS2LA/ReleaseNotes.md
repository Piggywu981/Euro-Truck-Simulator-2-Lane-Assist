### ETS2LA C# 3.4.15
- Updated TruckLib to the latest release. This fixes issues with ETS2LA thinking the road is to the side of where it actually is.
- Libraries are now also using `ShadowCopy` to ensure they can be updated without causing a crash.
- When the speedlimit changes ETS2LA will now save your current offset. For example: `64 -> 84` instead of `64 -> 80`.
- Fixed the overlay moving to the far left monitor on Linux. There will be an update to automatically move the over to the monitor ETS2 is on eventually

---
<!-- Content inside ETS2LA will be cutoff at the line above, do not place lines inside the changelog. -->

> [!CAUTION]
> 如果您来自中国，可访问 [https://cnb.cool/ETS2LA-CN/Euro-Truck-Simulator-2-Lane-Assist/-/releases/latest](https://cnb.cool/ETS2LA-CN/Euro-Truck-Simulator-2-Lane-Assist/-/releases/latest) 这是我们官方的中国镜像仓库，从该仓库下载和安装的文件会优先使用中国仓库进行更新；另外这是开源免费项目，如发现有倒卖/安装收费行为，中国镜像站运营负责人有权利封禁你的ip，如遭遇ip封禁请不要提请任何申诉，我们不会受理
<sub>The above notice is to make sure Chinese people can find their specific download as they can't download from GitHub without a VPN.</sub>

> [!WARNING]
> If you have issues with your **overlay being fully opaque**, make sure to reinstall your graphics card drivers.  
> **NVIDIA** - Set `OpenGL GDI Compatibility` to `Prefer Compatible` in the *NVIDIA App*.  
> **AMD** - We've yet to find a solution, if you have information please tell us. Some GPUs work some don't...

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