using PlayFab;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ResetPasswordPanel : Panel
{
    public TMP_InputField EmailInputField;
    public TMP_Text EmailErrorMessage;
    public Button ResetButton;
    public Button CancelButton;

    public override void Awake()
    {
        base.Awake();

        EmailInputField.onValueChanged.AddListener(OnEmailChanged);
        
        CancelButton.onClick.AddListener(OnCancelPressed);
        ResetButton.onClick.AddListener(OnResetPasswordPressed);
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        OnEmailChanged(EmailInputField.text);
    }

    public void OnResetPasswordPressed()
    {
        Hide();
        Manager.GetManager<PlayfabManager>().ResetPassword(EmailInputField.text, OnResetCallBack);
    }

    public void OnResetCallBack(bool state, object callbackInfo)
    {
        if(state)
        {
            Panel.GetPanel<MessagePanel>().Show(
                "Reset Password", "An email has been sent to your email address with instructions on how to reset your password.",
                ButtonTypes.Ok, ButtonTypes.None, Show);
        }
        else
        {
            Panel.GetPanel<MessagePanel>().Show("Reset Password", ((PlayFabError)callbackInfo).ErrorMessage,
                ButtonTypes.Ok, ButtonTypes.None, Show);
        }
    }

    public void OnCancelPressed()
    {
        Hide();
        Panel.GetPanel<ProfilePanel>().Show<LoginPanel>();
    }
    public void OnEmailChanged(string value)
    {
        var emailCorrection = InputCorrectionManager.GetCorrectionMessage(EmailInputField.text, InputType.Email);
        EmailErrorMessage.text = emailCorrection.CorrectionMessage;
        ResetButton.interactable = emailCorrection.IsInputCorrect;
    }
}
