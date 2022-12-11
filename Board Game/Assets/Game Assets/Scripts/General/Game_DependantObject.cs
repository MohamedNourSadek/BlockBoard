using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_DependantObject : MonoBehaviour
{
    [SerializeField] GameObject DominoObject;
    [SerializeField] GameObject ChessObject;
    [SerializeField] GameObject PokerObject;

    private void OnEnable()
    {
        if (Cross_Scene_Data.currentGame == CurrentGame.Domino)
        {
            DominoObject.SetActive(true);
            ChessObject.SetActive(false);
            PokerObject.SetActive(false);
        }
        else if (Cross_Scene_Data.currentGame == CurrentGame.Chess)
        {
            ChessObject.SetActive(true);
            DominoObject.SetActive(false);
            PokerObject.SetActive(false);
        }
        else if (Cross_Scene_Data.currentGame == CurrentGame.Poker)
        {
            PokerObject.SetActive(true);
            ChessObject.SetActive(false);
            DominoObject.SetActive(false);
        }
    }
}
