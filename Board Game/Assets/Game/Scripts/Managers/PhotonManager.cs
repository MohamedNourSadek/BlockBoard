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
    public UnityAction OnPhotonFullyConnected;
    public UnityAction<bool, object> OnRoomJoinCallback;
    public UnityAction OnPlayersNumberChange;
    public UnityAction OnPlayersPropertiesUpdated;
    public UnityAction<ExitGames.Client.Photon.Hashtable> OnRoomInfoUpdated;

    public static string PlayerReadyKey = "ReadyState";
    public static string PlayerBetKey = "BetAmount";
    public static string PlayerMaxBetKey = "MaxBetAmount";
    
    private PhotonView view;

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        view = GetComponent<PhotonView>();  
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

        OnPhotonFullyConnected?.Invoke();
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

        bool join = PhotonNetwork.JoinRandomOrCreateRoom(null, maxplayers, MatchmakingMode.FillRoom, null, null, null, roomOp, null);

        if (!join)
        {
            Panel.GetPanel<WaitingPanel>().Hide();

            Panel.GetPanel<MessagePanel>().Show("Error", "Error Trying to find a lobby", ButtonTypes.Ok, ButtonTypes.None, () =>
            {
                Panel.GetPanel<ModeSelectionPanel>().Show();
            });
        }
    }
    public bool SetMyReadyState()
    {
        bool isReady = GetReadyState();

        bool newReadyState = !isReady;

        ExitGames.Client.Photon.Hashtable hs = new ExitGames.Client.Photon.Hashtable();
        hs[PlayerReadyKey] = newReadyState ? "true" : "false";
        PhotonNetwork.SetPlayerCustomProperties(hs);

        return newReadyState;
    }
    public bool GetReadyState()
    {
        return (((string)PhotonNetwork.LocalPlayer.CustomProperties[PlayerReadyKey]) == "true");
    }
    public bool AreAllPlayersReady()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (((string)player.CustomProperties[PlayerReadyKey]) != "true")
                return false;
        }

        return true;
    }
    public bool IsMaster()
    {
        return PhotonNetwork.IsMasterClient;
    }
    public void UpdateRoomBet(int betValue, int maxValue)
    {
        ExitGames.Client.Photon.Hashtable data = new ExitGames.Client.Photon.Hashtable();
        
        data[PlayerBetKey] = betValue;
        data[PlayerMaxBetKey] = maxValue;

        PhotonNetwork.CurrentRoom.SetCustomProperties(data);
    }
    public void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        view.RPC("StartGameSync", RpcTarget.All);
    }


    [PunRPC]
    public void StartGameSync()
    {
        PhotonNetwork.LoadLevel("Game");
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        OnRoomInfoUpdated?.Invoke(propertiesThatChanged);
    }
    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        OnPlayersPropertiesUpdated?.Invoke();
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
