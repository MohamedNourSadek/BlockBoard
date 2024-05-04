using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileManager : Manager
{
    [NonSerialized] public PlayerProfile PlayerProfile = new PlayerProfile();
}

public class PlayerProfile
{
    public string NiceName;
    public string Email;

    public Dictionary<GameType, int> Skill;
    public Dictionary<GameType, int> GamesPlayed;
    public Dictionary<GameType, int> GamesWon;
    public Dictionary<GameType, int> GamesDraw;
    public Dictionary<GameType, int> GamesLost;
}
