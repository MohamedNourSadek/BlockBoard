using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Dictionary<Type, Manager> Managers = new Dictionary<Type, Manager>();


    public virtual void Awake()
    {
        if(!Managers.ContainsKey(GetType()))
            Managers.Add(GetType(), this);
        else
            Debug.LogWarning("There are more than one " + GetType() + " in the scene");
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
