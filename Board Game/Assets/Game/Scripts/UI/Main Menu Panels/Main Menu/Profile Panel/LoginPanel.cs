using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class LoginPanel : Panel
{
    public Button LoginButton;
    public Button CancelButton;
    public Button ResetPasswordButton;
    public TMP_InputField EmailInputField;
    public TMP_InputField PasswordInputField;
    public TMP_Text EmailErrorText;
    public TMP_Text PasswordErrorText;


    public override void Awake()
    {
        base.Awake();

        CancelButton.onClick.AddListener(OnCancelPressed);
        LoginButton.onClick.AddListener(OnLoginPressed);
        ResetPasswordButton.onClick.AddListener(OnResetPasswordPressed);

        EmailInputField.onValueChanged.AddListener(OnEmailChanged);
        PasswordInputField.onValueChanged.AddListener(OnPasswordChanged);
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

        OnEmailChanged(EmailInputField.text);
        OnPasswordChanged(PasswordInputField.text);
    }

    public void OnLoginPressed()
    {
        Hide();
        Panel.GetPanel<WaitingPanel>().Show(OnLoginCanceled);
    }

    public void OnLoginCanceled()
    {
        Show();
    }

    public void OnResetPasswordPressed()
    {
        Hide();
        Panel.GetPanel<ProfilePanel>().Show<ResetPasswordPanel>();
    }

    public void OnCancelPressed()
    {
        Hide();
        Panel.GetPanel<ProfilePanel>().Show<ProfileNoLoginPanel>();
    }

    public void OnEmailChanged(string value)
    {
        var emailCorrection = InputCorrectionManager.GetCorrectionMessage(EmailInputField.text, InputType.Email);   
        var passwordCorrection = InputCorrectionManager.GetCorrectionMessage(PasswordInputField.text, InputType.Username);

        EmailErrorText.text = emailCorrection.CorrectionMessage;
        LoginButton.interactable = emailCorrection.IsInputCorrect && passwordCorrection.IsInputCorrect;
    }


    public void OnPasswordChanged(string value)
    {
        var emailCorrection = InputCorrectionManager.GetCorrectionMessage(EmailInputField.text, InputType.Email);
        var passwordCorrection = InputCorrectionManager.GetCorrectionMessage(PasswordInputField.text, InputType.Username);

        PasswordErrorText.text = passwordCorrection.CorrectionMessage;
        LoginButton.interactable = emailCorrection.IsInputCorrect && passwordCorrection.IsInputCorrect;
    }
}
