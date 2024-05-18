using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : Manager
{
    public bool IsDebugEnabled = true;

    public override void Awake()
    {
        base.Awake();
        DebugManager = this;
    }
    public static void Log(object message)
    {
        if(DebugManager.IsDebugEnabled)
            UnityEngine.Debug.Log(message);
    }
    public static void LogWarning(object message)
    {
        if (DebugManager.IsDebugEnabled)
            UnityEngine.Debug.LogWarning(message);
    }
    public static void LogError(object message)
    {
        if (DebugManager.IsDebugEnabled)
            UnityEngine.Debug.LogError(message);
    }

}
