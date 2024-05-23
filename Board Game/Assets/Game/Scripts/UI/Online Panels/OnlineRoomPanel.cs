using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnlineRoomPanel : Panel
{
    public TMP_Text WaitingText;

    public Button ReadyButton;
    public Button LeaveButton;
    public Button StartButton;
    public Button ChangeBetButton;

    public Slider BetSlider;
    public TMP_Text BetText;
    public TMP_Text ReadyButtonText;

    public PlayerRoomItem PlayerRoomItemPrefab;
    public Transform PlayerListContent;


    public override void Awake()
    {
        base.Awake();

        ReadyButton.onClick.AddListener(OnReadyPressed);
        LeaveButton.onClick.AddListener(OnLeavePressed);
        StartButton.onClick.AddListener(OnStartPressed);
        ChangeBetButton.onClick.AddListener(OnChangeBetPressed);
        BetSlider.onValueChanged.AddListener(OnBetChanged);

        PhotonManager.Instance.OnPlayersNumberChange += RefreshUI;
        PhotonManager.Instance.OnPlayersPropertiesUpdated += RefreshUI;
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

        OnBetChanged(BetSlider.value);

        foreach (var oldItem in PlayerListContent.GetComponentsInChildren<PlayerRoomItem>())
            Destroy(oldItem.gameObject);

        var players = PhotonNetwork.PlayerList;

        foreach (var player in players)
        {
            var playerItem = Instantiate(PlayerRoomItemPrefab, PlayerListContent);
            playerItem.SetPlayerInfo(player);
        }

        SetReadyButtonUi(PhotonManager.Instance.GetReadyState());

        /*
        Cross_Scene_Data.players.Clear();

        List<Photon.Realtime.Player> guests = new List<Photon.Realtime.Player>();
        Photon.Realtime.Player masterPlayer = players[0];

        foreach (Photon.Realtime.Player player in players)
        {
            if (player.IsMasterClient)
                masterPlayer = player;
            else
                guests.Add(player);
        }

        Player_Info master_Info = new Player_Info()
        {
            Identity = identity.master,
            PlayerName = masterPlayer.NickName,

            Domino_Rating = (int)masterPlayer.CustomProperties[Cross_Scene_Data.mystats[0].Game_Skill_Key],
            Chess_Rating = (int)masterPlayer.CustomProperties[Cross_Scene_Data.mystats[1].Game_Skill_Key],
            Poker_Rating = (int)masterPlayer.CustomProperties[Cross_Scene_Data.mystats[2].Game_Skill_Key],
            IsPlayerReady = (string)masterPlayer.CustomProperties[PlayerReady_String] == "true" ? true : false
        };


        Cross_Scene_Data.players.Add(master_Info, masterPlayer.ActorNumber);
        if (master_Info.IsPlayerReady)
        {
            Player1_Banner_Highlight.SetActive(true); Debug.Log("Master is ready");
        }
        else
        {
            Player1_Banner_Highlight.SetActive(false); Debug.Log("Master is not ready");
        }

        Player1_Text.text = master_Info.PlayerName;

        if (guests.Count > 0)
        {
            for (int i = 0; i < guests.Count; i++)
            {
                int domino_rating = (int)guests[i].CustomProperties[Cross_Scene_Data.mystats[0].Game_Skill_Key];
                int chess_rating = (int)guests[i].CustomProperties[Cross_Scene_Data.mystats[1].Game_Skill_Key];
                int poker_rating = (int)guests[i].CustomProperties[Cross_Scene_Data.mystats[2].Game_Skill_Key];
                bool isPlayerReady = (string)guests[i].CustomProperties[PlayerReady_String] == "true" ? true : false;

                Player_Info playerInfo = new Player_Info()
                {
                    Identity = identity.master + 1 + i,
                    PlayerName = guests[i].NickName,
                    Chess_Rating = chess_rating,
                    Domino_Rating = domino_rating,
                    Poker_Rating = poker_rating,
                    IsPlayerReady = isPlayerReady
                };

                Cross_Scene_Data.players.Add(playerInfo, guests[i].ActorNumber);

                if (i == 0)
                {
                    Player2_Text.text = guests[i].NickName;

                    if (isPlayerReady)
                        Player2_Banner_Highlight.SetActive(true);
                    else
                        Player2_Banner_Highlight.SetActive(false);
                }
                else if (i == 1)
                {
                    Player3_Text.text = guests[i].NickName;

                    if (isPlayerReady)
                        Player3_Banner_Highlight.SetActive(true);
                    else
                        Player3_Banner_Highlight.SetActive(false);
                }
                else if (i == 2)
                {
                    Player4_Text.text = guests[i].NickName;

                    if (isPlayerReady)
                        Player4_Banner_Highlight.SetActive(true);
                    else
                        Player4_Banner_Highlight.SetActive(false);
                }
            }
        }

        //Room state based on players count
        if (Manager.GameManager.CurrentGame == GameType.Poker)
        {
            if (PhotonNetwork.PlayerList.Length == 4)
            {
                Player2_Banner.SetActive(true);
                Player3_Banner.SetActive(true);
                Player4_Banner.SetActive(true);

                Start_Button.interactable = true;
            }
            else if (PhotonNetwork.PlayerList.Length == 3)
            {
                Player2_Banner.SetActive(true);
                Player3_Banner.SetActive(true);
                Player4_Banner.SetActive(false);

                Start_Button.interactable = true;
            }
            else if (PhotonNetwork.PlayerList.Length == 2)
            {
                Player2_Banner.SetActive(true);
                Player3_Banner.SetActive(false);
                Player4_Banner.SetActive(false);

                Start_Button.interactable = true;
            }
            else
            {
                Player2_Banner.SetActive(false);
                Player3_Banner.SetActive(false);
                Player4_Banner.SetActive(false);
            }
        }
        if (Manager.GameManager.CurrentGame == GameType.Domino || Manager.GameManager.CurrentGame == GameType.Chess)
        {
            Player3_Banner.SetActive(false);
            Player4_Banner.SetActive(false);

            if (PhotonNetwork.PlayerList.Length == 2)
            {
                Player2_Banner.SetActive(true);

                Start_Button.interactable = true;

                if (PhotonNetwork.IsMasterClient)
                {
                    Room_Message_Text.text = "";
                    loading_icon.SetActive(false);
                }
                else
                {
                    Room_Message_Text.text = "Waiting for the host to Start";
                    loading_icon.SetActive(true);
                }
            }
            else
            {
                Player2_Banner.SetActive(false);
            }
        }


        if (PhotonNetwork.PlayerList.Length >= 2)
        {
            bool EveryOne_Is_Ready = true;

            foreach (var player in Cross_Scene_Data.players)
            {
                if (!player.Key.IsPlayerReady)
                    EveryOne_Is_Ready = false;
            }

            if (EveryOne_Is_Ready)
            {
                Start_Button.interactable = true;

                if (Cross_Scene_Data.PlayingPublic)
                {
                    loading_icon.SetActive(true);
                    Room_Message_Text.text = "Starting";
                    Start_Game();
                    Cross_Scene_Data.where = WhereTo.Lobby;

                }
                else
                {
                    loading_icon.SetActive(true);

                    if (PhotonNetwork.IsMasterClient)
                        Room_Message_Text.text = "";
                    else
                        Room_Message_Text.text = "Waiting for the host to start";
                }
            }
            else
            {
                Room_Message_Text.text = "Waiting for everyone to press Ready";
                Start_Button.interactable = false;

            }
        }
        else
        {
            Start_Button.interactable = false;
            Room_Message_Text.text = "Waiting for other players to join";
            loading_icon.SetActive(true);
        }
        */
    }

    private void OnReadyPressed()
    {
        bool readyState = PhotonManager.Instance.SetMyReadyState();
        SetReadyButtonUi(readyState);
    }

    private void SetReadyButtonUi(bool state)
    {
        ReadyButtonText.text = state ? "Unready" : "Ready";
    }

    private void OnLeavePressed()
    {
        Hide();
        Panel.GetPanel<OnlineModesPanel>().Show();
        PhotonManager.Instance.LeaveRoom();
    }

    private void OnStartPressed()
    {

    }

    private void OnChangeBetPressed()
    {

    }

    private void OnBetChanged(float value)
    {
        BetText.text = value.ToString();
    }
}
