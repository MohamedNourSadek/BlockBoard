using Sirenix.OdinInspector;
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
    public Button FirstButton;
    public Button SecondButton;
    public TMP_Text FirstButtonText;
    public TMP_Text SecondButtonText;

    public Action OnFirstButton;
    public Action OnSecondButton;


    public override void Awake()
    {
        base.Awake();

        FirstButton.onClick.AddListener(OnFirstButtonPressed);
        SecondButton.onClick.AddListener(OnSecondButtonPressed);
    }

    public void Show(string title, string message, ButtonTypes firstButtonType = ButtonTypes.Yes, ButtonTypes secondButtonType = ButtonTypes.No, Action firstButtonCallback = null, Action secondButtonCallback = null)
    {
        Title.text = title;
        Message.text = message;

        if(firstButtonType != ButtonTypes.None)
        {
            FirstButton.gameObject.SetActive(true);
            FirstButtonText.text = firstButtonType.ToString();
            OnFirstButton = firstButtonCallback;
        }
        else
        {
            FirstButton.gameObject.SetActive(false);
            OnFirstButton = null;
        }

        if (secondButtonType != ButtonTypes.None)
        {
            SecondButton.gameObject.SetActive(true);
            SecondButtonText.text = secondButtonType.ToString();
            OnSecondButton = secondButtonCallback;
        }
        else
        {
            SecondButton.gameObject.SetActive(false);
            OnSecondButton = null;
        }

        base.Show();
    }

    public override void Hide()
    {
        base.Hide();

        OnFirstButton = null;
        OnSecondButton = null;
    }
    
    public void OnFirstButtonPressed()
    {
        OnFirstButton?.Invoke();
        Hide();
    }
    public void OnSecondButtonPressed()
    {
        OnSecondButton?.Invoke();
        Hide();
    }
}

public enum ButtonTypes
{
    Ok, Yes, No, Cancel, None
}