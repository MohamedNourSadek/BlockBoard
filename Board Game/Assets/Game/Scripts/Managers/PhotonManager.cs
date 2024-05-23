using Photon.Pun;
using Photon.Realtime;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance;

    public UnityAction<bool, object> OnRoomJoinCallback;
    public UnityAction OnPlayersNumberChange;

    public static string PlayerReadyKey = "ReadyState";

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ConnectToPhoton()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.ConnectUsingSettings();

        string gameName = Manager.GameManager.CurrentGame.ToString();
        PhotonNetwork.GameVersion = Manager.GameManager.GameVersion + gameName;
    }
    public void SetPhotonOnlineSettings()
    {
        if(!PhotonNetwork.InLobby)
        {
            ConnectToPhoton();
            return;
        }

        var profile = Manager.GetManager<ProfileManager>().GetPlayerProfile();

        if(profile != null)
        {
            PhotonNetwork.NickName = profile.NickName;

            ExitGames.Client.Photon.Hashtable data = new ExitGames.Client.Photon.Hashtable();

            foreach (var gameType in Enum.GetValues(typeof(GameType)))
            {
                var type = (GameType)gameType;

                if(type == GameType.None)
                    continue;

                data.Add(type.ToString() + "_Skill", profile.Skill[type]);
                data.Add(type.ToString() + "_GamesPlayed", profile.GamesPlayed[type]);
                data.Add(type.ToString() + "_GamesWon", profile.GamesWon[type]);
                data.Add(type.ToString() + "_GamesLost" , profile.GamesLost[type]);
                data.Add(type.ToString() + "_GamesDraw" , profile.GamesDraw[type]);
                data.Add(type.ToString() + "_AvgOp", profile.AvgOpponentSkill[type]);
            }

            data.Add(PlayerReadyKey, "false");

            PhotonNetwork.SetPlayerCustomProperties(data);

            Debug.Log("Player Data Set Successfully");

            /*
            UpdateLeadBoard(GameType.Chess);
            UpdateLeadBoard(GameType.Domino);
            UpdateLeadBoard(GameType.Poker);
            */
        }
    }
    public void JoinRoom(string roomName)
    {
        Debug.Log(PhotonNetwork.GameVersion);

        PhotonNetwork.JoinRoom(roomName);
    }
    public void CreateRoom(string roomName)
    {
        Debug.Log(PhotonNetwork.GameVersion);

        RoomOptions roomOp = new RoomOptions();
        
        roomOp.IsVisible = false;
        roomOp.IsOpen = true;
        roomOp.MaxPlayers = 2;

        if (Manager.GameManager.CurrentGame == GameType.Poker)
            roomOp.MaxPlayers = 4;

        Cross_Scene_Data.UseNewMaxScore = true;
        
        PhotonNetwork.CreateRoom(roomName, roomOp);
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public void JoinPublic()
    {
        RoomOptions roomOp = new RoomOptions();

        byte maxplayers = 2;
        
        if (Manager.GameManager.CurrentGame == GameType.Poker)
            maxplayers = 4;

        roomOp.MaxPlayers = maxplayers;
        roomOp.IsVisible = true;
        roomOp.IsOpen = true;

        Cross_Scene_Data.where = WhereTo.Muliplayer;
        Cross_Scene_Data.UseNewMaxScore = false;
        Cross_Scene_Data.PlayingPublic = true;

        PhotonNetwork.JoinRandomOrCreateRoom(null, maxplayers, MatchmakingMode.FillRoom, null, null, null, roomOp, null);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        SetPhotonOnlineSettings();
    }
    public override void OnJoinedRoom()
    {
        OnRoomJoinCallback?.Invoke(true , null);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        OnRoomJoinCallback?.Invoke(false, message);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        OnRoomJoinCallback?.Invoke(false, message);
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        OnPlayersNumberChange?.Invoke();
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        OnPlayersNumberChange?.Invoke();
    }
}
