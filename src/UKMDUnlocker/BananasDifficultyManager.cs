// namespace UKMDUnlocker;

// using HarmonyLib;

// using Discord;
// using TMPro;
// using UnityEngine.EventSystems;
// using UnityEngine;

// using BananaDifficulty;

// public static class BananaDifficultyManager
// { 
//     private static bool realEnabled = false;
// 	public static bool Enabled
//     {
//         set
//         {
//             realEnabled = value;

//             if (value)
//             {
//                 Plugin.Log.LogInfo("Enabling bananas difficulty");
//                 Plugin.harmony.Unpatch(
//                     typeof(BananaDifficultyPlugin).GetMethod("CanUseIt", AccessTools.all),
//                     typeof(BananaDifficultyPlugin_CanUseIt_Patch).GetMethod("Postfix", AccessTools.all)
//                 );

//                 Plugin.harmony.Unpatch(
//                     typeof(DiscordController).GetMethod("SendActivity", AccessTools.all),
//                     typeof(DiscordController_SendActivity_Patch).GetMethod("Prefix", AccessTools.all)
//                 );

//                 Plugin.harmony.Patch(
//                     typeof(DifficultyTitle).GetMethod("Check", AccessTools.all),
//                     postfix: new(typeof(DifficultyTitle_Check_Patch).GetMethod("Postfix", AccessTools.all))
//                 );
//                 return;
//             }

//             Plugin.Log.LogInfo("Disabling bananas difficulty");
//             Plugin.harmony.Patch(
//                 typeof(BananaDifficultyPlugin).GetMethod("CanUseIt", AccessTools.all),
//                 postfix: new(typeof(BananaDifficultyPlugin_CanUseIt_Patch).GetMethod("Postfix", AccessTools.all))
//             );

//             Plugin.harmony.Patch(
//                 typeof(DiscordController).GetMethod("SendActivity", AccessTools.all),
//                 prefix: new(typeof(DiscordController_SendActivity_Patch).GetMethod("Prefix", AccessTools.all))
//             );

//             Plugin.harmony.Unpatch(
//                 typeof(DifficultyTitle).GetMethod("Check", AccessTools.all),
//                 typeof(DifficultyTitle_Check_Patch).GetMethod("Postfix", AccessTools.all)
//             );
//         }

//         get
//         {
//             return realEnabled;
//         }
//     }

//     public static void ModifyButton()
//     {
//         Plugin.Log.LogInfo("Adding trigger to bananas difficulty");
//         var bananaTrigger = Plugin.Interactables.Find("Brutal(Clone)").GetComponent<EventTrigger>();
//         var bananaInfo = Plugin.Interactables.Find("Brutal Info(Clone)");

//         bananaInfo.Find("Text").GetComponent<TextMeshProUGUI>().text += 
//             $"\n\n<color=yellow>Due to technical reasons, this uses the same save slot as {Plugin.DifficultyName}</color>";

//         EventTrigger.Entry repatchBanana = new() { eventID = EventTriggerType.PointerClick };
//         repatchBanana.callback.AddListener((data) => Enabled = true);
//         bananaTrigger.triggers.Add(repatchBanana);
//     }

// 	static class BananaDifficultyPlugin_CanUseIt_Patch
//     {
//         static void Postfix(ref bool __result)
//         {
//             __result = false;
//         }
// 	};
    
//     static class DiscordController_SendActivity_Patch
//     {
//         static void Prefix(ref Activity ___cachedActivity)
//         {
//             if (___cachedActivity.State != "DIFFICULTY: BANANAS") return;
//             ___cachedActivity.State = "DIFFICULTY: UKMD";
//         }
//     }
// }