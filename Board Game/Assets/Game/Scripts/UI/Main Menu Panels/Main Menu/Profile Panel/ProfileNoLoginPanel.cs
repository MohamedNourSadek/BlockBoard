using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileNoLoginPanel : Panel
{
    public Button LoginButton;
    public Button SignUpButton;
    public Button BackButton;

    public override void Awake()
    {
        base.Awake();

        BackButton.onClick.AddListener(OnBackPressed);
        LoginButton.onClick.AddListener(OnLoginPressed);
        SignUpButton.onClick.AddListener(OnSignUpPressed);
    }

    public void OnLoginPressed()
    {
        Hide();
        Panel.GetPanel<ProfilePanel>().Show<LoginPanel>();
    }
    public void OnSignUpPressed()
    {
        Hide();
        Panel.GetPanel<ProfilePanel>().Show<SignUpPanel>();
    }
    public void OnBackPressed()
    {
        Panel.GetPanel<ProfilePanel>().Hide();   
        Panel.GetPanel<MainMenuPanel>().Show();
    }
}
