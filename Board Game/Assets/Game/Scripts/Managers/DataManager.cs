using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Manager
{
    public Dictionary<GameType, GameDataItem> GameTypeData = new Dictionary<GameType, GameDataItem>();

    public List<string> DominoTutorialStrings = new List<string>();
    public List<string> ChessTutorialStrings = new List<string>();
    public List<string> PokerTutorialStrings = new List<string>();
}

