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

        if (Manager.GameManager.CurrentGame == GameType.Domino)
            mybutton.interactable = DominoIsInteractable;
        else if (Manager.GameManager.CurrentGame == GameType.Chess)
            mybutton.interactable = ChessIsInteractable;
    }
}
