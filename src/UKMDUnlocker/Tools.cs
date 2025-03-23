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

    public static void PatchOne(this Harmony harmony, Type origType, string methodName, Type patch, bool prefix = false)
    {
        harmony.Patch(
            origType.GetMethod(methodName, AccessTools.all),
            prefix: (prefix)? new(patch.GetMethod("Prefix", AccessTools.all)) : null,
            postfix: (prefix)? null : new(patch.GetMethod("Postfix", AccessTools.all))
        );
    }
}