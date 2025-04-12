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
    public static Transform Button => Ultrapain.Plugin.currentDifficultyButton.transform;
    public static Transform Info => Ultrapain.Plugin.currentDifficultyInfoText.transform.parent;
    
    static Plugin plugin => Plugin.Instance;

    public static void Init()
    {
        if (!HasUltrapain) return;

        plugin.Log.LogInfo("Detected Ultrapain");
        SceneManager.activeSceneChanged += (_, _) => OnSceneChange();
    }

    static void OnSceneChange()
    {
        if (SceneHelper.CurrentScene != "Main Menu") return;

        Button.GetComponent<EventTrigger>().triggers.Add(
            Tools.CreateTriggerEntry(EventTriggerType.PointerEnter, _ => plugin.UKMDInfo.SetActive(false))
        );

        plugin.UKMDButton.GetComponent<EventTrigger>().triggers.Add(
            Tools.CreateTriggerEntry(EventTriggerType.PointerEnter, _ => Info.gameObject.SetActive(false))
        );
    }
}