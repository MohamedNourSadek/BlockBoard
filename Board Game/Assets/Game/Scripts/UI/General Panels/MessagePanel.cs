using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessagePanel : Panel
{
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Message;
    public Button OkButton;
    public Button CancelButton;

    public Action OnOkPressed;
    public Action OnCancelPressed;


    public override void Awake()
    {
        base.Awake();

        OkButton.onClick.AddListener(OnOkButtonPressed);
        CancelButton.onClick.AddListener(OnCancelButtonPressed);
    }

    public void Show(string title, string message, Action onOkPressed = null, Action onCancelPressed = null)
    {
        Title.text = title;
        Message.text = message;

        OnOkPressed = onOkPressed;
        OnCancelPressed = onCancelPressed;

        base.Show();
    }
    public override void Hide()
    {
        base.Hide();

        OnOkPressed = null;
        OnCancelPressed = null;
    }
    
    public void OnOkButtonPressed()
    {
        OnOkPressed?.Invoke();
        Hide();
    }
    public void OnCancelButtonPressed()
    {
        OnCancelPressed?.Invoke();
        Hide();
    }
}
