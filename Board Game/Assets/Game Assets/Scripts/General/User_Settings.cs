using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System;
using Photon.Pun;

public class User_Settings : MonoBehaviour
{
    [SerializeField] string TitleID;

    [SerializeField] GameObject Main_Screen;
    [SerializeField] GameObject LogIn_Screen;
    [SerializeField] GameObject SignUp_Screen;
    [SerializeField] GameObject Reset_Password_Screen;
    [SerializeField] GameObject UserStats;
    [SerializeField] Connect_ToServer Manager;
    [SerializeField] public bool Logged_IN;

    [Header("UI Reference")]
    [SerializeField] InputField SignUp_UserName;
    [SerializeField] Text SignUp_UserName_Error;
    [SerializeField] InputField SignUp_Email;
    [SerializeField] Text SignUp_Email_Error;
    [SerializeField] InputField SignUp_Password;
    [SerializeField] Text SignUp_Pass_Error;
    [SerializeField] InputField SignIn_Email;
    [SerializeField] Text SignIn_Email_Error;
    [SerializeField] InputField SignIn_Password;
    [SerializeField] Text SignIn_Pass_Error;
    [SerializeField] InputField Reset_Password_Email;
    [SerializeField] Text Reset_Password_Email_error;
    [SerializeField] Text SignUp_Error;
    [SerializeField] Text LogIn_Error;
    [SerializeField] Text Reset_Error;
    [SerializeField] Button Sign_In;
    [SerializeField] Button Sign_Up;
    [SerializeField] Button Reset_Pass;

    [Header("Player stats")]
    [SerializeField] Text playerName_UI;
    [SerializeField] Text playerEmail_UI;
    [SerializeField] public List<Game_Stats> Games_Stats = new List<Game_Stats>();

    [SerializeField] GameObject DominoStats_Screen;
    [SerializeField] GameObject ChessStats_Screen;
    [SerializeField] GameObject PokerStats_Screen;
    [SerializeField] GameObject LeaderBoards_stats;
    [SerializeField] List<Leader_Board_Player> leaderboard_UI = new List<Leader_Board_Player>();
    [SerializeField] Connect_ToServer manager;
    [SerializeField] Text Debug_Error;
    [SerializeField] public Text WalletAddress;
    [SerializeField] InputField WalletAddress_Input;
    [SerializeField] Text Address_Button_Text;
    [SerializeField] public Text Credit_Text;

    [System.NonSerialized] public static string Player_Name = "";
    [System.NonSerialized] public static string Email = "";

    public static string Payment_InProgress = "Payment_In_Progress";
    private string SavedEmail = "Saved_Email";
    private string SavedPass = "Saved_Pass";
    public static string Saved_Address = "Wallet_Address";
    public static string Credit_Saved = "Credit_Amount";
    public static string WithDraw_Req = "WithReq";

    public int MinBet = 100;
    public int MinCredit = 0;
    public int Requested_Amount = 0;

    //Input Corrections
    private void Awake()
    {
        SignUp_UserName.onValueChanged.AddListener(OnSignUp_UserName_Error);
        SignUp_Email.onValueChanged.AddListener(OnSignUp_Email_Error);
        SignUp_Password.onValueChanged.AddListener(OnSignUp_Password_Error);

        SignIn_Email.onValueChanged.AddListener(OnSignIn_Email_Error);
        SignIn_Password.onValueChanged.AddListener(OnSignIn_Password_Error);

        Reset_Password_Email.onValueChanged.AddListener(OnReset_Password_Email_Error);

        Debug_Error.text = " Starting Auth";

        Debug_Error.text = " Activated ";

        // if (!FB.IsInitialized)
        //   FB.Init(() => FB.ActivateApp());
    }


    void OnSignUp_UserName_Error(string input)
    {
        if (input.Length >= 4)
        {
            SignUp_UserName_Error.text = "";

            if (SignUp_Email.text.Length >= 5 && SignUp_Password.text.Length >= 4)
                Sign_Up.interactable = true;
        }
        else
        {
            SignUp_UserName_Error.text = "Your user name should be 4 letters or more";
            Sign_Up.interactable = false;
        }
    }
    void OnSignUp_Email_Error(string input)
    {
        if (input.Length >= 4)
        {
            SignUp_Email_Error.text = "";

            if (SignUp_UserName.text.Length >= 4 && SignUp_Password.text.Length >= 4)
                Sign_Up.interactable = true;
        }
        else
        {
            SignUp_Email_Error.text = "Your email should be 5 letters or more";
            Sign_Up.interactable = false;
        }
    }
    void OnSignUp_Password_Error(string input)
    {
        if (input.Length >= 4)
        {
            SignUp_Pass_Error.text = "";

            if (SignUp_Email.text.Length >= 5 && SignUp_UserName.text.Length >= 4)
                Sign_Up.interactable = true;
        }
        else
        {
            SignUp_Pass_Error.text = "Your password should be 4 letters or more";
            Sign_Up.interactable = false;
        }
    }

    void OnSignIn_Email_Error(string input)
    {
        if (input.Length >= 5)
        {
            SignIn_Email_Error.text = "";

            if (SignIn_Password.text.Length >= 4)
                Sign_In.interactable = true;
        }
        else
        {
            SignIn_Email_Error.text = "Your email should be 5 letters or more";
            Sign_In.interactable = false;
        }
    }
    void OnSignIn_Password_Error(string input)
    {
        if (input.Length >= 4)
        {
            SignIn_Pass_Error.text = "";

            if (SignIn_Email.text.Length >= 5)
                Sign_In.interactable = true;
        }
        else
        {
            SignIn_Pass_Error.text = "Your password should be 4 letters or more";
            Sign_In.interactable = false;
        }
    }
    void OnReset_Password_Email_Error(string input)
    {
        if (input.Length >= 5)
        {
            Reset_Password_Email_error.text = "";
            Reset_Pass.interactable = true;
        }
        else
        {
            Reset_Password_Email_error.text = "Your email should be 5 letters or more";
            Reset_Pass.interactable = false;
        }
    }


    //Helpers
    public bool Logging_In = false;
    public void Remember_User()
    {
        if (PlayerPrefs.GetString(SavedEmail) != "")
        {
            SignIn_Email.text = PlayerPrefs.GetString(SavedEmail);
            SignIn_Password.text = PlayerPrefs.GetString(SavedPass);

            Logging_In = true;
            Log_In();
        }
        else
        {
            Manager.SelectPage();
        }
    }
    string Encrypt(string pass)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] bs = System.Text.Encoding.UTF8.GetBytes(pass);
        bs = x.ComputeHash(bs);
        System.Text.StringBuilder s = new System.Text.StringBuilder();

        foreach (byte b in bs)
            s.Append(b.ToString("x2").ToLower());

        return s.ToString();
    }
    public void Set_New_CreditUI()
    {

    }


    //UI Functions
    public void Show_MainScreen()
    {
        Main_Screen.SetActive(true);

        LogIn_Screen.SetActive(false);
        UserStats.SetActive(false);
        SignUp_Screen.SetActive(false);
        Reset_Password_Screen.SetActive(false);

        PlayerPrefs.SetString(SavedEmail, "");
        PlayerPrefs.SetString(SavedPass, "");
        Logged_IN = false;

        LogIn_Error.text = "";
        SignUp_Error.text = "";
        Reset_Error.text = "";
    }
    public void Show_LogIn_Screen()
    {
        LogIn_Screen.SetActive(true);

        Main_Screen.SetActive(false);
        UserStats.SetActive(false);
        SignUp_Screen.SetActive(false);
        Reset_Password_Screen.SetActive(false);

        LogIn_Error.text = "";
        SignUp_Error.text = "";
        Reset_Error.text = "";
    }
    public void Show_SignUp_Screen()
    {
        SignUp_Screen.SetActive(true);

        Main_Screen.SetActive(false);
        LogIn_Screen.SetActive(false);
        UserStats.SetActive(false);
        Reset_Password_Screen.SetActive(false);


        LogIn_Error.text = "";
        SignUp_Error.text = "";
        Reset_Error.text = "";
    }
    public void Show_Reset_Password()
    {
        Reset_Password_Screen.SetActive(true);

        SignUp_Screen.SetActive(false);
        Main_Screen.SetActive(false);
        LogIn_Screen.SetActive(false);
        UserStats.SetActive(false);

        LogIn_Error.text = "";
        SignUp_Error.text = "";
        Reset_Error.text = "";
    }
    public void Show_User_Stats()
    {
        UserStats.SetActive(true);

        SignUp_Screen.SetActive(false);
        Main_Screen.SetActive(false);
        LogIn_Screen.SetActive(false);
        DominoStats_Screen.SetActive(false);
        ChessStats_Screen.SetActive(false);
        PokerStats_Screen.SetActive(false);
    }
    public void Show_Domino_Stats()
    {
        UserStats.SetActive(false);
        DominoStats_Screen.SetActive(true);
    }
    public void Show_Poker_Stats()
    {
        UserStats.SetActive(false);
        PokerStats_Screen.SetActive(true);
    }
    public void Show_Chess_Stats()
    {
        UserStats.SetActive(false);
        ChessStats_Screen.SetActive(true);
    }


    //Change Wallet Address
    public void Change_Address()
    {
        if(WalletAddress.gameObject.activeSelf)
        {
            WalletAddress.gameObject.SetActive(false);
            WalletAddress_Input.gameObject.SetActive(true);
            WalletAddress_Input.text = WalletAddress.text;
            Address_Button_Text.text = "Confirm";
        }
        else
        {
            WalletAddress.text = WalletAddress_Input.text;
            WalletAddress.gameObject.SetActive(true);
            WalletAddress_Input.gameObject.SetActive(false);
            Address_Button_Text.text = "Change";

            Send_Address_Data();
        }
    }
    public void Send_Address_Data()
    {
        Dictionary<string, string> Data_To_Send = new Dictionary<string, string>();

        Data_To_Send.Add(Saved_Address, WalletAddress.text);

        var request = new UpdateUserDataRequest
        {
            Data = Data_To_Send,
        };

        PlayFabClientAPI.UpdateUserData(request, OnAddress_Send, OnAddress_Fail);
    }

    private void OnAddress_Send(UpdateUserDataResult obj)
    {
    }
    private void OnAddress_Fail(PlayFabError obj)
    {
        Send_Address_Data();
    }

    //SignUp
    public void SignUp()
    {
        var registerRequest = new RegisterPlayFabUserRequest { Email = SignUp_Email.text, Password = SignUp_Password.text, Username = SignUp_UserName.text, TitleId = TitleID };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, RegisterSucess, RegisterFailuer);
    } 
    private void RegisterSucess(RegisterPlayFabUserResult result)
    {
        OnRegisterSucess();
    }
    private void RegisterFailuer(PlayFabError error)
    {
        SignUp_Error.text = error.ErrorMessage;
    }
    void OnRegisterSucess()
    {
        SignUp_Error.text = "";
        LogIn_Error.text = "";

        Intialize_Player_Data();

        Show_LogIn_Screen();
    }


    //Log In
    public void Log_In()
    {
        GetPlayerCombinedInfoRequestParams v = new GetPlayerCombinedInfoRequestParams()
        {
            GetUserAccountInfo = true
        };

        var LogIn_Request = new LoginWithEmailAddressRequest { Email = SignIn_Email.text, Password = SignIn_Password.text, InfoRequestParameters = v, TitleId = TitleID};
        PlayFabClientAPI.LoginWithEmailAddress(LogIn_Request, Login_Sucess, Login_Failuer);
    }
    private void Login_Sucess(PlayFab.ClientModels.LoginResult result)
    {
        OnLogin_Sucess(result);
    }
    private void OnUpdateDisplay(UpdateUserTitleDisplayNameResult obj)
    {
    }
    private void OnUpdateDisplayFail(PlayFabError obj)
    {
    }
    private void Login_Failuer(PlayFabError error)
    {
        Logging_In = false;
        LogIn_Error.text = error.ErrorMessage;
    }
    void OnLogin_Sucess(PlayFab.ClientModels.LoginResult result)
    {
        SignUp_Error.text = "";
        LogIn_Error.text = "";


        playerName_UI.text = result.InfoResultPayload.AccountInfo.Username;
        playerEmail_UI.text = SignIn_Email.text;
        GetData();

        Show_User_Stats();

        PlayerPrefs.SetString(SavedEmail, SignIn_Email.text);
        PlayerPrefs.SetString(SavedPass, SignIn_Password.text);
        PhotonNetwork.NickName = playerName_UI.text;

        Logging_In = false;
        Logged_IN = true;

        Cross_Scene_Data.mystats = Games_Stats;

        Manager.SelectPage();

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = playerName_UI.text
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnUpdateDisplay, OnUpdateDisplayFail);
    }

    public void Login_With_Google()
    {
        Debug_Error.text = "Starting ";

        try
        {
            Social.localUser.Authenticate((bool success) => {

                if (success)
                {
                    Debug_Error.text = "Google Signed In";
                }
                else
                {
                    Debug_Error.text = "Google Failed to Authorize your login";
                }

            });

        }
        catch (Exception e)
        {
            Debug_Error.text = e.Message;

            Debug.Log(e.Message);
        }

    }

    private void Google_Auth_Fail(PlayFabError obj)
    {
        Debug_Error.text = obj.ErrorMessage;
        Debug.Log(obj.ErrorMessage);
    }



    //Reset Password
    public void Reset_Password()
    {

        var ResetRequest = new SendAccountRecoveryEmailRequest { Email = Reset_Password_Email.text, TitleId = TitleID};

        PlayFabClientAPI.SendAccountRecoveryEmail(ResetRequest, OnResetSucess, OnResetFail);

    }
    private void OnResetSucess(SendAccountRecoveryEmailResult resut)
    {
        Reset_Error.text = "Email sent";
    }
    private void OnResetFail(PlayFabError error)
    {
        Reset_Error.text = error.ErrorMessage;
    }


    // Data Sending and recieval
    public void Intialize_Player_Data()
    {
        foreach(var stats in Games_Stats)
        {
            Dictionary<string, string> Data_To_Send = new Dictionary<string, string>();

            Data_To_Send.Add(stats.Game_Skill_Key, "440");
            Data_To_Send.Add(stats.Games_Played_Key, "0");
            Data_To_Send.Add(stats.Games_Won_Key, "0");
            Data_To_Send.Add(stats.Games_Lost_Key, "0");
            Data_To_Send.Add(stats.Games_Draw_Key, "0");
            Data_To_Send.Add(stats.Avg_Op_Key, "0");
            Data_To_Send.Add(Saved_Address, "-----");
            Data_To_Send.Add(Credit_Saved, MinCredit.ToString());
            Data_To_Send.Add(WithDraw_Req, "0");

            var request = new UpdateUserDataRequest
            {
                Data = Data_To_Send,
            };

            PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnSendError);
        }

    }


    private void OnDataSend(UpdateUserDataResult result)
    {
    }
    private void OnSendError(PlayFabError obj)
    {
        if (obj.Error == PlayFabErrorCode.ServiceUnavailable)
        {
            Intialize_Player_Data();
        }
    }


    public delegate void Delg();  // delegate

    public event Delg DataRecieved; // event

    public void GetData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnRecieveError);
    }
    private void OnDataRecieved(GetUserDataResult result)
    {
        WalletAddress.text = result.Data[Saved_Address].Value;
        Credit_Text.text = result.Data[Credit_Saved].Value;
        Requested_Amount = Int32.Parse(result.Data[WithDraw_Req].Value);

        if (Int32.Parse(Credit_Text.text.ToString()) < 20)
            Credit_Text.text = MinCredit.ToString();

        foreach (var stats in Games_Stats)
        {
            stats.Games_Played = Int32.Parse(result.Data[stats.Games_Played_Key].Value);
            stats.Games_Won = Int32.Parse(result.Data[stats.Games_Won_Key].Value);
            stats.Games_Lost = Int32.Parse(result.Data[stats.Games_Lost_Key].Value);
            stats.Games_Draw = Int32.Parse(result.Data[stats.Games_Draw_Key].Value);
            stats.Avg_Op = Int32.Parse(result.Data[stats.Avg_Op_Key].Value);

            stats.Game_Skill = Compute_Skill(stats.Games_Won, stats.Games_Lost, stats.Avg_Op);

            stats.Game_Skill_UI.text = stats.Game_Skill.ToString();
            stats.Games_Played_UI.text = stats.Games_Played.ToString();
            stats.Avg_Oponent_UI.text = stats.Avg_Op.ToString();
            stats.Game_Stats_Stats_UI.text = "( " + stats.Games_Won.ToString() + " , " +
                                                    stats.Games_Lost.ToString() + " ," +
                                                    stats.Games_Draw.ToString() + " )";
        }

        ExitGames.Client.Photon.Hashtable hs = new ExitGames.Client.Photon.Hashtable();

        foreach (var p in Games_Stats)
        {
            hs[p.Game_Skill_Key] = p.Game_Skill;
            hs[p.Games_Played_Key] = p.Games_Played;
            hs[p.Games_Won_Key] = p.Games_Won_Key;
            hs[p.Games_Lost_Key] = p.Games_Lost;
            hs[p.Games_Draw_Key] = p.Games_Draw;
            hs[p.Avg_Op_Key] = p.Avg_Op;
        }

        hs[Credit_Saved] = Credit_Text.text;

        PhotonNetwork.SetPlayerCustomProperties(hs);

        UpdateLeadBoard(CurrentGame.Chess);
        UpdateLeadBoard(CurrentGame.Domino);
        UpdateLeadBoard(CurrentGame.Poker);

        if (DataRecieved != null)
            DataRecieved.Invoke();
    }
    private void OnRecieveError(PlayFabError obj)
    {
    }

    //Update Credit
    
    //LeaderBoards
    public void UpdateLeadBoard(CurrentGame game)
    {
        string Leaderboard_Name = "";
        int Score = 0;

        if (game == CurrentGame.Domino)
        {
            Leaderboard_Name = "Domino_LeaderBoard";
            Score = Games_Stats[0].Game_Skill;
        }
        else if (game == CurrentGame.Chess)
        {
            Leaderboard_Name = "Chess_LeaderBoard";
            Score = Games_Stats[1].Game_Skill;
        }
        else if (game == CurrentGame.Poker)
        {
            Leaderboard_Name = "Poker_LeaderBoard";
            Score = Games_Stats[2].Game_Skill;
        }

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = Leaderboard_Name,
                    Value = Score
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderBoardUpdate, OnLeaderBoardError);
    }
    private void OnLeaderBoardUpdate(UpdatePlayerStatisticsResult result)
    {
    }
    private void OnLeaderBoardError(PlayFabError error)
    {
    }
    
    public void ShowLeaderBoard_MainScreen()
    {
        LeaderBoards_stats.SetActive(false);
        GetLeaderBoard(Cross_Scene_Data.currentGame);
    }
    public void GetLeaderBoard(CurrentGame game)
    {
        string Leaderboard_Name = "";
        if (game == CurrentGame.Domino)
        {
            Leaderboard_Name = "Domino_LeaderBoard";
        }
        else if (game == CurrentGame.Chess)
        {
            Leaderboard_Name = "Chess_LeaderBoard";
        }
        else if (game == CurrentGame.Poker)
        {
            Leaderboard_Name = "Poker_LeaderBoard";
        }

        PlayerProfileViewConstraints constrains = new PlayerProfileViewConstraints() { ShowDisplayName = true };

        var request = new GetLeaderboardRequest
        {
            StatisticName = Leaderboard_Name,
            StartPosition = 0,
            MaxResultsCount = leaderboard_UI.Count, ProfileConstraints = constrains
        };

        PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderBoard, OnLeaderBoardFail);

    }
    private void OnGetLeaderBoard(GetLeaderboardResult result)
    {
        for(int i = 0; i < result.Leaderboard.Count; i++)
        {
            leaderboard_UI[i].playerName.text = result.Leaderboard[i].DisplayName;
            leaderboard_UI[i].player_Score.text = result.Leaderboard[i].StatValue.ToString();
        }

        LeaderBoards_stats.SetActive(true);
    }
    private void OnLeaderBoardFail(PlayFabError Error)
    {
        manager.SelectPage();
    }

    public static int Compute_Skill(int GamesWon, int GamesLost, int AvgOpp)
    {
        int Sum_Games = GamesWon - GamesLost;
        int Skill = 0;

        if(Sum_Games >= 0)
        {
            Skill = AvgOpp + (4 * Sum_Games) + 440;
        }
        else
        {
            Skill = AvgOpp + 40 + (int)(400 * (Math.Pow(2, Sum_Games)));
        }

        return Skill;
    }



    [SerializeField] string Address;

    private void Update()
    {
        if(Input.GetKeyDown("x"))
        {
            if (Input.GetKeyDown("x"))
            {
                var Request = new ExecuteCloudScriptRequest
                {
                    FunctionName = "FindPlayer",
                    FunctionParameter = new
                    {
                        _address = Address
                    }
                };

                PlayFabClientAPI.ExecuteCloudScript(Request, OnSucess, OnError);
            }
        }
    }

    private void OnSucess(ExecuteCloudScriptResult obj)
    {
        Debug.Log(obj.FunctionResult.ToString());
    }

    private void OnError(PlayFabError obj)
    {
        Debug.Log(obj.ErrorMessage);
    }
}


[System.Serializable]
public class Game_Stats
{
    [SerializeField] String GameName;

    [SerializeField] public Text Game_Skill_UI;
    [SerializeField] public Text Games_Played_UI;
    [SerializeField] public Text Avg_Oponent_UI;
    [SerializeField] public Text Game_Stats_Stats_UI;

    [SerializeField] public string Game_Skill_Key;
    [SerializeField] public string Games_Played_Key;
    [SerializeField] public string Avg_Op_Key;
    [SerializeField] public string Games_Won_Key;
    [SerializeField] public string Games_Lost_Key;
    [SerializeField] public string Games_Draw_Key;


    [SerializeField] public int Game_Skill = 0;
    [SerializeField] public int Games_Played = 0;
    [SerializeField] public int Avg_Op = 0;
    [SerializeField] public int Games_Won = 0;
    [SerializeField] public int Games_Lost = 0;
    [SerializeField] public int Games_Draw = 0;
}


[System.Serializable]
public class Leader_Board_Player
{
    [SerializeField] public Text playerName;
    [SerializeField] public Text player_Score;
}
