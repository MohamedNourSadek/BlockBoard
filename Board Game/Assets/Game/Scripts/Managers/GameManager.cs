using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Manager
{
    public string GameVersion;
    public GameType CurrentGame = GameType.None;
    public GameMode GameMode = GameMode.None;

    public override void Awake()
    {
        GameManager = this;
    }
}

public enum  GameMode
{
    None, Offline, Online   
}
