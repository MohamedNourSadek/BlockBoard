using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Manager : SerializedMonoBehaviour
{
    public static Dictionary<Type, Manager> Managers = new Dictionary<Type, Manager>();
    
    public static GameManager GameManager;
    
    public virtual void Awake()
    {
        if (!Managers.ContainsKey(GetType()))
        {
            Managers.Add(GetType(), this);
        }
    }


    public virtual void OnDestroy()
    {
        if(Managers.ContainsKey(GetType()))
            Managers.Remove(GetType());
    }

    public static T GetManager<T>() where T : Manager
    {
        Type type = typeof(T);

        if(Managers.ContainsKey(type))
            return Managers[type] as T;

        return null;
    }


}
