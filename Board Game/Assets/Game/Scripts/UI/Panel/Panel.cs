using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
    public void Awake()
    {
        var panelsManager = Manager.GetManager<PanelsManager>();

        if(!panelsManager.Panels.ContainsKey(GetType()))
            panelsManager.Panels.Add(GetType(), this);
    }

    public void OnDestroy()
    {
        var panelsManager = Manager.GetManager<PanelsManager>();

        if(panelsManager.Panels.ContainsKey(GetType()))
            panelsManager.Panels.Remove(GetType());
    }

    public virtual void Show()
    {
        AnimatePanel(true);
    }

    public virtual void Hide()
    {
        AnimatePanel(false);
    }

    public virtual void AnimatePanel(bool show)
    {
        this.gameObject.SetActive(show);
    }   

}
