namespace UKMDUnlocker;

using UnityEngine.Events;
using UnityEngine.EventSystems;

public static class Tools
{
    public static int Difficulty
    {
        set => PrefsManager.Instance.SetInt("difficulty", value);
        get => PrefsManager.Instance.GetInt("difficulty", -1);
    }

    public static EventTrigger.Entry CreateTriggerEntry(EventTriggerType id, UnityAction<BaseEventData> call)
    {
        EventTrigger.Entry ret = new() { eventID = id };
        ret.callback.AddListener(call);
        return ret;
    }
}