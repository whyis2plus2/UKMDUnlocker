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
    public static bool BananasEnabled {private set; get;} = false;
    public static Transform Button {private set; get;} = null;
    public static Transform Info {private set; get;} = null;
    
    static Plugin plugin => Plugin.Instance;

    public static void Init()
    {
        if (!HasBananas) return;

        plugin.Log.LogInfo("Detected BananasDifficulty");

        plugin.HarmonyPatches.PatchAll(typeof(Patches));
        SceneManager.activeSceneChanged += (_, _) => OnSceneChange();
    }

    static void OnSceneChange()
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
            Tools.CreateTriggerEntry(EventTriggerType.PointerClick, _ => BananasEnabled = true)
        );

        plugin.UKMDButton.GetComponent<EventTrigger>().triggers.Add(
            Tools.CreateTriggerEntry(EventTriggerType.PointerClick, _ => BananasEnabled = false)
        );

        Info.Find("Text").GetComponent<TMP_Text>().text += $"\n\n<#ff0>This uses the same save data as {Plugin.DIF_NAME}";
    }

    static class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BananaDifficultyPlugin), "CanUseIt")]
        static void DisableBananas(ref bool __result)
        {
            BananasEnabled = __result &= BananasEnabled;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DifficultyTitle), "Check")]
        static void RestoreUKMDTitle(ref TMP_Text ___txt2)
        {
            if (BananasEnabled) return;
            if (!___txt2.text.Contains("BANANAS DIFFICULTY")) return;
            ___txt2.text = ___txt2.text.Replace("BANANAS DIFFICULTY", Plugin.DIF_NAME.ToUpper());
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DiscordController), "SendActivity")]
        static void RestoreUKMDActivity(ref Activity ___cachedActivity)
        {
            if (BananasEnabled) return;
            if (___cachedActivity.State != "DIFFICULTY: BANANAS") return;
            ___cachedActivity.State = $"DIFFICULTY {Plugin.DIF_NAME_SHORT}";
        }
    }
}