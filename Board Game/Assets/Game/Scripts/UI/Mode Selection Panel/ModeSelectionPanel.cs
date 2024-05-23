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
        Hide();

        var isLoggedIn = Manager.GetManager<PlayfabManager>().IsLoggedIn;

        if(isLoggedIn)
        {
            GetPanel<WaitingPanel>().Show(null, false);
            var playfabManager = Manager.GetManager<PlayfabManager>();
            playfabManager.DataReceivedCallback += OnDataRecieved;
            playfabManager.GetUserData();
        }
        else
        {
            Hide();
            GetPanel<ProfileNoLoginPanel>().Show();
        }
    }
    private void OnDataRecieved(bool success, object data)
    {
        var playfabManager = Manager.GetManager<PlayfabManager>();
        playfabManager.DataReceivedCallback -= OnDataRecieved;

        PhotonManager.Instance.OnPhotonFullyConnected += OnPhotonFullyConnected;
        PhotonManager.Instance.ConnectToPhoton();
    }

    private void OnPhotonFullyConnected()
    {
        PhotonManager.Instance.OnPhotonFullyConnected -= OnPhotonFullyConnected;

        GetPanel<WaitingPanel>().Hide();
        GetPanel<OnlineModesPanel>().Show();
    }
    private void OnOfflinePressed()
    {
        Hide();
        Panel.GetPanel<OfflineRoomPanel>().Show();  
    }
}
