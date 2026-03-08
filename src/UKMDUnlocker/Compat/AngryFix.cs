namespace UKMDUnlocker.Compat;

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AngryLevelLoader.Fields;
using AngryUiComponents;
using BepInEx.Bootstrap;
using HarmonyLib;
using PluginConfig.API.Fields;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class AngryFix
{
    public static DifficultyField angryDifficultyField => hasAngry? AngryLevelLoader.Plugin.difficultyField : null;
    public static bool hasAngry => Chainloader.PluginInfos.ContainsKey(AngryLevelLoader.Plugin.PLUGIN_GUID);
    
    static Plugin plugin => Plugin.Instance;

    public static void Init()
    {
        if (!hasAngry) return;

        plugin.logger.LogInfo($"Detected {AngryLevelLoader.Plugin.PLUGIN_NAME}");

        plugin.HarmonyPatches.PatchAll(typeof(Patches));
    }

    static class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AngryLevelLoader.Plugin), "Start")]
        public static void AddUKMDToDifficultyList(ref List<string> ___difficultyList)
        {
            ___difficultyList.Add(Plugin.DIF_NAME_SHORT);
            plugin.logger.LogInfo("Attempted to add UKMD to Angry's Difficulty List");
        }
    }
}