using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
    [NonSerialized] public static Dictionary<Type, Panel> Panels = new Dictionary<Type, Panel>();
    
    private Dictionary<Type, Panel> SubPanels = new Dictionary<Type, Panel>();

    public virtual void Awake()
    {
        AddToPanelList();
        GetChildPanels();
    }
    public virtual void Start()
    {
        RefreshUI();
    }
    public void OnDestroy()
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
    public void GetChildPanels()
    {
        foreach (var panel in GetComponentsInChildren<Panel>(true))
        {
            if (SubPanels.ContainsKey(panel.GetType()))
                SubPanels[panel.GetType()] = panel;
            else 
                SubPanels.Add(panel.GetType(), panel);
        }
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
    public virtual void Show<T>() where T : Panel
    {
        if (SubPanels.Count == 0)
            GetChildPanels();

        if(SubPanels.ContainsKey(typeof(T)))
        {
            SubPanels[typeof(T)].Show();
        }
        else
        {
            Debug.LogError("Panel not found in SubPanels list");
        }
    }
    public virtual void Hide()
    {
        foreach (var panel in SubPanels)
            if(panel.Value != this)
                panel.Value.Hide();

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
