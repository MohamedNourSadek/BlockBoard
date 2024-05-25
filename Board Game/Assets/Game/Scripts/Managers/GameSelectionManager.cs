using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSelectionManager : Manager
{
    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        GameType gameType = Manager.GameManager.CurrentGame;

        switch (gameType)
        {
            case GameType.Domino:
                DominoController.Instance.StartGame();
                break;
            case GameType.Chess:
                break;
            case GameType.Poker:
                break;
        }
    }
}
