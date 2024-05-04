using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game_DependantButton : MonoBehaviour
{
    [SerializeField] bool DominoIsInteractable;
    [SerializeField] bool ChessIsInteractable;

    Button mybutton;

    private void OnEnable()
    {
        mybutton = GetComponent<Button>();

        if (Cross_Scene_Data.currentGame == GameType.Domino)
            mybutton.interactable = DominoIsInteractable;
        else if (Cross_Scene_Data.currentGame == GameType.Chess)
            mybutton.interactable = ChessIsInteractable;
    }
}
