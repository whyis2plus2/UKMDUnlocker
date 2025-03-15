namespace UKMDUnlocker;

using HarmonyLib;
using System;
using System.Collections.Generic;

using BananaDifficulty;
using UnityEngine.EventSystems;
using UnityEngine;
using Discord;
using TMPro;

public static class BananaDifficultyManager { 
    private static bool realEnabled = false;
	public static bool Enabled
    {
        set
        {
            if (!Plugin.HasBananaDifficulty) return;

            realEnabled = value;
            Plugin.Log.LogInfo($"AllowBananaDifficulty set to {value}");

            if (value)
            {
                Plugin.Log.LogInfo("Enabling bananas difficulty");
                Plugin.Harmony.Unpatch(
                    typeof(BananaDifficultyPlugin).GetMethod("CanUseIt", AccessTools.all),
                    typeof(BananaDifficultyPlugin_CanUseIt_Patch).GetMethod("Postfix", AccessTools.all)
                );

                Plugin.Harmony.Unpatch(
                    typeof(DiscordController).GetMethod("SendActivity", AccessTools.all),
                    typeof(DiscordController_SendActivity_Patch).GetMethod("Prefix", AccessTools.all)
                );

                Plugin.Harmony.Patch(
                    typeof(DifficultyTitle).GetMethod("Check", AccessTools.all),
                    postfix: new(typeof(DifficultyTitle_Check_Patch).GetMethod("Postfix", AccessTools.all))
                );
                return;
            }

            Plugin.Log.LogInfo("Disabling bananas difficulty");
            Plugin.Harmony.Patch(
                typeof(BananaDifficultyPlugin).GetMethod("CanUseIt", AccessTools.all),
                postfix: new(typeof(BananaDifficultyPlugin_CanUseIt_Patch).GetMethod("Postfix", AccessTools.all))
            );

            Plugin.Harmony.Patch(
                typeof(DiscordController).GetMethod("SendActivity", AccessTools.all),
                prefix: new(typeof(DiscordController_SendActivity_Patch).GetMethod("Prefix", AccessTools.all))
            );

            Plugin.Harmony.Unpatch(
                typeof(DifficultyTitle).GetMethod("Check", AccessTools.all),
                typeof(DifficultyTitle_Check_Patch).GetMethod("Postfix", AccessTools.all)
            );
        }

        get
        {
            return realEnabled;
        }
    }

    public static void ModifyButton(Transform parent)
    {
        if (!Plugin.HasBananaDifficulty) return;

        Plugin.Log.LogInfo("Adding trigger to bananas difficulty");
        var bananaTrigger = parent.Find("Brutal(Clone)").GetComponent<EventTrigger>();
        var bananaInfo = parent.Find("Brutal Info(Clone)");

        bananaInfo.Find("Text").GetComponent<TMP_Text>().text += 
            "\n\n<color=yellow>Due to technical reasons, this uses the same save slot as ULTRAKILL MUST DIE</color>";

        EventTrigger.Entry repatchBanana = new() { eventID = EventTriggerType.PointerClick };
        repatchBanana.callback.AddListener((data) => Enabled = true);
        bananaTrigger.triggers.Add(repatchBanana);
    }

	[HarmonyPatch(typeof(BananaDifficultyPlugin), "CanUseIt")]
	static class BananaDifficultyPlugin_CanUseIt_Patch {
        static void Postfix(ref bool __result)
        {
            __result = false;
        }
	};
    
    [HarmonyPatch(typeof(DiscordController), "SendActivity")]
    static class DiscordController_SendActivity_Patch
    {
        static void Prefix(ref Activity ___cachedActivity)
        {
            if (___cachedActivity.State != "DIFFICULTY: BANANAS") return;
            ___cachedActivity.State = "DIFFICULTY: UKMD";
        }
    }
}