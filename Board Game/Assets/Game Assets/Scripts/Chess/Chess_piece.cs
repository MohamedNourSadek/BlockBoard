using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Chess_Piece_Type { King, Queen, Knight, Bishop, Rock, Pawn}
public enum Piece_Color { White, Black}

public class Chess_piece : MonoBehaviour
{
    [SerializeField] public Chess_Piece_Type piece_type;
    [SerializeField] public Piece_Color color;
    [SerializeField] public Vector2 currentposition;
    [SerializeField] public bool firstMove = true;
    [SerializeField] public bool EnPassent_Possible = false;
    [SerializeField] public Vector3 FinalPosition;
}
