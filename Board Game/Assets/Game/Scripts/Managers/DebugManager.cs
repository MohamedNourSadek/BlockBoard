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
    public static void Debug(object message)
    {
        if(DebugManager.IsDebugEnabled)
            UnityEngine.Debug.Log(message);
    }
}
