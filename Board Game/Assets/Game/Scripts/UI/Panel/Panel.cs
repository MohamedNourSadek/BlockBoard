using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : SerializedMonoBehaviour
{
    [NonSerialized] public static Dictionary<Type, Panel> Panels = new Dictionary<Type, Panel>();
    
    public virtual void Awake()
    {
        AddToPanelList();
    }
    public virtual void Start()
    {
        RefreshUI();
    }
    public virtual void OnDestroy()
    {
        RemoveFromPanelList();
    }

    public static T GetPanel<T>() where T : Panel
    {
        if (Panels.ContainsKey(typeof(T)))
            return Panels[typeof(T)] as T;
        else
            return null;
    }
    public void AddToPanelList()
    {
        if (!Panels.ContainsKey(this.GetType()))
            Panels.Add(this.GetType(), this);
    }
    public void RemoveFromPanelList()
    {
        if (Panels.ContainsKey(this.GetType()))
            Panels.Remove(this.GetType());
    }

    public virtual void Show()
    {
        AnimatePanel(true);
        RefreshUI();
    }
    public virtual void Hide()
    {
        AnimatePanel(false);
    }
    public virtual void AnimatePanel(bool show)
    {
        this.gameObject.SetActive(show);
    }   
    public virtual void RefreshUI()
    {

    }

}
