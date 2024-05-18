using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        Panel.GetPanel<WaitingPanel>().Show(OnLoginCanceled);
    }

    public void OnLoginCanceled()
    {
        Show();
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
