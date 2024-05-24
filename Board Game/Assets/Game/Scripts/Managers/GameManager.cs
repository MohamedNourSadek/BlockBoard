using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Manager
{
    public string GameVersion;
    public GameType CurrentGame = GameType.None;
    public GameMode GameMode = GameMode.None;

    public DominoSettings DominoSettings = new DominoSettings();    
    public ChessSettings ChessSettings = new ChessSettings();
    public PokerSettings PokerSettings = new PokerSettings();

    public int[] CurrentScore = new int[] { 0, 0, 0, 0 };
    public bool MasterWonLastGame = false;  

    public override void Awake()
    {
        if(GameManager == null)
            GameManager = this;
        else
            Destroy(gameObject);

        base.Awake();
    }
}

public class DominoSettings
{
    public int DominoWinScore 
    {
        set 
        {
            SaveManager.SetInt(SaveManager.DominoDefaultScoreKey, value);
        }
        get 
        {
            return SaveManager.GetInt(SaveManager.DominoDefaultScoreKey, 100);
        } 
    }
}
public class ChessSettings
{
    public int ChessTime
    {
        set
        {
            SaveManager.SetInt(SaveManager.ChessDefaultTimeKey, value);
        }
        get
        {
            return SaveManager.GetInt(SaveManager.ChessDefaultTimeKey, 30);
        }
    }
    public int ChessBonusTime
    {
        set
        {
            SaveManager.SetInt(SaveManager.ChessDefaultBonusKey, value);
        }
        get
        {
            return SaveManager.GetInt(SaveManager.ChessDefaultBonusKey, 60);
        }
    }
    public int ChessDifficulty
    {
        set
        {
            SaveManager.SetInt(SaveManager.ChessDefaultDifficultyKey, value);
        }
        get
        {
            return SaveManager.GetInt(SaveManager.ChessDefaultDifficultyKey, 50);
        }
    }
}

public class PokerSettings
{
    public int PokerBetAmount   
    {
        set
        {
            SaveManager.SetInt(SaveManager.PokerDefaultScoreKey, value);
        }
        get
        {
            return SaveManager.GetInt(SaveManager.PokerDefaultScoreKey, 100);
        }
    }

}

public enum  GameMode
{
    None, Offline, Online   
}
