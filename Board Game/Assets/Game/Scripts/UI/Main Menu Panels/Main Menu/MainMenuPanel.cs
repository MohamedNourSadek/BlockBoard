using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPanel : Panel
{
    public GameItem GameItemPrefab;
    public GameObject GameListParent;

    
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

}

