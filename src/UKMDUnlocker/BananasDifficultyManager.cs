namespace UKMDUnlocker;

using HarmonyLib;
using System;
using System.Collections.Generic;

using BananaDifficulty;
using UnityEngine.EventSystems;
using UnityEngine;

public static class BananaDifficultyManager { 
    static bool realEnabled = false;
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
                    typeof(BananaDifficultyPlugin).GetMethod(nameof(BananaDifficultyPlugin.CanUseIt), AccessTools.all),
                    typeof(BananaDifficultyPlugin_Patch).GetMethod("CanUseIt", AccessTools.all)
                );
                return;
            }

            Plugin.Log.LogInfo("Disabling bananas difficulty");
            Plugin.Harmony.Patch(
                typeof(BananaDifficultyPlugin).GetMethod(nameof(BananaDifficultyPlugin.CanUseIt), AccessTools.all),
                postfix: new(typeof(BananaDifficultyPlugin_Patch).GetMethod("CanUseIt", AccessTools.all))
            );
        }

        get
        {
            return realEnabled;
        }
    }

    public static void AddTriggers(Transform parent)
    {
        if (!Plugin.HasBananaDifficulty) return;

        Plugin.Log.LogInfo("Adding trigger to bananas difficulty");
        var bananaTrigger = parent.Find("Brutal(Clone)").GetComponent<EventTrigger>();

        EventTrigger.Entry repatchBanana = new() { eventID = EventTriggerType.PointerClick };
        repatchBanana.callback.AddListener((data) => Enabled = true);
        bananaTrigger.triggers.Add(repatchBanana);
    }

	[HarmonyPatch(typeof(BananaDifficultyPlugin))]
	static class BananaDifficultyPlugin_Patch {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BananaDifficultyPlugin.CanUseIt))]
        static void CanUseIt(ref bool __result)
        {
            __result = false;
        }
	};
}