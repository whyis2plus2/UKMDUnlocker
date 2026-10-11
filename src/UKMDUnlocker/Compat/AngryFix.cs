namespace UKMDUnlocker.Compat;

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AngryLevelLoader.Fields;
using AngryLevelLoader.Managers;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class AngryFix {
    public static DifficultyField angryDifficultyField => hasAngry ? ConfigManager.difficultyField : null;
    public static bool hasAngry => Chainloader.PluginInfos.ContainsKey(AngryLevelLoader.Plugin.PLUGIN_GUID);
    public static int ukmdIdx;

    static Plugin plugin => Plugin.Instance;

    public static void Init() {
        if (!hasAngry) return;

        plugin.logger.LogInfo($"Detected {AngryLevelLoader.Plugin.PLUGIN_GUID}");
        plugin.HarmonyPatches.PatchAll(typeof(Patches));
    }

    static class Patches {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AngryDifficultyManager), "Init")]
        public static void AddUKMDToDifficultyList(ref List<AngryDifficulty> ___difficulties) {
            ukmdIdx = ___difficulties.Count;
            ___difficulties.Add(new(Plugin.DIF_NAME_SHORT, 5));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AngryDifficultyManager), "SetDifficultyFromPrefs")]
        public static bool AllowLoadingUKMDFromPrefs() {
            if (Tools.Difficulty != GameDifficulty.UKMD) return true;
            ConfigManager.difficultyField.difficultyListValueIndex = ukmdIdx;
            return false;
        }
    }
}
