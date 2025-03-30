namespace UKMDUnlocker;

using HarmonyLib;

using System;
using System.Reflection;

using UnityEngine.Events;
using UnityEngine.EventSystems;

public static class Tools
{
    public static EventTrigger.Entry CreateTriggerEntry(EventTriggerType id, UnityAction<BaseEventData> call)
    {
        EventTrigger.Entry ret = new() { eventID = id };
        ret.callback.AddListener(call);
        return ret;
    }
}