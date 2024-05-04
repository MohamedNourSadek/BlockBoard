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
        if (Manager.GameManager.CurrentGame == GameType.Domino)
        {
            DominoObject.SetActive(true);
            ChessObject.SetActive(false);
            PokerObject.SetActive(false);
        }
        else if (Manager.GameManager.CurrentGame == GameType.Chess)
        {
            ChessObject.SetActive(true);
            DominoObject.SetActive(false);
            PokerObject.SetActive(false);
        }
        else if (Manager.GameManager.CurrentGame == GameType.Poker)
        {
            PokerObject.SetActive(true);
            ChessObject.SetActive(false);
            DominoObject.SetActive(false);
        }
    }
}
