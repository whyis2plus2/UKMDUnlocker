namespace UKMDUnlocker;

using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    // angry level loader does this, and I quite like it
    public const string PLUGIN_GUID = "whyis2plus2.UKMDUnlocker";
    public const string PLUGIN_NAME = "UKMDUnlocker";
    public const string PLUGIN_VERSION = "0.1.2";

    public static readonly string DifficultyName = GameDifficulty.UKMD.GetDifficultyName();

    public static bool HasBananaDifficulty { private set; get; } = false;

    /// <summary> We need to have an instance of this in order to do patches </summary>
	public static readonly Harmony Harmony = new(PLUGIN_GUID);

    public static ManualLogSource Log;

    private void Awake()
    {
        Log = Logger;
        HasBananaDifficulty = Chainloader.PluginInfos.ContainsKey("com.michi.BananaDifficulty");

        SceneManager.activeSceneChanged += OnSceneChange;

        Log.LogInfo("Loading UKMDUnlocker");
        if (HasBananaDifficulty)
        {
            Log.LogInfo("Detected BananasDifficulty");
            BananaDifficultyManager.Enabled = true;
        }

        Harmony.Patch(
            typeof(PrefsManager).GetConstructor(new Type[0]),
            postfix: new(typeof(PrefsManager_Ctor_Patch).GetMethod("Postfix", AccessTools.all))
        );

        Log.LogInfo("Loaded UKMDUnlocker");
    }

    private void OnSceneChange(Scene last, Scene current)
    {
        string titleSceneName = "b3e7f2f8052488a45b35549efb98d902";

        if (current.name == titleSceneName)
        {
            // difficulty buttons and difficulty infos
            Transform interactables = (from obj in SceneManager.GetActiveScene().GetRootGameObjects() where obj.name == "Canvas" select obj)
                .First().transform.Find("Difficulty Select (1)").Find("Interactables"); 

            // clone the brutal button
            var ukmdButton = Instantiate(interactables.Find("Brutal").gameObject, interactables);
            ukmdButton.GetComponent<DifficultySelectButton>().difficulty = 5;
            ukmdButton.transform.Find("Name").GetComponent<TMP_Text>().text = DifficultyName.ToUpper();
            ukmdButton.GetComponent<RectTransform>().position = new Vector2(30f, 157.5f);
            ukmdButton.name = $"{PLUGIN_NAME} UKMD";

            var ukmdInfo = Instantiate(interactables.Find("Brutal Info").gameObject, interactables);
            ukmdInfo.name = $"{PLUGIN_NAME} UKMD Info";

            var ukmdTitle = ukmdInfo.transform.Find("Title (1)").GetComponent<TMP_Text>();
            ukmdTitle.fontSize = 29 /* this is the largest font size that we can use without the text being split into more than one line */;
            ukmdTitle.text = $"--{DifficultyName.ToUpper()}--";

            // set the description of UKMD
            ukmdInfo.transform.Find("Text").GetComponent<TMP_Text>().text = 
                """
                <color=white>Extremely aggressive enemies and very high damage.

                Quick reflexes and extensive knowledge of the game are expected. Any mistake made is likely to be deadly.</color>

                <b>Recommended for players who have achieved near mastery over the game and are looking for a final challenge.</b>
                """;

            // give the user notice about stuff
            if (HasBananaDifficulty)
                ukmdInfo.transform.Find("Text").GetComponent<TMP_Text>().text +=
                    "\n\n<color=yellow>Due to technical reasons, this uses the same save slot as Bananas Difficulty</color>";


            // the event triggers that the button uses to show/hide its description
            var ukmdTrigger = ukmdButton.GetComponent<EventTrigger>();

            // remove old triggers because those use Brutal's description instead of UKMD's description
            ukmdTrigger.triggers.Clear();
            
            // show ukmd info on mouse hover
            EventTrigger.Entry pointerEnter = new() { eventID = EventTriggerType.PointerEnter };
            pointerEnter.callback.AddListener((data) => ukmdInfo.SetActive(true));

            // hide ukmd info on mouse exit
            EventTrigger.Entry pointerExit = new() { eventID = EventTriggerType.PointerExit };
            pointerExit.callback.AddListener((data) => ukmdInfo.SetActive(false));

            // hide ukmd info when clicked
            EventTrigger.Entry onClick = new() { eventID = EventTriggerType.PointerClick };
            onClick.callback.AddListener((data) =>
            {
                if (HasBananaDifficulty) BananaDifficultyManager.Enabled = false;
                ukmdInfo.SetActive(false);
            });

            // add new triggers to ukmd button
            ukmdTrigger.triggers.Add(pointerEnter);
            ukmdTrigger.triggers.Add(pointerExit);
            ukmdTrigger.triggers.Add(onClick);

            // add ukmd button to the button activation sequence
            var activationSequence = interactables.GetComponent<ObjectActivateInSequence>();
            activationSequence.objectsToActivate[14 /* index of ultrakill's ukmd button */] = ukmdButton;

            // deactivate the normal ukmd button so that it doesn't get in the way
            interactables.Find("V1 Must Die").gameObject.SetActive(false);

            // if banana difficulty is avaliable then make any neccessary tweaks to its button and 
            if (HasBananaDifficulty) BananaDifficultyManager.ModifyButton(interactables);
        }
    }   
}

[HarmonyPatch(typeof(PrefsManager), MethodType.Constructor)]
static class PrefsManager_Ctor_Patch {
    // documentation comment because I hate when harmony patches go unexplained
    /// <summary> Postfix that applies to the constructor of PrefsManager, esuring that no instance of said class does a bounds check for difficulty </summary>
    static void Postfix(ref Dictionary<string, Func<object, object>> ___propertyValidators)
    {
        ___propertyValidators.Remove("difficulty");
    }
};
