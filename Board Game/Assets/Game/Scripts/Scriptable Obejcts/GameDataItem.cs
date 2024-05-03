using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GameDataItem", menuName = "Game Data Item")]
public class GameDataItem : ScriptableObject
{
    public GameType GameType;
    public Sprite GameIcon;
}

public enum GameType
{
    Domino,
    Chess,
    Poker
}
