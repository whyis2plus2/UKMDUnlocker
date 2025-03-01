namespace UKMDUnlocker;

using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary> Bootloader class needed to avoid destroying the mod by the game. </summary>
[BepInPlugin("whyis2plus2.UKMDUnlocker", "UKMDUnlocker", "0.1.0")]
public class PluginLoader : BaseUnityPlugin
{
    private void Awake() => SceneManager.sceneLoaded += (_, _) =>
    {
        if (Plugin.Instance == null)
        {
            GameObject parent = new();
            parent.transform.SetParent(null);
            parent.AddComponent<Plugin>();
        }
    };
}

/// <summary> Plugin main class. Essentially only initializes all other components. </summary>
public class Plugin : MonoBehaviour
{
    /// <summary> Plugin instance available everywhere. </summary>
    public static Plugin Instance;
    /// <summary> Whether the plugin has been initialized. </summary>
    public bool Initialized;

    private void Awake() {
        DontDestroyOnLoad(Instance = this); // save the instance of the mod for later use and prevent it from being destroyed by the game
        PrefsManager.Instance.propertyValidators.Clear();
        SceneManager.activeSceneChanged += this.OnSceneChange;
    }

    public void OnSceneChange(Scene _0, Scene _1)
    {
        string titleSceneName = "b3e7f2f8052488a45b35549efb98d902";
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == titleSceneName)
        {
            // difficulty buttons and difficulty infos
            Transform interactables = (from obj in SceneManager.GetActiveScene().GetRootGameObjects() where obj.name == "Canvas" select obj)
                .First<GameObject>().transform.Find("Difficulty Select (1)").Find("Interactables"); 

            // clone the brutal button
            var ukmdButton = Instantiate(interactables.Find("Brutal").gameObject, interactables);
            
            ukmdButton.GetComponent<DifficultySelectButton>().difficulty = 5;
            ukmdButton.transform.Find("Name").GetComponent<TMP_Text>().text = "ULTRAKILL MUST DIE";
            ukmdButton.GetComponent<RectTransform>().position = new Vector2(30f, 157.5f);
            ukmdButton.gameObject.SetActive(true);

            // this won't stay disabled unfortunately, so we can just make it so small that we can't see it
            interactables.Find("V1 Must Die").localScale = new (0f, 0f, 0f);

            var ukmdInfo = Instantiate(interactables.Find("Brutal Info").gameObject, interactables);
            var ukmdTitle = ukmdInfo.transform.Find("Title (1)").GetComponent<TMP_Text>();

            // this is the largest font size that I can use without it being split
            // into two lines
            ukmdTitle.fontSize = 29;
            ukmdTitle.text = "--ULTRAKILL MUST DIE--";

            // set the description of UKMD
            ukmdInfo.transform.Find("Text").GetComponent<TMP_Text>().text = 
                """
                <color=white>Extremely aggressive enemies and very high damage.

                Quick reflexes and extensive knowledge of the game are expected. Any mistake made is likely to be deadly.</color>

                <b>Recommended only for those who are worthy.</b>
                """;

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
            onClick.callback.AddListener((data) => ukmdInfo.SetActive(false));

            // add new triggers to ukmd button
            ukmdTrigger.triggers.Add(pointerEnter);
            ukmdTrigger.triggers.Add(pointerExit);
            ukmdTrigger.triggers.Add(onClick);
        }
    }
}
