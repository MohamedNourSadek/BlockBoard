using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnlineModesPanel : Panel
{
    public TMP_Text TitleText;
    public Button PlayPublic;
    public Button HostPrivate;
    public Button JoinPrivate;
    public Button LeaderboardButton;
    public Button LeaveButton;

    public override void Awake()
    {
        base.Awake();

        PlayPublic.onClick.AddListener(OnPlayPublicPressed);
        HostPrivate.onClick.AddListener(OnHostPrivatePressed);
        JoinPrivate.onClick.AddListener(OnJoinPrivatePressed);
        LeaderboardButton.onClick.AddListener(OnLeaderboardPressed);
        LeaveButton.onClick.AddListener(OnLeavePressed);

    }

    public override void Show()
    {
        base.Show();
        PhotonManager.Instance.SetPhotonOnlineSettings();
    }

    private void OnPlayPublicPressed()
    {
        throw new NotImplementedException();
    }
    private void OnHostPrivatePressed()
    {
        Hide();
        Panel.GetPanel<HostLobbyPanel>().Show();
    }
    private void OnJoinPrivatePressed()
    {
        Hide();
        Panel.GetPanel<JoinLobbyPanel>().Show();
    }
    private void OnLeaderboardPressed()
    {
        throw new NotImplementedException();
    }
    private void OnLeavePressed()
    {
        Hide();
        Panel.GetPanel<ModeSelectionPanel>().Show();
    }
}
