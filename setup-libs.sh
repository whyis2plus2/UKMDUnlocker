#!/usr/bin/env bash

# CONFIG
# usage: ./setup-libs.sh [ULTRAKILL_PATH] [BEPINEX_PATH]
ULTRAKILL_PATH=${1:-"$HOME/.steam/steam/steamapps/common/ULTRAKILL"}
BEPINEX_PATH=${2:-"$HOME/.var/app/io.github.ebkr.r2modman/config/r2modmanPlus-local/ULTRAKILL/profiles/Default/BepInEx"}
ULTRAKILL_MANAGED_DATA_PATH="$ULTRAKILL_PATH/ULTRAKILL_Data/Managed"

REQUIRED_ULTRAKILL_DLLS=(
    "Assembly-CSharp.dll"
    "plog.dll"
    "plog.unity.dll"
    "Unity.Addressables.dll"
    "Unity.ResourceManager.dll"
    "Unity.TextMeshPro.dll"
    "UnityEngine.AssetBundleModule.dll"
    "UnityEngine.UI.dll"
)

REQUIRED_BEPINEX_DLLS=(
    "core/BepInEx.dll"
    "core/0Harmony.dll"
)

mkdir -p ./lib/
mkdir -p ./lib/BepInEx/core

if [ ! -d "$ULTRAKILL_MANAGED_DATA_PATH" ]; then
    echo "Could not find directory '$ULTRAKILL_MANAGED_DATA_PATH'" 1>&2;
fi

if [ ! -d "$BEPINEX_PATH" ]; then
    echo "Could not find directory '$BEPINEX_PATH'" 1>&2;
fi

for it in "${REQUIRED_ULTRAKILL_DLLS[@]}"
do
    echo "Copying $it from ULTRAKILL/ULTRAKILL_Data/Managed/"
    cp "$ULTRAKILL_MANAGED_DATA_PATH/$it" "./lib/$it"
done

for it in "${REQUIRED_BEPINEX_DLLS[@]}"
do
    echo "Copying $it from BepInEx/"
    cp "$BEPINEX_PATH/$it" "./lib/BepInEx/$it"
done

echo "Finished copying files"
