using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TileName { Bland, One, Two, Three, Four, Five, Six}
public enum CardSide { Up, Down }

[System.Serializable]
public class DominoTile : MonoBehaviour
{
    [SerializeField] public bool Double;
    [SerializeField] public TileName Up;
    [SerializeField] public TileName Down;
    [SerializeField] public Vector3 FinalPosition;
}
