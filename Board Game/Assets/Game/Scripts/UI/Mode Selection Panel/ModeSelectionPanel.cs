using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModeSelectionPanel : Panel
{
    public TMP_Text GameNameText;
    public Button OfflineButton;
    public Button OnlineButton;
    public Button BackButton;

    public override void Awake()
    {
        base.Awake();

        OfflineButton.onClick.AddListener(OnOfflinePressed);
        OnlineButton.onClick.AddListener(OnOnlinePressed);
        BackButton.onClick.AddListener(OnBackPressed);
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        GameNameText.text = Manager.GameManager.CurrentGame.ToString();
    }

    private void OnBackPressed()
    {
        Hide();
        Panel.GetPanel<MainMenuPanel>().Show();
    }
    private void OnOnlinePressed()
    {

    }
    private void OnOfflinePressed()
    {
        Hide();
        Panel.GetPanel<OfflineRoomPanel>().Show();  
    }
}
