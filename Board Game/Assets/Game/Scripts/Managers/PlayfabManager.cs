using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using UnityEngine.Events;
using Newtonsoft.Json;

public class PlayfabManager : Manager
{
    public string TitleID;
    public bool IsLoggedIn;

    private UnityAction<bool, object> LoginCallBack;
    private UnityAction<bool, object> SignUpCallBack;
    private UnityAction<bool, object> ResetCallBack;
    
    public override void Awake()
    {
        base.Awake();
        ApplyAutomaticLogin();
    }

    #region Public Functions
    public void ApplyAutomaticLogin()
    {
        if (SaveManager.GetString(SaveManager.EmailSaveKey) != "")
        {
            string email = SaveManager.GetString(SaveManager.EmailSaveKey);
            string password = SaveManager.GetString(SaveManager.PassSaveKey);

            Login(email, password);
        }
    }
    public void Login(string email, string password, UnityAction<bool, object> callBack = null)
    {
        LoginCallBack = callBack;

        SaveManager.SetString(SaveManager.EmailSaveKey, email);
        SaveManager.SetString(SaveManager.PassSaveKey, password);
        
        GetPlayerCombinedInfoRequestParams requestParameters = new GetPlayerCombinedInfoRequestParams() { GetUserAccountInfo = true };
        var loginRequest = new LoginWithEmailAddressRequest { Email = email, Password = password, InfoRequestParameters = requestParameters, TitleId = TitleID };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnLoginFailure);
    }
    public void SignUp(string username, string email, string password, UnityAction<bool, object> callBack = null)
    {
        SignUpCallBack = callBack;

        SaveManager.SetString(SaveManager.EmailSaveKey, email);
        SaveManager.SetString(SaveManager.PassSaveKey, password);

        var registerRequest = new RegisterPlayFabUserRequest { Email = email, Password = password, Username = username, TitleId = TitleID };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnSignUpSuccess, OnSignUpFail);
    }
    public void ResetPassword(string email, UnityAction<bool, object> callBack = null)
    {
        ResetCallBack = callBack;
        var ResetRequest = new SendAccountRecoveryEmailRequest { Email = email, TitleId = TitleID};
        PlayFabClientAPI.SendAccountRecoveryEmail(ResetRequest, OnResetSuccess, OnResetFail);
    }
    public void Logout()
    {
        IsLoggedIn = false;
        SaveManager.SetString(SaveManager.EmailSaveKey, "");
        SaveManager.SetString(SaveManager.PassSaveKey, "");
    }
    public void GetUserData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnDataRecieveError);
    }
    public void SaveUserData()
    {
        Dictionary<string, string> data = new Dictionary<string, string>();

        string userProfile = JsonConvert.SerializeObject(GetManager<ProfileManager>().GetPlayerProfile());
        data.Add(SaveManager.PlayerProfileKey, userProfile);

        var request = new UpdateUserDataRequest { Data = data };
        PlayFabClientAPI.UpdateUserData(request, OnDataSent, OnDataSendError);
    }
    public void CancelLogin()
    {
        LoginCallBack = null;
    }
    public void CancelSignUp()
    {
        SignUpCallBack = null;
    }
    #endregion


    #region Private Functions
    private void SetPlayerFirstTimeData(string userName)
    {
        PlayerProfile profile = new PlayerProfile();

        profile.Email = SaveManager.GetString(SaveManager.EmailSaveKey);
        profile.NickName = userName;

        profile.Skill = new Dictionary<GameType, int>();
        profile.GamesPlayed = new Dictionary<GameType, int>();
        profile.GamesWon = new Dictionary<GameType, int>();
        profile.GamesDraw = new Dictionary<GameType, int>();
        profile.GamesLost = new Dictionary<GameType, int>();
        profile.AvgOpponentSkill = new Dictionary<GameType, int>();

        foreach (var game in Enum.GetValues(typeof(GameType)))
        {
            if((GameType)game != GameType.None)
            {
                profile.Skill.Add((GameType)game, 0);
                profile.GamesPlayed.Add((GameType)game, 0);
                profile.GamesWon.Add((GameType)game, 0);
                profile.GamesDraw.Add((GameType)game, 0);
                profile.AvgOpponentSkill.Add((GameType)game, 0);
                profile.GamesLost.Add((GameType)game, 0);
            }
        }

        GetManager<ProfileManager>().SetPlayerProfile(profile);

        DebugManager.Instance.Log("Creating Data for a newly created account");

        SaveUserData();
    }
    #endregion

    #region Callbacks
    private void OnSignUpSuccess(RegisterPlayFabUserResult result)
    {
        SignUpCallBack?.Invoke(true, result);
        SignUpCallBack = null;

        SetPlayerFirstTimeData(result.Username);
        ApplyAutomaticLogin();

        DebugManager.Instance.Log("Sign up success");
    }
    private void OnSignUpFail(PlayFabError error)
    {
        SignUpCallBack?.Invoke(false, error);
        SignUpCallBack = null;

        DebugManager.Instance.Log("Sign up Failed");
    }
    private void OnResetSuccess(SendAccountRecoveryEmailResult result)
    {
        ResetCallBack?.Invoke(true, result);
        ResetCallBack = null;

        DebugManager.Instance.Log("Password reset email sent");
    }
    private void OnResetFail(PlayFabError error)
    {
        ResetCallBack?.Invoke(false, error);
        ResetCallBack = null;

        DebugManager.Instance.Log("Password reset failed");
    }
    private void OnLoginSuccess(PlayFab.ClientModels.LoginResult result)
    {
        IsLoggedIn = true;
        LoginCallBack?.Invoke(true, result);
        LoginCallBack = null;

        if(result.NewlyCreated)
            SetPlayerFirstTimeData(result.InfoResultPayload.AccountInfo.Username);
        else
            GetUserData();

        DebugManager.Instance.Log("Login Success");
    }
    private void OnLoginFailure(PlayFabError error)
    {
        LoginCallBack?.Invoke(false, error);
        LoginCallBack = null;

        DebugManager.Instance.Log("Login Fail");
    }
    private void OnDataRecieveError(PlayFabError error)
    {
        Logout();
        DebugManager.Instance.Log("Data recieved Fail");
    }
    private void OnDataRecieved(GetUserDataResult result)
    {
        DebugManager.Instance.Log("Data recieve Success");

        if(result.Data.ContainsKey(SaveManager.PlayerProfileKey))
        {
            PlayerProfile playerProfile = new PlayerProfile();
            string userData = result.Data[SaveManager.PlayerProfileKey].Value;
            playerProfile = JsonConvert.DeserializeObject<PlayerProfile>(userData);

            GetManager<ProfileManager>().SetPlayerProfile(playerProfile);
        }

        DebugManager.Instance.LogWarning("Need to update this");

        /*
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

        UpdateLeadBoard(GameType.Chess);
        UpdateLeadBoard(GameType.Domino);
        UpdateLeadBoard(GameType.Poker);
        
        GetManager<PhotonManager>().SetPlayerNickName(profile.NiceName);
        */
    }
    private void OnDataSent(UpdateUserDataResult result)
    {
        DebugManager.Instance.Log("Data send Success");
    }   
    private void OnDataSendError(PlayFabError error)
    {
        DebugManager.Instance.LogError("Data send Fail, Sending Agin");
        SaveUserData();
    }
    #endregion
}
