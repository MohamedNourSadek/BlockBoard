using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : Manager
{
    public bool IsDebugEnabled = true;

    public override void Awake()
    {
        base.Awake();
        Debug.unityLogger.logEnabled = IsDebugEnabled;
    }

}
