using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_DependantCamera : MonoBehaviour
{
    [SerializeField] GameObject DominoCamera_Transform;
    [SerializeField] float Domino_POV;
    [SerializeField] GameObject ChessCamera_Transform;
    [SerializeField] float Chess_POV;
    [SerializeField] GameObject PokerCamera_Transform;
    [SerializeField] float Poker_POV;


    private void Awake()
    {
        OnEnable();
    }

    private void OnEnable()
    {
        if (Manager.GameManager.CurrentGame == GameType.Domino)
        {
            Camera.main.transform.position = DominoCamera_Transform.transform.position;
            Camera.main.transform.rotation = DominoCamera_Transform.transform.rotation;
            Camera.main.fieldOfView = Domino_POV;
        }
        else if (Manager.GameManager.CurrentGame == GameType.Chess)
        {
            Camera.main.transform.position = ChessCamera_Transform.transform.position;
            Camera.main.transform.rotation = ChessCamera_Transform.transform.rotation;
            Camera.main.fieldOfView = Chess_POV;
        }
        else if (Manager.GameManager.CurrentGame == GameType.Poker)
        {
            Camera.main.transform.position = PokerCamera_Transform.transform.position;
            Camera.main.transform.rotation = PokerCamera_Transform.transform.rotation;
            Camera.main.fieldOfView = Poker_POV;
        }
    }
}
