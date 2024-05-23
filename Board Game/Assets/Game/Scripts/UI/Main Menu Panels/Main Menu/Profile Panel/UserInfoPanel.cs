using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoPanel : Panel
{
    public TMP_Text UserName;
    public TMP_Text UserEmail;

    public Button LogoutButton;
    public Button BackButton;
    public Button ChessButton;
    public Button DominoButton;
    public Button PokerButton;

    public override void Awake()
    {
        base.Awake();

        LogoutButton.onClick.AddListener(OnLogoutPressed);
        BackButton.onClick.AddListener(OnBackPressed);

        ChessButton.onClick.AddListener(OnChessPressed);
        DominoButton.onClick.AddListener(OnDominoPressed);
        PokerButton.onClick.AddListener(OnPokerPressed);

        Manager.GetManager<ProfileManager>().OnProfileDataReceived += OnProfileDataReceived;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        
        Manager.GetManager<ProfileManager>().OnProfileDataReceived -= OnProfileDataReceived;
    }

    public override void Show()
    {
        base.Show();
        OnProfileDataReceived();
    }

    private void OnProfileDataReceived()
    {
        PlayerProfile profile = Manager.GetManager<ProfileManager>().GetPlayerProfile();

        UserName.text = profile.NickName;
        UserEmail.text = profile.Email;
    }
    private void OnLogoutPressed()
    {
        Manager.GetManager<PlayfabManager>().Logout();
        Hide();
        Panel.GetPanel<LoginPanel>().Show();
    }
    private void OnBackPressed()
    {
        Hide();
        Panel.GetPanel<MainMenuPanel>().Show();
    }
    private void OnChessPressed()
    {
        Hide();
        Panel.GetPanel<GameStatsPanel>().Show(GameType.Chess);
    }
    private void OnDominoPressed()
    {
        Hide();
        Panel.GetPanel<GameStatsPanel>().Show(GameType.Domino);
    }
    private void OnPokerPressed()
    {
        Hide();
        Panel.GetPanel<GameStatsPanel>().Show(GameType.Poker);
    }
}
