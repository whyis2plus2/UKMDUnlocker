# UKMDUnlocker
UKMD Unlocker adds the ability to select the 'ULTRAKILL MUST DIE' difficulty on the difficulty select menu.<br>
This mod has support for [BananasDifficulty](https://thunderstore.io/c/ultrakill/p/bananastudio/BananasDifficulty/) since v0.1.2

## Installation
### This mod does not work on any ULTRAKILL version before patch 16
1. Download and install [BepInEx](https://thunderstore.io/c/ultrakill/p/BepInEx/BepInExPack/)
2. Download this and extract its contents to a folder in BepInEx/plugins

## Building
### This requires [dotnet 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0), if you do not have it, you must install it
1. Create a folder called "lib" in the root directory of the mod code
2. Add the following to it:
  - From BepInEx/core add:
    * BepInEx.dll
    * 0Harmony.dll
  - From ULTRAKILL_Data/Managed add:
    * Assembly-CSharp.dll
    * Unity.TextMeshPro.dll
    * UnityEngine.UI.dll
  - From [BananasDifficulty](https://thunderstore.io/c/ultrakill/p/bananastudio/BananasDifficulty/)
    * BananaDifficulty.dll
  - From [ULTRAPAIN Refueled](https://thunderstore.io/c/ultrakill/p/Kritzmaker/ULTRAPAIN_Refueled/)
    * Ultrapain_Refueled.dll

3. run "dotnet build"

The output file will be UKMDUnlocker.dll in bin/Debug/netstandard2.1/
