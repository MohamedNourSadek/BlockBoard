using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_DependantObject : MonoBehaviour
{
    [SerializeField] GameObject DominoObject;
    [SerializeField] GameObject ChessObject;
    [SerializeField] GameObject PokerObject;

    private void Start()
    {
        if (Manager.GameManager.CurrentGame == GameType.Domino)
        {
            if(DominoObject)
                DominoObject.SetActive(true);

            if(ChessObject)
                ChessObject.SetActive(false);
            
            if(PokerObject) 
                PokerObject.SetActive(false);
        }
        else if (Manager.GameManager.CurrentGame == GameType.Chess)
        {
            if(DominoObject)
                DominoObject.SetActive(false);
            
            if(ChessObject)
                ChessObject.SetActive(true);
            
            if(PokerObject)
                PokerObject.SetActive(false);
        }
        else if (Manager.GameManager.CurrentGame == GameType.Poker)
        {
            if(DominoObject)
                DominoObject.SetActive(false);
            
            if(ChessObject)
                ChessObject.SetActive(false);
            
            if(PokerObject)
                PokerObject.SetActive(true);
        }
    }
}
