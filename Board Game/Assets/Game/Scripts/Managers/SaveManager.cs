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
    public static string SfxVolumeKey = "SfxVolume";
    public static string MusicVolumeKey = "MusicVolume";
    public static string CameraSnappingKey = "CameraSnapping";
    public static string ChessDefaultTimeKey = "chessTime";
    public static string ChessDefaultBonusKey = "chessBonus";
    public static string ChessDefaultDifficultyKey = "difficultyBonus";


    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }
    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }
    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }
    public static string GetString(string key)
    {
        return PlayerPrefs.GetString(key);
    }
    public static int GetInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }
    public static float GetFloat(string key, float defaultValue = 0)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }   
    public static string GetTutorialSaveKey(GameType game)
    {
        return game.ToString() + SaveManager.TutorialSaveKey;
    }
}

