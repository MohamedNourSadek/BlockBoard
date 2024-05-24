using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cross_Scene_Data : MonoBehaviour
{
    public static Dictionary<Player_Info, int> players = new Dictionary<Player_Info, int>();

    public static List<Game_Stats> mystats = new List<Game_Stats>();
}

public enum identity { master, Guest, Guest2, Guest3 }
