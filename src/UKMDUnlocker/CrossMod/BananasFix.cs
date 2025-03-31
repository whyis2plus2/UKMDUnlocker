namespace UKMDUnlocker.CrossMod;

using BananaDifficulty;
using BepInEx.Bootstrap;
using HarmonyLib;

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using Discord;
using System;

public static class BananasFix
{
    public static bool HasBananas => Chainloader.PluginInfos.ContainsKey("com.michi.BananaDifficulty");
    public static bool IsEnabled {private set; get;} = false;
    public static Transform Button {private set; get;} = null;
    public static Transform Info {private set; get;} = null;
    
    // static readonly Harmony Harmony = new($"{Plugin.PLUGIN_GUID}.crossmod.bananas");
    static Plugin plugin => Plugin.Instance;

    public static void Init()
    {
        if (!HasBananas) return;

        plugin.Log.LogInfo("Detected BananasDifficulty");
        SceneManager.activeSceneChanged += (_, _) => OnSceneChange();
    }

    private static void OnSceneChange()
    {
        if (SceneHelper.CurrentScene != "Main Menu") return;

        // bananas difficulty doesn't keep track of its button or infos, so we have to find them manually
        // find the difficulty button for BananasDifficulty
        foreach (Transform transform in plugin.Interactables)
        {
            if (transform.name != "Brutal(Clone)") continue;

            plugin.Log.LogInfo($"Found Brutal(Clone) with ID {transform.GetInstanceID()}");
            var nametr = transform.Find("Name");
            if (!nametr) continue;

            var name = nametr.GetComponent<TMP_Text>();
            if (name.text.ToLower().Contains("banana"))
            {
                plugin.Log.LogInfo($"Found BananasDifficulty button (ID: {transform.GetInstanceID()})");
                Button = transform;
                break;
            }
        }

        // find the difficulty info for BananasDifficulty
        foreach (Transform transform in plugin.Interactables)
        {
            if (transform.name != "Brutal Info(Clone)") continue;

            plugin.Log.LogInfo($"Found Brutal Info(Clone) with ID {transform.GetInstanceID()}");
            var titletr = transform.Find("Title (1)");
            if (!titletr) continue;

            var title = titletr.GetComponent<TMP_Text>();
            if (title.text.ToLower().Contains("banana"))
            {
                plugin.Log.LogInfo($"Found BananasDifficulty info (ID: {transform.GetInstanceID()})");
                Info = transform;
                break;
            }
        }

        Button.GetComponent<EventTrigger>().triggers.Add(
            Tools.CreateTriggerEntry(EventTriggerType.PointerClick, _ => IsEnabled = true)
        );

        plugin.UKMDButton.GetComponent<EventTrigger>().triggers.Add(
            Tools.CreateTriggerEntry(EventTriggerType.PointerClick, _ => IsEnabled = false)
        );

        Info.Find("Text").GetComponent<TMP_Text>().text += $"\n\n<#ff0>This uses the same save data as {Plugin.DIF_NAME}";
    }

    [HarmonyPatch(typeof(BananaDifficultyPlugin), "CanUseIt")]
    private static class BananaDifficultyPlugin_CanUseIt_Patch
    {
        static bool Prepare() => HasBananas;
        static void Postfix(ref bool __result)
        {
            if (!IsEnabled) __result = false;
        }
    }

    [HarmonyPatch(typeof(DifficultyTitle), "Check")]
    private class DifficultyTitle_Check_Patch
    {
        static bool Prepare() => HasBananas;
        static void Postfix(ref TMP_Text ___txt2)
        {
            if (IsEnabled) return;
            if (!___txt2.text.Contains("BANANAS DIFFICULTY")) return;
            ___txt2.text = ___txt2.text.Replace("BANANAS DIFFICULTY", Plugin.DIF_NAME.ToUpper());
        }
    }

    [HarmonyPatch(typeof(DiscordController), "SendActivity")]
    private static class DiscordController_SendActivity_Patch
    {
        static bool Prepare() => HasBananas;
        static void Prefix(ref Activity ___cachedActivity)
        {
            if (IsEnabled) return;
            if (___cachedActivity.State != "DIFFICULTY: BANANAS") return;
            ___cachedActivity.State = $"DIFFICULTY {Plugin.DIF_NAME_SHORT}";
        }
    }
}