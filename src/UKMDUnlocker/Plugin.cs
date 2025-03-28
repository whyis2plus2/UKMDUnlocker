﻿namespace UKMDUnlocker;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    // angry level loader does this, and I quite like it
    public const string PLUGIN_GUID = "com.whyis2plus2.UKMDUnlocker";
    public const string PLUGIN_NAME = "UKMDUnlocker";
    public const string PLUGIN_VERSION = "0.2.1";

    public const string DIF_NAME = "Ultrakill Must Die";
    public const string DIF_NAME_SHORT = "UKMD";


    /// <summary> The scene name of the main menu </summary>
    public const string MAIN_MENU_NAME = "b3e7f2f8052488a45b35549efb98d902";


    /// <summary> The "interactable" components of the difficulty select menu (mostly just difficulty buttons and infos) </summary>
    public Transform Interactables {private set; get;}

    public GameObject UKMDButton = null;
    public GameObject UKMDInfo = null;

    /// <summary> The current instance of the plugin, accessable by all parts of the code </summary>
    public static Plugin Instance;

    /// <summary> We need to have an instance of this in order to do patches </summary>
    readonly Harmony Harmony = new(PLUGIN_GUID);

    public ManualLogSource Log => Logger;

    // TODO: Figure out how to actually get the intro to work
    /// <summary> The scene name of the startup sequence/new save intro </summary>
    // private const string INTRO_NAME = "4f8ecffaa98c2614f89922daf31fa22d";

    private void Awake()
    {
        Instance = this;
        SceneManager.activeSceneChanged += OnSceneChange;

        CrossMod.BananasFix.Init();
        CrossMod.UltrapainFix.Init();

        Harmony.PatchAll();
        Log.LogInfo($"Loaded {PLUGIN_NAME}");
    }

    private void OnSceneChange(Scene last, Scene next)
    {
        LeaderboardProperties.Difficulties[5] = (next.name == MAIN_MENU_NAME)? DIF_NAME_SHORT : DIF_NAME;
        if (next.name != MAIN_MENU_NAME) return;

        var canvas = (from obj in next.GetRootGameObjects() where obj.name == "Canvas" select obj).First().transform;

        // difficulty buttons and difficulty infos
        Interactables = canvas.Find("Difficulty Select (1)/Interactables");

        // create the new UKMD button and Info
        AddInfo();
        AddButton();
    }

    /// <summary> Add the UKMD button and info to the difficulty select menu </summary>
    private void AddButton()
    {
        KeyValuePair<string, GameObject> FindElem(string name) =>
            new(name, Interactables.Find(name).gameObject);

        Log.LogInfo("Adding UKMD Button...");

        Dictionary<string, GameObject> buttons = new([
            FindElem("Casual Easy"), // Harmless
            FindElem("Casual Hard"), // Lenient
            FindElem("Standard"),
            FindElem("Violent"),
            FindElem("Brutal"),
            FindElem("V1 Must Die"), // Normal UKMD button
        ]);

        Dictionary<string, GameObject> infos = new([
            FindElem("Harmless Info"),
            FindElem("Lenient Info"),
            FindElem("Standard Info"),
            FindElem("Violent Info"),
            FindElem("Brutal Info"),
        ]);

        // clone the brutal button
        UKMDButton = Instantiate(buttons.GetValueSafe("Brutal"), Interactables);
        UKMDButton.GetComponent<DifficultySelectButton>().difficulty = 5;
        UKMDButton.transform.Find("Name").GetComponent<TMP_Text>().text = DIF_NAME.ToUpper();
        UKMDButton.transform.position = buttons.GetValueSafe("V1 Must Die").transform.position;
        UKMDButton.name = $"{DIF_NAME_SHORT} Button";

        // disable the original ukmd button so that it doesn't get in the way
        buttons.GetValueSafe("V1 Must Die").gameObject.SetActive(false);

        // the event triggers that the button uses to show/hide its description
        var ukmdTrigger = UKMDButton.GetComponent<EventTrigger>();

        // remove old triggers because those use Brutal's description instead of UKMD's description
        ukmdTrigger.triggers.Clear();

        // If the info hasn't been created yet, try to create it
        if (!UKMDInfo) AddInfo();

        // hide ukmd info if any of the other buttons are hovered over
        foreach (var button in buttons.Values) {
            var trigger = button.GetComponent<EventTrigger>();
            if (!trigger) continue;

            trigger.triggers.Add(
                Tools.CreateTriggerEntry(EventTriggerType.PointerEnter, _ => UKMDInfo.SetActive(false))
            );
        }

        // add new triggers to ukmd button
        ukmdTrigger.triggers.AddRange([
            Tools.CreateTriggerEntry(EventTriggerType.PointerEnter, _ =>
            {
                UKMDInfo.SetActive(true);
                foreach (var info in infos.Values) info.SetActive(false);
            }),

            Tools.CreateTriggerEntry(EventTriggerType.PointerExit,  _ => UKMDInfo.SetActive(false)),
            Tools.CreateTriggerEntry(EventTriggerType.PointerClick, _ => UKMDInfo.SetActive(false)),
        ]);

        // add ukmd button to the button activation sequence
        var activationSequence = Interactables.GetComponent<ObjectActivateInSequence>();
        activationSequence.objectsToActivate[14 /* index of vanilla ukmd button */] = UKMDButton;

        Log.LogInfo("Added UKMD Button");
    }

    private void AddInfo()
    {
        Log.LogInfo("Adding UKMD Info...");

        UKMDInfo = Instantiate(Interactables.Find("Brutal Info").gameObject, Interactables);
        UKMDInfo.name = $"{DIF_NAME_SHORT} Info";

        var ukmdTitle = UKMDInfo.transform.Find("Title (1)").GetComponent<TMP_Text>();

        // set the font size to 29 because if it's the default it'll span multiple lines
        ukmdTitle.fontSize = 29;
        ukmdTitle.text = $"--{DIF_NAME.ToUpper()}--";

        // set the description of UKMD
        UKMDInfo.transform.Find("Text").GetComponent<TMP_Text>().text = 
            """
            <color=yellow>The unfinished version of UKMD in the game's files.</color>

            <color=white>Fast and extremely aggresive enemies with very high damage.

            Quick thinking and a full arsenal are expected. Slip-ups are often fatal.</color>

            <b>Recommended for players who have achieved near mastery over the game and are looking for a fitting challenge.</b>
            """;

        Log.LogInfo("Added UKMD Info");
    }

    [HarmonyPatch(typeof(PrefsManager), MethodType.Constructor)]
    private static class PrefsManager_Ctor_Patch
    {
        static void Postfix(plog.Logger ___Log, ref Dictionary<string, Func<object, object>> ___propertyValidators)
        {
            ___propertyValidators.Remove("difficulty");
            ___propertyValidators.Add("difficulty", (value) =>
            {
                if (value is not int)
                {
                    ___Log.Warning("Difficulty value is not an int");
                    return 2; // standard
                }

                int difficulty = (int)value;
                if (difficulty < 0 || difficulty > 5)
                {
                    ___Log.Warning("Difficulty validation error");
                    return 5; // UKMD
                }

                return null;
            });
        }
    };
}