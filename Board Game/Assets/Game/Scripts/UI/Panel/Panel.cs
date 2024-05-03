using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
    public virtual void Awake()
    {
        var panelsManager = Manager.GetManager<PanelsManager>();

        if(!panelsManager.Panels.ContainsKey(GetType()))
            panelsManager.Panels.Add(GetType(), this);

    }

    public virtual void Start()
    {
        RefreshUI();
    }

    public void OnDestroy()
    {
        var panelsManager = Manager.GetManager<PanelsManager>();

        if(panelsManager && panelsManager.Panels.ContainsKey(GetType()))
            panelsManager.Panels.Remove(GetType());
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
