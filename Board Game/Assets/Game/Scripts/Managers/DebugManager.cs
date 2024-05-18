using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : Manager
{
    public bool IsDebugEnabled = true;
    
    public static DebugManager Instance;

    public override void Awake()
    {
        base.Awake();
        Instance = this;
    }
    public void Log(object message)
    {
        if(Instance.IsDebugEnabled)
            Debug.Log(message);
    }
    public void LogWarning(object message)
    {
        if (Instance.IsDebugEnabled)
            Debug.LogWarning(message);
    }
    public void LogError(object message)
    {
        if (Instance.IsDebugEnabled)
            Debug.LogError(message);
    }

}
