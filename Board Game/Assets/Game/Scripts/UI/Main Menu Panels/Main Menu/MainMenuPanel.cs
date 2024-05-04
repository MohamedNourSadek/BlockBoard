using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : Panel
{
    public GameItem GameItemPrefab;
    public GameObject GameListParent;
    public Button SettingsButton;
    public Button ExitButton;
    public Button ProfileButton;

    public override void Awake()
    {
        base.Awake();

        SettingsButton.onClick.AddListener(OnSettingsPressed);
        ExitButton.onClick.AddListener(OnQuitPressed);
        ProfileButton.onClick.AddListener(OnProfilePressed);    
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

        var spawnedGameItmes = GameListParent.GetComponentsInChildren<GameItem>();

        foreach(var item in spawnedGameItmes)
            Destroy(item.gameObject);

        foreach(var game in Manager.GetManager<DataManager>().GameTypeData)
        {
            var newGameUI = Instantiate(GameItemPrefab, GameListParent.transform);
            newGameUI.Init(game.Key, game.Value);
        }
    }

    public void OnSettingsPressed()
    {
        Hide();
        GetPanel<SettingsPanel>().Show();
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }

    public void OnProfilePressed()
    {
        Hide();
        GetPanel<ProfilePanel>().Show();
    }

}

