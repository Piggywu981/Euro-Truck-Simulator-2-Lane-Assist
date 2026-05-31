### ETS2LA C# 3.2.10
* Fix notifications not showing up in UI.
* Better Linux styling.
* Removed `Avalonia.Diagnostics` as it no longer works on Avalonia 12.
* `ETS2LA.Backend` now uses Ids for plugins. These are necessary to keep internal references consistent even when in the future the plugin names might change.
* Added `Version`, `SupportedETS2LA`, `Icon` and `Dependencies` fields to plugin info. 
* Redesigned the plugin manager. This will continue to get updates in the future.

**WARNING:** ETS2LA C# on Linux requires Linux specific SDKs. These can be found on the closed beta Discord, as they aren't yet included in ETS2LA C#.

**IMPORTANT:** If you're not in the ETS2LA beta program, please take a look at https://ets2la.com/download. This version is not what you're looking for!