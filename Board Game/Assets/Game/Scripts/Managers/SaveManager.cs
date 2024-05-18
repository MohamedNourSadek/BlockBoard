using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : Manager
{
    //All Save Keys
    public static string EmailSaveKey = "Email";
    public static string PassSaveKey = "Pass";
    public static string TutorialSaveKey = "Tutorial";
    public static string PlayerProfileKey = "PlayerProfile";

    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }
    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }
    public static string GetString(string key)
    {
        return PlayerPrefs.GetString(key);
    }
    public static int GetInt(string key)
    {
        return PlayerPrefs.GetInt(key);
    }

    public static string GetTutorialSaveKey(GameType game)
    {
        return game.ToString() + SaveManager.TutorialSaveKey;
    }
}

