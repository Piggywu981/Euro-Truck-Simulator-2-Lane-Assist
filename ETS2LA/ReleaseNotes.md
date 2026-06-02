### ETS2LA C# 3.3.0
* Implement `LibraryPlugin`. These are plugins that have no logic, but they provide libraries for others. These are necessary as all ETS2LA plugins are in separated memory spaces, where only the main load context is shared.
* Window now has `ClipToBounds` on linux, corners are now properly rounded.
* Core Plugins got a major rewrite, it's now split into two plugins and one library. This is preliminary work to get ACC done later on. 

**WARNING:** ETS2LA C# on Linux requires Linux specific SDKs. These can be found on the closed beta Discord, as they aren't yet included in ETS2LA C#.

**IMPORTANT:** If you're not in the ETS2LA beta program, please take a look at https://ets2la.com/download. This version is not what you're looking for!