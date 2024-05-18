using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpPanel : Panel
{
    public TMP_InputField EmailInput;
    public TMP_InputField UsernameInput;
    public TMP_InputField PasswordInput;

    public TMP_Text EmailErrorText;
    public TMP_Text UsernameErrorText;
    public TMP_Text PasswordErrorText;

    public Button SignUpButton;
    public Button BackButton;


    public override void Awake()
    {
        base.Awake();

        SignUpButton.onClick.AddListener(OnSignUpPressed);
        BackButton.onClick.AddListener(OnBackPressed);

        EmailInput.onValueChanged.AddListener(OnEmailChanged);
        UsernameInput.onValueChanged.AddListener(OnUsernameChanged);
        PasswordInput.onValueChanged.AddListener(OnPasswordChanged);
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        OnUsernameChanged(UsernameInput.text);
        OnEmailChanged(EmailInput.text);
        OnPasswordChanged(PasswordInput.text);
    }


    private void OnSignUpPressed()
    {
        Hide();
        Panel.GetPanel<WaitingPanel>().Show(OnSignUpCanceled);
    }
    private void OnSignUpCanceled()
    {
        Show();
    }
    private void OnBackPressed()
    {
        Hide();
        Panel.GetPanel<ProfilePanel>().Show<ProfileNoLoginPanel>();
    }
    private void OnUsernameChanged(string value)
    {
        var usernameCorrection = InputCorrectionManager.GetCorrectionMessage(UsernameInput.text, InputType.Username);
        var emailCorrection = InputCorrectionManager.GetCorrectionMessage(EmailInput.text, InputType.Email);
        var passwordCorrection = InputCorrectionManager.GetCorrectionMessage(PasswordInput.text, InputType.Password);

        UsernameErrorText.text = usernameCorrection.CorrectionMessage;
        SignUpButton.interactable = usernameCorrection.IsInputCorrect && emailCorrection.IsInputCorrect && passwordCorrection.IsInputCorrect;
    }
    private void OnEmailChanged(string value)
    {
        var usernameCorrection = InputCorrectionManager.GetCorrectionMessage(UsernameInput.text, InputType.Username);
        var emailCorrection = InputCorrectionManager.GetCorrectionMessage(EmailInput.text, InputType.Email);
        var passwordCorrection = InputCorrectionManager.GetCorrectionMessage(PasswordInput.text, InputType.Password);

        EmailErrorText.text = emailCorrection.CorrectionMessage;
        SignUpButton.interactable = usernameCorrection.IsInputCorrect && emailCorrection.IsInputCorrect && passwordCorrection.IsInputCorrect ;
    }
    private void OnPasswordChanged(string value)
    {
        var usernameCorrection = InputCorrectionManager.GetCorrectionMessage(UsernameInput.text, InputType.Username);
        var emailCorrection = InputCorrectionManager.GetCorrectionMessage(EmailInput.text, InputType.Email);
        var passwordCorrection = InputCorrectionManager.GetCorrectionMessage(PasswordInput.text, InputType.Password);

        PasswordErrorText.text = passwordCorrection.CorrectionMessage;
        SignUpButton.interactable = usernameCorrection.IsInputCorrect && emailCorrection.IsInputCorrect && passwordCorrection.IsInputCorrect;
    }
}
