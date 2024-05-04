using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Manager
{
    public GameType CurrentGame = GameType.None;

    public override void Awake()
    {
        GameManager = this;
    }
}
