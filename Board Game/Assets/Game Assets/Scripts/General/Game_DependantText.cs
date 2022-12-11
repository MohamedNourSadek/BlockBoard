using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_DependantText : MonoBehaviour
{
    [SerializeField] string DominoText;
    [SerializeField] string ChessText;
    [SerializeField] string PokerText;

    Text mytext;
    private void OnEnable()
    {
        mytext = GetComponent<Text>();

        if (Cross_Scene_Data.currentGame == CurrentGame.Domino)
            mytext.text = DominoText;
        else if (Cross_Scene_Data.currentGame == CurrentGame.Chess)
            mytext.text = ChessText;
        else if (Cross_Scene_Data.currentGame == CurrentGame.Poker)
            mytext.text = PokerText;
    }
}
