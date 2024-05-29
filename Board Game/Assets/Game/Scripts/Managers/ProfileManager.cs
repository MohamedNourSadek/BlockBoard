using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProfileManager : Manager
{
    [SerializeField] private PlayerProfile PlayerProfile = new PlayerProfile();

    public UnityAction OnProfileDataReceived;

    public  PlayerProfile GetPlayerProfile() 
    {
        return PlayerProfile;
    }

    public void SetPlayerProfile(PlayerProfile profile)
    {
        PlayerProfile = profile;
        OnProfileDataReceived?.Invoke();
    }
}

public class PlayerProfile
{
    public string NickName = "--";
    public string Email = "--";
    public int Credit = 100;

    public Dictionary<GameType, int> Skill;
    public Dictionary<GameType, int> GamesPlayed;
    public Dictionary<GameType, int> GamesWon;
    public Dictionary<GameType, int> GamesDraw;
    public Dictionary<GameType, int> GamesLost;
}
