namespace UKMDUnlocker.CrossMod;

using BepInEx.Bootstrap;
using HarmonyLib;

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public static class UltrapainFix
{
    public static bool HasUltrapain => Chainloader.PluginInfos.ContainsKey(Ultrapain.Plugin.PLUGIN_GUID);
    public static Transform Button {private set; get;} = null;
    public static Transform Info {private set; get;} = null;
    
    static Plugin plugin => Plugin.Instance;

    public static void Init()
    {
        if (!HasUltrapain) return;

        plugin.Log.LogInfo("Detected Ultrapain");
        SceneManager.activeSceneChanged += OnSceneChange;
    }

    private static void OnSceneChange(Scene last, Scene next)
    {
        if (next.name != Plugin.MAIN_MENU_NAME) return;

        // find the difficulty button for BananasDifficulty
        foreach (Transform transform in plugin.Interactables)
        {
            if (transform.name != "Brutal(Clone)") continue;

            plugin.Log.LogInfo($"Found Brutal(Clone) with ID {transform.GetInstanceID()}");
            var nametr = transform.Find("Name");
            if (!nametr) continue;

            var name = nametr.GetComponent<TMP_Text>();
            if (name.text.ToLower().Contains("ultrapain"))
            {
                plugin.Log.LogInfo($"Found Ultrapain button (ID: {transform.GetInstanceID()})");
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
            if (title.text.ToLower().Contains("ultrapain"))
            {
                plugin.Log.LogInfo($"Found Ultrapain info (ID: {transform.GetInstanceID()})");
                Info = transform;
                break;
            }
        }

        Button.GetComponent<EventTrigger>().triggers.Add(
            Tools.CreateTriggerEntry(EventTriggerType.PointerEnter, _ => plugin.UKMDInfo.SetActive(false))
        );

        plugin.UKMDButton.GetComponent<EventTrigger>().triggers.Add(
            Tools.CreateTriggerEntry(EventTriggerType.PointerEnter, _ => Info.gameObject.SetActive(false))
        );
    }
}