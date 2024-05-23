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
    public UnityAction OnRoomLeft;

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
            string gameName = Manager.GameManager.CurrentGame.ToString();
            
            PhotonNetwork.GameVersion = Manager.GameManager.GameVersion + gameName;
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
        PhotonNetwork.JoinRoom(roomName);
    }
    public void CreateRoom(string roomName)
    {
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
}
