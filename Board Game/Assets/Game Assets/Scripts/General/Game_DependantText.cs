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

        if (Manager.GameManager.CurrentGame == GameType.Domino)
            mytext.text = DominoText;
        else if (Manager.GameManager.CurrentGame == GameType.Chess)
            mytext.text = ChessText;
        else if (Manager.GameManager.CurrentGame == GameType.Poker)
            mytext.text = PokerText;
    }
}
