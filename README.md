# UKMDUnlocker
UKMD Unlocker adds the ability to select the 'ULTRAKILL MUST DIE' difficulty on the difficulty select menu.

## Installation
### NOTE: This mod does not work on any ULTRAKILL version before patch 16
1. Download and install [BepInEx](https://thunderstore.io/c/ultrakill/p/BepInEx/BepInExPack/)
2. Download this and extract its contents to a folder in BepInEx/plugins

## Building
#### Build-system dependencies:
  - gnu coreutils
  - gnu make
  - [The 7zip command line utility](www.7-zip.org)
  - [dotnet 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
#### Build steps:
1. Create a folder called "lib" in the root directory of the mod code
2. Add the following to it:
  - From BepInEx/core add:
    * BepInEx.dll
    * 0Harmony.dll
  - From ULTRAKILL_Data/Managed add:
    * Assembly-CSharp.dll
    * plog.dll
    * Unity.TextMeshPro.dll
    * UnityEngine.UI.dll
  - From [AngryLevelLoader](https://github.com/eternalUnion/AngryLevelLoader)
    * AngryLevelLoader/AngryLevelLoader.dll
    * AngryLevelLoader/AngryUiComponents.dll
  - From [PluginConfigurator](https://github.com/eternalUnion/UKPluginConfigurator)
    * PluginConfigurator/PluginConfigurator.dll

3. run `make release`

The output file will be `whyis2plus2-UKMDUnlocker-0.3.0.zip`
