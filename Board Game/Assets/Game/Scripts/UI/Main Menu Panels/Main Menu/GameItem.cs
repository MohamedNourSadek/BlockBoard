using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GameItem : MonoBehaviour
{
    public Button MyButton;
    public TextMeshProUGUI GameNameText;
    public Image GameIcon;
    
    public GameType GameType;


    private void Awake()
    {
        MyButton.onClick.AddListener(OnButtonPressed);
    }
    public void Init(GameType gameType, GameDataItem gameData)
    {
        GameNameText.text = gameData.GameType.ToString();
        GameIcon.sprite = gameData.GameIcon;
        GameType = gameType;
    }

    public void OnButtonPressed()
    {
        var tutorialManagers = Manager.GetManager<TutorialsManager>();

        Manager.GameManager.CurrentGame = GameType;

        if (tutorialManagers.IsTutorialAvailable(GameType))
        {
            string title = Manager.GameManager.CurrentGame.ToString();
            string message = "Do you want to start the tutorial?";

            Panel.GetPanel<MainMenuPanel>().Hide();
            Panel.GetPanel<MessagePanel>().Show(title, message, OnOpenTutorialYes , OnOpenTutorialNo);
        }
        else
        {

        }
    }
    public void OnOpenTutorialYes()
    {

    }
    public void OnOpenTutorialNo()
    {
        Panel.GetPanel<MessagePanel>().Hide();
        Panel.GetPanel<MainMenuPanel>().Show();
    }
}


