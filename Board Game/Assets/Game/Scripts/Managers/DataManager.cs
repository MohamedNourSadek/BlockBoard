using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Manager
{
    public Dictionary<GameType, GameDataItem> GameTypeData = new Dictionary<GameType, GameDataItem>();


    //All Save Keys
    public static string EmailSaveKey = "Email";
    public static string PassSaveKey = "Pass";
    public static string TutorialSaveKey = "Tutorial";

    public static string PlayerProfileKey = "PlayerProfile";
}

