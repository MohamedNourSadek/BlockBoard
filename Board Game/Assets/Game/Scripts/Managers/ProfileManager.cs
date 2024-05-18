using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileManager : Manager
{
    private PlayerProfile PlayerProfile = new PlayerProfile();
    

    public PlayerProfile GetPlayerProfile()
    {
        return PlayerProfile;
    }
    public void SetPlayerProfile(PlayerProfile profile)
    {
        PlayerProfile = profile;
    }
}

public class PlayerProfile
{
    public string NickName = "--";
    public string Email = "--";

    public Dictionary<GameType, int> Skill;
    public Dictionary<GameType, int> GamesPlayed;
    public Dictionary<GameType, int> GamesWon;
    public Dictionary<GameType, int> GamesDraw;
    public Dictionary<GameType, int> GamesLost;
}
