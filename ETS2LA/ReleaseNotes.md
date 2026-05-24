### ETS2LA C# 3.2.5
* Created a new toggle element for use in UI.
* Implemented `Display` settings page in the UI.
* Implemented overlay framerate limiting. By default the overlay will now limit itself to 30fps. This cuts ETS2LA's CPU usage by around 60-70% on an R7 5800x3d.
* Added a setting for changing the maximum AR rendering distance, default is now 150m.
* Added a setting to disable AR rendering entirely.
* Implemented new `UnitConversion` class, as well as a setting to change default units. Once ETS2LA starts adding UI, all displayed values will follow this setting.

**WARNING:** ETS2LA C# on Linux requires Linux specific SDKs. These can be found on the closed beta Discord, as they aren't yet included in ETS2LA C#.

**IMPORTANT:** If you're not in the ETS2LA beta program, please take a look at https://ets2la.com/download. This version is not what you're looking for!