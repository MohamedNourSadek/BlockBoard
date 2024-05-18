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
    }

    public void Show(string userName, string userEmail)
    {
        UserName.text = userName;
        UserEmail.text = userEmail;
        
        base.Show();
    }

    public void OnLogoutPressed()
    {
        Hide();
        Panel.GetPanel<ProfilePanel>().Show<LoginPanel>();
    }

    public void OnBackPressed()
    {
        Panel.GetPanel<ProfilePanel>().Hide();
        Panel.GetPanel<MainMenuPanel>().Show();
    }

    public void OnChessPressed()
    {
        Hide();
        Panel.GetPanel<ProfilePanel>().Show<GameStatsPanel>();
        Panel.GetPanel<GameStatsPanel>().Show(GameType.Chess);
    }

    public void OnDominoPressed()
    {
        Hide();
        Panel.GetPanel<ProfilePanel>().Show<GameStatsPanel>();
        Panel.GetPanel<GameStatsPanel>().Show(GameType.Domino);
    }

    public void OnPokerPressed()
    {
        Hide();
        Panel.GetPanel<ProfilePanel>().Show<GameStatsPanel>();
        Panel.GetPanel<GameStatsPanel>().Show(GameType.Poker);
    }
}
