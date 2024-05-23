using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OfflineRoomPanel : Panel
{
    public TMP_Text GameTile;
    public Button StartButton;
    public Button BackButton;
    public Dictionary<GameType, Panel> GameSettings = new Dictionary<GameType, Panel>();

    public override void Awake()
    {
        base.Awake();

        StartButton.onClick.AddListener(OnStartPressed);
        BackButton.onClick.AddListener(OnBackPressed);
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

        Manager.GameManager.GameMode = GameMode.Offline;

        GameTile.text = "Offline " + Manager.GameManager.CurrentGame.ToString();
        
        foreach(GameType game in GameSettings.Keys)
        {
            if (game == GameType.None)
                continue;

            if(game == Manager.GameManager.CurrentGame)
                GameSettings[game].Show();
            else 
                GameSettings[game].Hide();
        }
    }


    public void OnStartPressed()
    {
        Manager.GameManager.GameMode = GameMode.Offline;
        SceneManager.LoadScene("Game");
    }
    public void OnBackPressed()
    {
        Hide();
        Manager.GameManager.GameMode = GameMode.None;
        Panel.GetPanel<ModeSelectionPanel>().Show();
    }



}
