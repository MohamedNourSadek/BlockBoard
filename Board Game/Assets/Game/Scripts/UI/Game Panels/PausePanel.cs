using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausePanel : Panel
{
    public GameObject Layer0;
    public GameObject Settings;

    public Button ContinueButton;
    public Button SettingsButton;
    public Button ExitButton;

    public override void Awake()
    {
        base.Awake();
        
        ContinueButton.onClick.AddListener(OnContinuePressed);
        SettingsButton.onClick.AddListener(OnSettingsPressed);
        ExitButton.onClick.AddListener(OnExitPressed);
    }

    public override void Show()
    {
        base.Show();

        Manager.GameManager.PauseGame();
        Layer0.SetActive(true);
        Settings.SetActive(false);
    }

    private void OnContinuePressed()
    {
        Manager.GameManager.ResumeGame();
        Hide();
        Panel.GetPanel<GameNormalPanel>().Show();
    }
    private void OnSettingsPressed()
    {
        Layer0.SetActive(false);
        Settings.SetActive(true);
    }
    public void OnBackFromSettingsPressed()
    {
        Layer0.SetActive(true);
        Settings.SetActive(false);
    }
    private void OnExitPressed()
    {
        PhotonNetwork.LeaveRoom();
        Manager.GameManager.LoadMenu();
    }
}
