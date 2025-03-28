namespace UKMDUnlocker.CrossMod;

using BananaDifficulty;
using BepInEx.Bootstrap;
using HarmonyLib;

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

using Discord;

public static class BananasFix
{
    public static bool HasBananas => Chainloader.PluginInfos.ContainsKey("com.michi.BananaDifficulty");
    public static bool IsEnabled {private set; get;} = false;
    public static Transform Button {private set; get;} = null;
    public static Transform Info {private set; get;} = null;
    
    static readonly Harmony Harmony = new($"{Plugin.PLUGIN_GUID}.crossmod.bananas");
    static Plugin plugin => Plugin.Instance;

    public static void Init()
    {
        if (!HasBananas) return;

        plugin.Log.LogInfo("Detected BananasDifficulty");
        SceneManager.activeSceneChanged += OnSceneChange;
    }

    public static void Enable()
    {
        if (!HasBananas) return;

        Harmony.UnpatchSelf();
        IsEnabled = true;
    }

    public static void Disable()
    {
        if (!HasBananas) return;

        // I don't know how else to prevent these from being patched in by patch all
        Harmony.PatchOne(typeof(BananaDifficultyPlugin), "CanUseIt", typeof(BananaDifficultyPlugin_CanUseIt_Patch));
        Harmony.PatchOne(typeof(DifficultyTitle), "Check", typeof(DifficultyTitle_Check_Patch));
        Harmony.PatchOne(typeof(DiscordController), "SendActivity", typeof(DiscordController_SendActivity_Patch), true);

        IsEnabled = false;
    }

    private static void OnSceneChange(Scene last, Scene next)
    {
        if (next.name != Plugin.MAIN_MENU_NAME) return;

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
            Tools.CreateTriggerEntry(EventTriggerType.PointerClick, _ => Enable())
        );

        plugin.UKMDButton.GetComponent<EventTrigger>().triggers.Add(
            Tools.CreateTriggerEntry(EventTriggerType.PointerClick, _ => Disable())
        );

        Info.Find("Text").GetComponent<TMP_Text>().text += $"\n\n<#ff0>This uses the same save data as {Plugin.DIF_NAME}";
    }

    private static class BananaDifficultyPlugin_CanUseIt_Patch
    {
        public static void Postfix(ref bool __result)
        {
            __result = false;
        }
    }

    private class DifficultyTitle_Check_Patch
    {
        public static void Postfix(ref TMP_Text ___txt2)
        {
            if (!___txt2.text.Contains("BANANAS DIFFICULTY")) return;
            ___txt2.text = ___txt2.text.Replace("BANANAS DIFFICULTY", Plugin.DIF_NAME.ToUpper());
        }
    }

    private static class DiscordController_SendActivity_Patch
    {
        public static void Prefix(ref Activity ___cachedActivity)
        {
            if (___cachedActivity.State != "DIFFICULTY: BANANAS") return;
            ___cachedActivity.State = $"DIFFICULTY {Plugin.DIF_NAME_SHORT}";
        }
    }
}