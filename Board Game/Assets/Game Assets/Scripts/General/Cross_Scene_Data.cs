using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum identity { master, Guest, Guest2, Guest3 }

public class Cross_Scene_Data : MonoBehaviour
{
    public static bool In_Domino_Game;
    public static bool In_Chess_Game;
    public static bool In_Poker_Game;
    public static float Current_Master_score;
    public static float Current_Guest_score;
    public static bool Master_Won_LastRound;
    public static WhereTo where = WhereTo.GameSelection;
    public static bool UseNewMaxScore;
    public static bool AI;
    public static string Game_Version = "0.31";
    public static float AI_MaxScore = 100;
    public static float chess_Time = 10;
    public static float chess_bonus = 0;
    public static float chess_difficulty = 50f;
    public static float BetAmount = 100f;
    public static bool PlayingPublic;
    public static Dictionary<Player_Info, int> players = new Dictionary<Player_Info, int>();
    public static bool Camera_Snap = true;

    public static Toggle.ToggleEvent Camera_Snap_Event = new Toggle.ToggleEvent();

    public static List<Game_Stats> mystats = new List<Game_Stats>();
}
