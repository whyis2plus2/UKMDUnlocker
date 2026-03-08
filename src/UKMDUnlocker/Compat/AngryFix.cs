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
    public static bool hasAngry => Chainloader.PluginInfos.ContainsKey("com.eternalUnion.angryLevelLoader");
    
    static Plugin plugin => Plugin.Instance;

    public static void Init()
    {
        if (!hasAngry) return;

        plugin.logger.LogInfo($"Detected com.eternalUnion.angryLevelLoader");
        plugin.HarmonyPatches.PatchAll(typeof(Patches));
    }

    static class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AngryLevelLoader.Plugin), "Start")]
        public static void AddUKMDToDifficultyList(ref List<string> ___difficultyList)
        {
            ___difficultyList.Add(Plugin.DIF_NAME_SHORT);
            plugin.logger.LogInfo("Added UKMD to Angry's Difficulty List");
        }
    }
}