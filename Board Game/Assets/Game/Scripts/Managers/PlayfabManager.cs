using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using UnityEngine.Events;

public class PlayfabManager : Manager
{
    public string TitleID;

    [NonSerialized] public bool IsLoggedIn;

    private UnityAction<bool, object> LoginCallBack;
    private UnityAction<bool, object> ResetCallBack;

    public override void Awake()
    {
        base.Awake();
        ApplyAutomaticLogin();
    }


    public void ApplyAutomaticLogin()
    {
        if (PlayerPrefs.GetString(DataManager.EmailSaveKey) != "")
        {
            string email = PlayerPrefs.GetString(DataManager.EmailSaveKey);
            string password = PlayerPrefs.GetString(DataManager.PassSaveKey);

            Login(email, password);
        }
    }
    public void Login(string email, string password, UnityAction<bool, object> callBack = null)
    {
        PlayerProfile profile = GetManager<ProfileManager>().PlayerProfile;
        profile.Email = email;
        LoginCallBack = callBack;

        PlayerPrefs.SetString(DataManager.EmailSaveKey, email);
        PlayerPrefs.SetString(DataManager.PassSaveKey, password);
        
        GetPlayerCombinedInfoRequestParams requestParameters = new GetPlayerCombinedInfoRequestParams() { GetUserAccountInfo = true };
        var loginRequest = new LoginWithEmailAddressRequest { Email = email, Password = password, InfoRequestParameters = requestParameters, TitleId = TitleID };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnLoginFailure);
    }
    public void CancelLogin()
    {
        LoginCallBack = null;
    }
    public void ResetPassword(string email, UnityAction<bool, object> callBack = null)
    {
        ResetCallBack = callBack;
        var ResetRequest = new SendAccountRecoveryEmailRequest { Email = email, TitleId = TitleID};
        PlayFabClientAPI.SendAccountRecoveryEmail(ResetRequest, OnResetSuccess, OnResetFail);
    }



    private void OnResetSuccess(SendAccountRecoveryEmailResult result)
    {
        ResetCallBack?.Invoke(true, result);
        ResetCallBack = null;
    }
    private void OnResetFail(PlayFabError error)
    {
        ResetCallBack?.Invoke(false, error);
        ResetCallBack = null;
    }
    private void OnLoginSuccess(PlayFab.ClientModels.LoginResult result)
    {
        PlayerProfile profile = GetManager<ProfileManager>().PlayerProfile;
        profile.NiceName = result.InfoResultPayload.AccountInfo.Username;
        GetManager<PhotonManager>().SetPlayerNickName(profile.NiceName);
        IsLoggedIn = true;
        LoginCallBack?.Invoke(true, result);
        LoginCallBack = null;   

        DebugManager.Debug("Login Success");
    }
    private void OnLoginFailure(PlayFabError error)
    {
        LoginCallBack?.Invoke(false, error);
        LoginCallBack = null;
        DebugManager.Debug("Login Fail");
    }


}
