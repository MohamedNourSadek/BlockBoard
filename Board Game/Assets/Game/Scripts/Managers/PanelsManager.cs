using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelsManager : Manager
{
    [NonSerialized] public Dictionary<Type, Panel> Panels = new Dictionary<Type, Panel>();

    public T GetPanel<T>() where T : Panel
    {
        if(Panels.ContainsKey(typeof(T)))
            return Panels[typeof(T)] as T;
        else 
            return null;
    }
}
