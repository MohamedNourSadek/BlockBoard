using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Public;


public enum WhereTo { GameSelection, Lobby, CreateRoom, JoinRoom, Muliplayer, AI}

public class Connect_ToServer : MonoBehaviourPunCallbacks
{
    [Header("UI Objects References")]
    [SerializeField] GameObject Loading;
    [SerializeField] GameObject Lobby;
    [SerializeField] GameObject Game_Selection_Menu;
    [SerializeField] InputField create;
    [SerializeField] InputField join;
    [SerializeField] GameObject Room;
    [SerializeField] GameObject Master_Buttons;
    [SerializeField] Text ErrorLog;
    [SerializeField] Button Start_Button;
    [SerializeField] Text Player1_Text;
    [SerializeField] GameObject Player1_Banner_Highlight;
    [SerializeField] GameObject Player2_Banner;
    [SerializeField] GameObject Player2_Banner_Highlight;
    [SerializeField] Text Player2_Text;
    [SerializeField] GameObject Player3_Banner;
    [SerializeField] Text Player3_Text;
    [SerializeField] GameObject Player3_Banner_Highlight;
    [SerializeField] GameObject Player4_Banner;
    [SerializeField] Text Player4_Text;
    [SerializeField] GameObject Player4_Banner_Highlight;
    [SerializeField] Text UserID;
    [SerializeField] GameObject Room_Creation;
    [SerializeField] GameObject Room_Join;
    [SerializeField] Text Room_Message_Text;
    [SerializeField] GameObject loading_icon;
    [SerializeField] Button join_button;
    [SerializeField] Text join_room_message;
    [SerializeField] Button host_button;
    [SerializeField] Text host_room_message;
    [SerializeField] GameObject Settings;
    [SerializeField] GameObject Ai_Room;
    [SerializeField] GameObject Multiplayer;
    [SerializeField] GameObject PlayTutorial;
    [SerializeField] AudioMixer Fx_Mixer;
    [SerializeField] AudioMixer Music_Mixer;
    [SerializeField] GameObject Authentication_Screen;
    [SerializeField] GameObject Leader_Boards;
    [SerializeField] GameObject Payment_Page;
    [SerializeField] User_Settings user_settings;
    [SerializeField] GameObject SupportScreen;
    [SerializeField] Text YourCredit;
    [SerializeField] Text ReadyButtonText;
    [SerializeField] Slider Room_BetSlider;
    [SerializeField] Text Room_BetText;
    [SerializeField] Text Room_Current_Bet;
    [SerializeField] Button Change_Bet_Button;

    PhotonView view;

    public string PlayerReady_String = "Ready_STR";
    public static string RoomBet_String = "Room_Bet";



    void Start()
    {
        Time.timeScale = 1f;
        Cross_Scene_Data.Master_Won_LastRound = true;
        Cross_Scene_Data.Current_Master_score = 0;
        Cross_Scene_Data.Current_Guest_score = 0;

        join.onValueChanged.AddListener(OnJoin_Input_Change);
        create.onValueChanged.AddListener(OnHost_Input_Change);
        view = GetComponent<PhotonView>();
        Room_BetSlider.onValueChanged.AddListener(OnRoomBetSlider_Change);
        
        //user_settings.Remember_User();
    }

    public void Connect_to_PhotonServer()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.ConnectUsingSettings();

        string Game = "";
        if (Manager.GameManager.CurrentGame == GameType.Domino)
            Game = "Domino";
        else if (Manager.GameManager.CurrentGame == GameType.Chess)
            Game = "Chess";
        else if (Manager.GameManager.CurrentGame == GameType.Poker)
            Game = "Poker";

        PhotonNetwork.GameVersion = Cross_Scene_Data.Game_Version + Game;
    }
    //Server Event Functions
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();

        UserID.text = PhotonNetwork.GameVersion + "\n";
        UserID.text += PhotonNetwork.AppVersion + "\n";
        UserID.text += PhotonNetwork.AuthValues.UserId;
    }
    public override void OnJoinedLobby()
    {
        if (user_settings.Logged_IN)
            SelectPage();
        else
            Show_Auth();
    }
    public override void OnJoinedRoom()
    {
        Open_Room();
        _Update_Bet_Constrains();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        StartCoroutine(Log_Temp_error(message, 3f));
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        StartCoroutine(Log_Temp_error(message, 3f));
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if(PhotonNetwork.PlayerList.Length == 1)
        {
            Initialize_Room();
        }

        Update_Players();
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Update_Players();

        if (PhotonNetwork.IsMasterClient)
            _Update_Bet_Constrains();
    }
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Room_UI_Set(PhotonNetwork.IsMasterClient);
    }
    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Update_Players();
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged != null)
        {
            string temp = propertiesThatChanged[RoomBet_String].ToString();
            Room_BetSlider.value = System.Int32.Parse(temp);
            Room_Current_Bet.text = Room_BetSlider.value.ToString();

            Change_Bet_Button.interactable = false;
        }
    }
    
    
    //UI functions
    public void cancelConnection()
    {
        PhotonNetwork.Disconnect();
        Cross_Scene_Data.where = WhereTo.Lobby;
        SelectPage();
    }
    public void ShowMultiplayer()
    {
        Cross_Scene_Data.where = WhereTo.Muliplayer;

        Loading.SetActive(true);
        Lobby.SetActive(false);
        Connect_to_PhotonServer();
    }
    public IEnumerator OnMultiplaye_Open()
    {
        yield return new WaitForSeconds(1f);
        YourCredit.text = "You have " + user_settings.Credit_Text.text + " credits";
    }

    public void Show_Settings_Menu()
    {
        Settings.SetActive(true);

        Authentication_Screen.SetActive(false);
        Loading.SetActive(false);
        Multiplayer.SetActive(false);
        Lobby.SetActive(false);
        Room.SetActive(false);
        Room_Join.SetActive(false);
        Room_Creation.SetActive(false);
        Game_Selection_Menu.SetActive(false);
        Ai_Room.SetActive(false);
    }
    public void Show_Auth()
    {
        Authentication_Screen.SetActive(true);

        Settings.SetActive(false);
        Loading.SetActive(false);
        Multiplayer.SetActive(false);
        Lobby.SetActive(false);
        Room.SetActive(false);
        Room_Join.SetActive(false);
        Room_Creation.SetActive(false);
        Game_Selection_Menu.SetActive(false);
        Ai_Room.SetActive(false);
        Leader_Boards.SetActive(false);
    }
    public void Show_Leader_board()
    {
        Leader_Boards.SetActive(true);

        Settings.SetActive(false);
        Loading.SetActive(false);
        Multiplayer.SetActive(false);
        Lobby.SetActive(false);
        Room.SetActive(false);
        Room_Join.SetActive(false);
        Room_Creation.SetActive(false);
        Game_Selection_Menu.SetActive(false);
        Ai_Room.SetActive(false);
        Authentication_Screen.SetActive(false);
    }
    public void Show_Payment_LogIN()
    {
        if (user_settings.Logged_IN)
            Show_PaymentPage();
        else
            Show_Auth();

    }
    public void Show_PaymentPage()
    {
        Payment_Page.SetActive(true);

        Leader_Boards.SetActive(false);
        Settings.SetActive(false);
        Loading.SetActive(false);
        Multiplayer.SetActive(false);
        Lobby.SetActive(false);
        Room.SetActive(false);
        Room_Join.SetActive(false);
        Room_Creation.SetActive(false);
        Game_Selection_Menu.SetActive(false);
        Ai_Room.SetActive(false);
        Authentication_Screen.SetActive(false);
    }
    public void Show_SupportScreen()
    {
        SupportScreen.SetActive(true);

        Settings.SetActive(false);
        Loading.SetActive(false);
        Multiplayer.SetActive(false);
        Lobby.SetActive(false);
        Room.SetActive(false);
        Room_Join.SetActive(false);
        Room_Creation.SetActive(false);
        Game_Selection_Menu.SetActive(false);
        Ai_Room.SetActive(false);
        Authentication_Screen.SetActive(false);
    }

    public void Show_GameSelection()
    {
        Cross_Scene_Data.where = WhereTo.GameSelection;
        SelectPage();
    }
    public void Show_Domino_Lobby()
    {
        Cross_Scene_Data.where = WhereTo.Lobby;
        Manager.GameManager.CurrentGame = GameType.Domino;

        var tutorialManagers = Manager.GetManager<TutorialsManager>();

        if (tutorialManagers.IsTutorialAvailable(Manager.GameManager.CurrentGame))
        {
            Game_Selection_Menu.SetActive(false);
            PlayTutorial.SetActive(true);
        }
        else
        {
            SelectPage();
        }
    }
    public void Show_Chess_Lobby()
    {
        Cross_Scene_Data.where = WhereTo.Lobby;
        Manager.GameManager.CurrentGame = GameType.Chess;

        var tutorialManagers = Manager.GetManager<TutorialsManager>();

        if (tutorialManagers.IsTutorialAvailable(Manager.GameManager.CurrentGame))
        {
            Game_Selection_Menu.SetActive(false);
            PlayTutorial.SetActive(true);
        }
        else
        {
            SelectPage();
        }
    }
    public void Show_Poker_Lobby()
    {
        Cross_Scene_Data.where = WhereTo.Lobby;
        Manager.GameManager.CurrentGame = GameType.Poker;

        var tutorialManagers = Manager.GetManager<TutorialsManager>();

        if (tutorialManagers.IsTutorialAvailable(Manager.GameManager.CurrentGame))
        {
            Game_Selection_Menu.SetActive(false);
            PlayTutorial.SetActive(true);
        }
        else
        {
            SelectPage();
        }
    }
    public void PlayTutorialYes()
    {
        Start_offline_game();
    }
    public void PlayTutorialNo()
    {
        PlayTutorial.SetActive(false);

        var tutorialManagers = Manager.GetManager<TutorialsManager>();
        tutorialManagers.SetTutorialState(Manager.GameManager.CurrentGame, false);
        SelectPage();
    }
    public void Open_Room()
    {
        Lobby.SetActive(false);
        Multiplayer.SetActive(false);
        Room_Creation.SetActive(false);
        Room_Join.SetActive(false);

        Room.SetActive(true);
        Room_UI_Set(PhotonNetwork.IsMasterClient);

        ReadyButtonText.text = "Ready";
        ExitGames.Client.Photon.Hashtable hs = new ExitGames.Client.Photon.Hashtable();
        hs[PlayerReady_String] = "false";
        PhotonNetwork.SetPlayerCustomProperties(hs);

        Update_Players();
    }
    public void Show_CreateRoom()
    {
        Cross_Scene_Data.where = WhereTo.CreateRoom;
        Cross_Scene_Data.PlayingPublic = false;

        SelectPage();
    }
    public void JoinPublic()
    {
        RoomOptions roomOp = new RoomOptions();

        byte MaxPlayers = 2;
        if (Manager.GameManager.CurrentGame == GameType.Poker)
            MaxPlayers = 4;

        roomOp.MaxPlayers = MaxPlayers;
        roomOp.IsVisible = true;
        roomOp.IsOpen = true;

        Cross_Scene_Data.where = WhereTo.Muliplayer;
        Cross_Scene_Data.UseNewMaxScore = false;
        Cross_Scene_Data.PlayingPublic = true;

        PhotonNetwork.JoinRandomOrCreateRoom(null, MaxPlayers, MatchmakingMode.FillRoom,null,null,null,roomOp,null);
    }
    public void Show_JoinRoom()
    {
        Cross_Scene_Data.where = WhereTo.JoinRoom;
        Cross_Scene_Data.PlayingPublic = false;
        SelectPage();
    }
    public void Back_To_Lobby()
    {
        Cross_Scene_Data.where = WhereTo.Lobby;
        SelectPage();
        ErrorLog.text = "";
    }
    public void Back_To_Multiplayer()
    {
        Cross_Scene_Data.where = WhereTo.Muliplayer;
        SelectPage();

        ErrorLog.text = "";
    }
    public void LeaveRoom_ToCreate()
    {
        Cross_Scene_Data.where = WhereTo.CreateRoom;
        PhotonNetwork.LeaveRoom();
        ErrorLog.text = "";
    }
    public void LeaveRoom_ToJoin()
    {
        Cross_Scene_Data.where = WhereTo.JoinRoom;
        PhotonNetwork.LeaveRoom();
        ErrorLog.text = "";
    }
    public void CreateRoom()
    {
        RoomOptions roomOp = new RoomOptions();
        roomOp.IsVisible = false;
        roomOp.IsOpen = true;
        roomOp.MaxPlayers = 2;
        
        if (Manager.GameManager.CurrentGame == GameType.Poker)
            roomOp.MaxPlayers = 4;

        Cross_Scene_Data.UseNewMaxScore = true;


        Cross_Scene_Data.where = WhereTo.CreateRoom;
        PhotonNetwork.CreateRoom(create.text, roomOp);
    }
    public void JoinRoom()
    {
        Cross_Scene_Data.where = WhereTo.JoinRoom;
        PhotonNetwork.JoinRoom(join.text);
    }
    public void LeaveRoom()
    {
        ErrorLog.text = "";
        PhotonNetwork.LeaveRoom();
    }
    public void Ready()
    {
        if (ReadyButtonText.text == "Ready")
        {
            ExitGames.Client.Photon.Hashtable hs = new ExitGames.Client.Photon.Hashtable();
            hs[PlayerReady_String] = "true";
            PhotonNetwork.SetPlayerCustomProperties(hs);

            ReadyButtonText.text = "UnReady";

            view.RPC("Notify_State", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber.ToString(), "Ready", "true");
        }
        else
        {
            ExitGames.Client.Photon.Hashtable hs = new ExitGames.Client.Photon.Hashtable();
            hs[PlayerReady_String] = "false";
            PhotonNetwork.SetPlayerCustomProperties(hs);

            ReadyButtonText.text = "Ready";

            view.RPC("Notify_State", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber.ToString(), "Ready", "false");
        }
    }
    public void Initialize_Room()
    {
        Change_Bet();
    }


    [PunRPC] public void Notify_State(string Sender_Player, string Action, string ActionArg)
    {
        string Message = "";

        if (Action == "Ready")
        {
            if (ActionArg == "true")
            {
                Message = "Player " + Sender_Player + " is ready";

                if (Sender_Player == PhotonNetwork.LocalPlayer.ActorNumber.ToString())
                {
                    ExitGames.Client.Photon.Hashtable hs = new ExitGames.Client.Photon.Hashtable();
                    hs[PlayerReady_String] = "true";
                    PhotonNetwork.SetPlayerCustomProperties(hs);
                    ReadyButtonText.text = "UnReady";
                }

            }
            else
            {
                Message = "Player " + Sender_Player + " is not Ready";

                if (Sender_Player == PhotonNetwork.LocalPlayer.ActorNumber.ToString())
                {
                    ExitGames.Client.Photon.Hashtable hs = new ExitGames.Client.Photon.Hashtable();
                    hs[PlayerReady_String] = "false";
                    PhotonNetwork.SetPlayerCustomProperties(hs);
                    ReadyButtonText.text = "Ready";
                }
            }
        }

    }
    
    
    public void OnRoomBetSlider_Change(float value)
    {
        Room_BetText.text = Room_BetSlider.value.ToString();
        Change_Bet_Button.interactable = true;
    }
    public void Change_Bet()
    {
        foreach(var player in PhotonNetwork.PlayerList)
        {
            view.RPC("Notify_State", RpcTarget.AllBuffered, player.ActorNumber.ToString(), "Ready", "false");

            ExitGames.Client.Photon.Hashtable hs = new ExitGames.Client.Photon.Hashtable();
            hs[RoomBet_String] = Room_BetSlider.value.ToString();

            PhotonNetwork.CurrentRoom.SetCustomProperties(hs);
        }

        Change_Bet_Button.interactable = false;
    }
    public void _Update_Bet_Constrains()
    {
        var players = PhotonNetwork.PlayerList;

        //Getting and setting Max Bet.
        int MaxPossibleBet = int.MaxValue;

        foreach (var p in players)
        {
            int Credit = System.Int32.Parse(p.CustomProperties[User_Settings.Credit_Saved].ToString());

            if (Credit < MaxPossibleBet)
            {
                MaxPossibleBet = Credit;
            }
        }

        Room_BetSlider.minValue = user_settings.MinBet;
        Room_BetSlider.maxValue = MaxPossibleBet;

        Change_Bet();

        view.RPC("Change_Bet_Constrains", RpcTarget.Others, MaxPossibleBet);
    }
    [PunRPC] public void Change_Bet_Constrains(int MaxPossibleBet)
    {
        Room_BetSlider.minValue = user_settings.MinBet;
        Room_BetSlider.maxValue = MaxPossibleBet;
    }

    public void vsAI()
    {
        Cross_Scene_Data.where = WhereTo.AI;
        Cross_Scene_Data.UseNewMaxScore = true;
        SelectPage();
    }
    public void Start_offline_game()
    {
        SceneManager.LoadScene("Game");
        Manager.GameManager.GameMode = GameMode.Offline;
    }
    public void Start_Game()
    {
        Cross_Scene_Data.where = WhereTo.Lobby;

        PhotonNetwork.CurrentRoom.IsOpen = false;
        view.RPC("Start_Game_Sync", RpcTarget.All);
    }

    public void OnJoin_Input_Change(string value)
    {
        if (value.Length < 4)
        {
            join_room_message.text = "Room name must be 4 letters or more.";
            join_button.interactable = false;
        }
        else
        {
            join_room_message.text = "";
            join_button.interactable = true;
        }
    }
    public void OnHost_Input_Change(string value)
    {
        if (value.Length < 4)
        {
            host_room_message.text = "Room name must be 4 letters or more.";
            host_button.interactable = false;
        }
        else
        {
            host_room_message.text = "";
            host_button.interactable = true;
        }
    }
    public void Close_ProflieSettings()
    {
        if (user_settings.Logged_IN)
            SelectPage();
        else
        {
            bool In_OnlineMode = Cross_Scene_Data.where == WhereTo.CreateRoom ||
                                Cross_Scene_Data.where == WhereTo.JoinRoom ||
                                Cross_Scene_Data.where == WhereTo.Muliplayer;

            if (In_OnlineMode)
            {
                Cross_Scene_Data.where = WhereTo.Lobby;
                SelectPage();
            }
            else
            {
                SelectPage();
            }
        }
    }
    public void Open_Email()
    {
        string t =
        "mailto:blockboard.crypto@gmail.com?subject=ContactUs";
        Application.OpenURL(t);
    }
    public void Report_Bug()
    {
        Application.OpenURL("https://forms.gle/hB9bsB8dEispRVyw7");
    }
    public void Give_Feedback()
    {
        Application.OpenURL("https://www.reddit.com/r/BlockBoard_Feedback/");
    }

    [PunRPC] void Start_Game_Sync()
    {
        PhotonNetwork.LoadLevel("Game");
        Manager.GameManager.GameMode = GameMode.Online;
    }
    
    void Update_Players()
    {
        //Adding players data to local storage
        
        var players = PhotonNetwork.PlayerList;
        Cross_Scene_Data.players.Clear();
        List<Photon.Realtime.Player> Guests = new List<Photon.Realtime.Player>();
        Photon.Realtime.Player masterPlayer = players[0];
        foreach (Photon.Realtime.Player player in players)
        {
            if (player.IsMasterClient)
                masterPlayer = player;
            else
                Guests.Add(player);
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
            Player1_Banner_Highlight.SetActive(true); DebugManager.Instance.Log("Master is ready");
        }
        else
        {
            Player1_Banner_Highlight.SetActive(false); DebugManager.Instance.Log("Master is not ready");
        }
        Player1_Text.text = master_Info.PlayerName;
        if (Guests.Count > 0)
        {
            for (int i = 0; i < Guests.Count; i++)
            {
                int domino_rating = (int)Guests[i].CustomProperties[Cross_Scene_Data.mystats[0].Game_Skill_Key];
                int chess_rating = (int)Guests[i].CustomProperties[Cross_Scene_Data.mystats[1].Game_Skill_Key];
                int poker_rating = (int)Guests[i].CustomProperties[Cross_Scene_Data.mystats[2].Game_Skill_Key];
                bool isPlayerReady = (string)Guests[i].CustomProperties[PlayerReady_String] == "true" ? true : false;

                Player_Info playerInfo = new Player_Info() 
                { 
                    Identity = identity.master + 1 + i,
                    PlayerName = Guests[i].NickName,
                    Chess_Rating = chess_rating,
                    Domino_Rating = domino_rating,
                    Poker_Rating = poker_rating,
                    IsPlayerReady = isPlayerReady
                };

                Cross_Scene_Data.players.Add(playerInfo, Guests[i].ActorNumber);

                if (i == 0)
                {
                    Player2_Text.text = Guests[i].NickName;

                    if (isPlayerReady)
                        Player2_Banner_Highlight.SetActive(true);
                    else
                        Player2_Banner_Highlight.SetActive(false);
                }
                else if (i == 1)
                {
                    Player3_Text.text = Guests[i].NickName;

                    if (isPlayerReady)
                        Player3_Banner_Highlight.SetActive(true);
                    else
                        Player3_Banner_Highlight.SetActive(false);
                }
                else if (i == 2)
                {
                    Player4_Text.text = Guests[i].NickName;

                    if (isPlayerReady)
                        Player4_Banner_Highlight.SetActive(true);
                    else
                        Player4_Banner_Highlight.SetActive(false);
                }
            }
        }

        //Room state based on players count
        if(Manager.GameManager.CurrentGame == GameType.Poker)
        {
            if (PhotonNetwork.PlayerList.Length == 4)
            {
                Player2_Banner.SetActive(true);
                Player3_Banner.SetActive(true);
                Player4_Banner.SetActive(true);

                Start_Button.interactable = true;
            }
            else if(PhotonNetwork.PlayerList.Length == 3)
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

                    if(PhotonNetwork.IsMasterClient)
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
    }
    void Room_UI_Set(bool isMaster)
    {
        if(isMaster && !Cross_Scene_Data.PlayingPublic)
             Master_Buttons.SetActive(true);
        else
            Master_Buttons.SetActive(false);
    }
    IEnumerator Log_Temp_error(string message, float time)
    {
        ErrorLog.text = message;
        yield return new WaitForSecondsRealtime(time);
        ErrorLog.text = "";
    }
    public void SelectPage()
    {
        if (Cross_Scene_Data.where == WhereTo.GameSelection)
        {
            Game_Selection_Menu.SetActive(true);

            Payment_Page.SetActive(false);
            Leader_Boards.SetActive(false);
            SupportScreen.SetActive(false);
            Authentication_Screen.SetActive(false);
            Loading.SetActive(false);
            Lobby.SetActive(false);
            Room.SetActive(false);
            Multiplayer.SetActive(false);
            Settings.SetActive(false);
            Ai_Room.SetActive(false);
            PlayTutorial.SetActive(false);
        }
        else if (Cross_Scene_Data.where == WhereTo.Lobby)
        {
            Lobby.SetActive(true);

            Payment_Page.SetActive(false);
            SupportScreen.SetActive(false);
            Authentication_Screen.SetActive(false);
            Leader_Boards.SetActive(false);
            Loading.SetActive(false);
            Game_Selection_Menu.SetActive(false);
            Room.SetActive(false);
            Multiplayer.SetActive(false);
            Settings.SetActive(false);
            Ai_Room.SetActive(false);
            PlayTutorial.SetActive(false);
        }
        else if (Cross_Scene_Data.where == WhereTo.CreateRoom)
        {
            Room_Creation.SetActive(true);

            Payment_Page.SetActive(false);
            SupportScreen.SetActive(false);
            Leader_Boards.SetActive(false);
            Authentication_Screen.SetActive(false);
            Loading.SetActive(false);
            Lobby.SetActive(false);
            Game_Selection_Menu.SetActive(false);
            Room.SetActive(false);
            Multiplayer.SetActive(false);
            Settings.SetActive(false);
            Ai_Room.SetActive(false);
            PlayTutorial.SetActive(false);
        }
        else if (Cross_Scene_Data.where == WhereTo.JoinRoom)
        {
            Room_Join.SetActive(true);

            Payment_Page.SetActive(false);
            SupportScreen.SetActive(false);
            Leader_Boards.SetActive(false);
            Authentication_Screen.SetActive(false);
            Loading.SetActive(false);
            Lobby.SetActive(false);
            Game_Selection_Menu.SetActive(false);
            Room.SetActive(false);
            Multiplayer.SetActive(false);
            Settings.SetActive(false);
            PlayTutorial.SetActive(false);
            Ai_Room.SetActive(false);
        }
        else if (Cross_Scene_Data.where == WhereTo.Muliplayer)
        {
            Multiplayer.SetActive(true);

            Payment_Page.SetActive(false);
            SupportScreen.SetActive(false);
            Leader_Boards.SetActive(false);
            Authentication_Screen.SetActive(false);
            Loading.SetActive(false);
            Lobby.SetActive(false);
            Game_Selection_Menu.SetActive(false);
            Room.SetActive(false);
            Settings.SetActive(false);
            Room_Join.SetActive(false);
            Room_Creation.SetActive(false);
            PlayTutorial.SetActive(false);
            Ai_Room.SetActive(false);

            StartCoroutine(OnMultiplaye_Open());
        }
        else if (Cross_Scene_Data.where == WhereTo.AI)
        {
            Ai_Room.SetActive(true);

            Payment_Page.SetActive(false);
            SupportScreen.SetActive(false);
            Leader_Boards.SetActive(false);
            Authentication_Screen.SetActive(false);
            Loading.SetActive(false);
            Multiplayer.SetActive(false);
            Lobby.SetActive(false);
            Game_Selection_Menu.SetActive(false);
            Room.SetActive(false);
            Settings.SetActive(false);
            Room_Join.SetActive(false);
            PlayTutorial.SetActive(false);
            Room_Creation.SetActive(false);
        }
    }
}


[System.Serializable]
public class Player_Info
{
    [SerializeField] public identity Identity;
    [SerializeField] public string PlayerName;

    [SerializeField] public int Chess_Rating;
    [SerializeField] public int Domino_Rating;
    [SerializeField] public int Poker_Rating;

    [SerializeField] public bool IsPlayerReady;
}
