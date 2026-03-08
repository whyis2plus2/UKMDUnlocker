namespace UKMDUnlocker;

using System;
using System.Collections.Generic;
using System.Linq;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using static BepInEx.BepInDependency;

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
[BepInDependency(AngryLevelLoader.Plugin.PLUGIN_GUID, Flags: DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    // angry level loader does this, and I quite like it
    public const string PLUGIN_GUID = "com.whyis2plus2.UKMDUnlocker";
    public const string PLUGIN_NAME = "UKMDUnlocker";
    public const string PLUGIN_VERSION = "0.3.0";

    public const string DIF_NAME = "Ultrakill Must Die";
    public const string DIF_NAME_SHORT = "UKMD";

    /// <summary> The current instance of the plugin, accessable by all parts of the code </summary>
    public static Plugin Instance;

    /// <summary> The "interactable" components of the difficulty select menu (mostly just difficulty buttons and infos) </summary>
    public Transform Interactables {private set; get;}

    /// <summary> Easy and convenient variable for accessing the Canvas </summary>
    public Transform Canvas {private set; get;}

    public GameObject UKMDButton = null;
    public GameObject UKMDInfo = null;

    /// <summary> Public version of the Logger so that the rest of the mod can acess it </summary>
    public ManualLogSource logger => Logger;

    /// <summary> We need to have an instance of this in order to do patches </summary>
    public readonly Harmony HarmonyPatches = new(PLUGIN_GUID);

    void Awake()
    {
        Instance = this;
        SceneManager.activeSceneChanged += (_, _) => OnSceneChange();
        
        Compat.AngryFix.Init();

        HarmonyPatches.PatchAll(typeof(Patches));
        logger.LogInfo($"Loaded {PLUGIN_NAME}");
    }

    void OnSceneChange()
    {
        LeaderboardProperties.Difficulties[5] = (SceneHelper.CurrentScene == "Main Menu")? DIF_NAME_SHORT : DIF_NAME;
        if (SceneHelper.CurrentScene != "Main Menu") return;

        Canvas = (from obj in SceneManager.GetActiveScene().GetRootGameObjects() where obj.name == "Canvas" select obj).First().transform;

        // difficulty buttons and difficulty infos
        Interactables = Canvas.Find("Difficulty Select (1)/Interactables");

        // create the new UKMD button and Info
        AddInfo();
        AddButton();
    }

    /// <summary> Add the UKMD button and info to the difficulty select menu </summary>
    void AddButton()
    {
        KeyValuePair<string, GameObject> FindElem(string name) =>
            new(name, Interactables.Find(name).gameObject);

        logger.LogInfo("Adding UKMD Button...");

        Dictionary<string, GameObject> buttons = new([
            FindElem("Casual Easy"), // Harmless
            FindElem("Casual Hard"), // Lenient
            FindElem("Standard"),
            FindElem("Violent"),
            FindElem("Brutal"),
            FindElem("V1 Must Die"), // Real UKMD button
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
            Tools.CreateTriggerEntry(EventTriggerType.PointerClick, _ => {
                UKMDInfo.SetActive(false);
                Tools.Difficulty = GameDifficulty.UKMD;
            }),
            Tools.CreateTriggerEntry(EventTriggerType.PointerClick, _ =>
            {
                if (!Compat.AngryFix.hasAngry) return;
                Compat.AngryFix.angryDifficultyField.difficultyListValueIndex = 5;
                Compat.AngryFix.angryDifficultyField.difficultyListValue = DIF_NAME_SHORT;
                logger.LogInfo("Setting Angry difficulty to UKMD");
            }),
        ]);

        // add ukmd button to the button activation sequence
        var activationSequence = Interactables.GetComponent<ObjectActivateInSequence>();
        activationSequence.objectsToActivate[14 /* index of real ukmd button */] = UKMDButton;

        logger.LogInfo("Added UKMD Button");
    }

    void AddInfo()
    {
        logger.LogInfo("Adding UKMD Info...");

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

        logger.LogInfo("Added UKMD Info");
    }

    static class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PrefsManager), MethodType.Constructor)]
        static void AllowUKMD(plog.Logger ___Log, ref Dictionary<string, Func<object, object>> ___propertyValidators)
        {
            if (!___propertyValidators.ContainsKey("difficulty")) return; // Just in case

            // remove the old difficulty check
            ___propertyValidators.Remove("difficulty");

            // add a new one that forces difficulty to be in the range 0..5 instead of 0..4
            ___propertyValidators.Add("difficulty", (value) =>
            {
                if (value is not int)
                {
                    ___Log.Warning("Difficulty value is not an int");
                    return GameDifficulty.Standard;
                }

                var difficulty = (int)value;
                if (difficulty < 0 || difficulty > 5)
                {
                    ___Log.Warning("Difficulty validation error");
                    return GameDifficulty.UKMD;
                }

                // use the passed in value
                return value;
            });
        }
    };
}