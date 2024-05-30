using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameNormalPanel : Panel
{
    public TMP_Text GameTurnText;
    public Button TutorialButton;
    public Button SettingsButton;
    public Button MessagesButton;
    public Button GameViewButton;
    public Image GameViewImage;
    public Sprite GameViewSpriteOn;
    public Sprite GameViewSpriteOff;

    public Color YourTurnColor;
    public Color OpponentTurnColor;


    public override void Awake()
    {
        base.Awake();

        TutorialButton.onClick.AddListener(OnTutorialPressed);
        SettingsButton.onClick.AddListener(OnPausePressed);
        GameViewButton.onClick.AddListener(OnGameViewPressed);

        if(Manager.GameManager.GameMode == GameMode.Offline)
            MessagesButton.gameObject.SetActive(false);
        else
            MessagesButton.onClick.AddListener(OnMessagesPressed);
    }
    public void OnEnable()
    {
        RefreshUI();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        var settingsManager = Manager.GetManager<SettingsManager>().CameraSnap;
        GameViewImage.sprite = settingsManager ? GameViewSpriteOn : GameViewSpriteOff;
    }
    public void SetTurnUi(Turn turn)
    {
        if(turn == Turn.MyTurn)
        {
            GameTurnText.text = "Your Turn";
            GameTurnText.color = YourTurnColor;
        }
        else
        {
            GameTurnText.text = "Opponent's Turn";
            GameTurnText.color = OpponentTurnColor;
        }
    }


    private void OnGameViewPressed()
    {
        var settingsManager = Manager.GetManager<SettingsManager>();
        settingsManager.ChangeCameraSnap(!settingsManager.CameraSnap);

        RefreshUI();
    }
    private void OnMessagesPressed()
    {
        throw new NotImplementedException();
    }
    private void OnPausePressed()
    {
        Hide();
        Panel.GetPanel<PausePanel>().Show();
    }
    private void OnTutorialPressed()
    {
        var tutorialPanel = Panel.GetPanel<TutorialPanel>();
        var dataManager = Manager.GetManager<DataManager>();

        switch (Manager.GameManager.CurrentGame)
        {
            case GameType.Domino:
                tutorialPanel.ShowTutorial(dataManager.DominoTutorialStrings);
                break;
            case GameType.Chess:
                tutorialPanel.ShowTutorial(dataManager.ChessTutorialStrings);
                break;
            case GameType.Poker:
                tutorialPanel.ShowTutorial(dataManager.PokerTutorialStrings);
                break;
        }

        Panel.GetPanel<GameNormalPanel>().Hide();
    }
}


public enum Turn
{
    MyTurn, OpponentTurn
}
