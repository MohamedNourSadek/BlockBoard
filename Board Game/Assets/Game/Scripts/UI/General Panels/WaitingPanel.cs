using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WaitingPanel : Panel
{
    public Button CancelButton;

    public UnityAction OnCancel;

    public override void Awake()
    {
        base.Awake();
        CancelButton.onClick.AddListener(OnCancelPressed);
    }
    public void Show(UnityAction onCancel = null, bool showCancel = false)
    {
        base.Show();

        if(showCancel)
            CancelButton.gameObject.SetActive(true);
        else
            CancelButton.gameObject.SetActive(false);

        OnCancel = onCancel;
    }

    public void OnCancelPressed()
    {
        Hide();

        if(OnCancel != null)
            OnCancel?.Invoke();

        OnCancel = null;
    }
}
