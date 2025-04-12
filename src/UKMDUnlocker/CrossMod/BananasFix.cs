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
    
    static Plugin plugin => Plugin.Instance;
    static readonly Harmony harmony = new($"{Plugin.PLUGIN_GUID}.CrossMod.BananasFix");

    public static void Init()
    {
        if (!HasBananas) return;

        plugin.Log.LogInfo("Detected BananasDifficulty");

        harmony.PatchAll(typeof(Patches));
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
            Tools.CreateTriggerEntry(EventTriggerType.PointerClick, _ => IsEnabled = true)
        );

        plugin.UKMDButton.GetComponent<EventTrigger>().triggers.Add(
            Tools.CreateTriggerEntry(EventTriggerType.PointerClick, _ => IsEnabled = false)
        );

        Info.Find("Text").GetComponent<TMP_Text>().text += $"\n\n<#ff0>This uses the same save data as {Plugin.DIF_NAME}";
    }

    // Turret is the name of the class for Sentries
    [HarmonyPatch(typeof(Turret), "Start")]
    static class Turret_Start_Patch
    {
        [HarmonyPatch]
        static void Postfix(ref float ___maxAimTime)
        {
            if (!IsEnabled) ___maxAimTime = 3.0f;
        }
    }

    static class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BananaDifficultyPlugin), "CanUseIt")]
        static void DisableBananas(ref bool __result)
        {
            if (!IsEnabled) __result = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DifficultyTitle), "Check")]
        static void RestoreUKMDTitle(ref TMP_Text ___txt2)
        {
            if (IsEnabled) return;
            if (!___txt2.text.Contains("BANANAS DIFFICULTY")) return;
            ___txt2.text = ___txt2.text.Replace("BANANAS DIFFICULTY", Plugin.DIF_NAME.ToUpper());
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DiscordController), "SendActivity")]
        static void RestoreUKMDActivity(ref Activity ___cachedActivity)
        {
            if (IsEnabled) return;
            if (___cachedActivity.State != "DIFFICULTY: BANANAS") return;
            ___cachedActivity.State = $"DIFFICULTY {Plugin.DIF_NAME_SHORT}";
        }

        // Turret is the name of the class for Sentries
        /// <summary> Bananas difficulty removes sentry cooldowns on all difficulties, so I have to fix that </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Turret), "Start")]
        static void FixSentryCooldowns(ref float ___maxAimTime, GameDifficulty ___difficulty)
        {
            // yeah, this is fine
            if (IsEnabled && ___difficulty == GameDifficulty.UKMD) return;

            ___maxAimTime = ___difficulty switch
            {
                GameDifficulty.Harmless => 7.5f,
                GameDifficulty.Lenient  => 5f,
                GameDifficulty.Standard => 5f,
                GameDifficulty.Violent  => 4f,
                _ => 3f
            };
        }
    }
}