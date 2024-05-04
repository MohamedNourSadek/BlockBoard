using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class PlayfabManager : Manager
{
    public string TitleID;

    [NonSerialized] public bool IsLoggedIn;

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
    public void Login(string email, string password)
    {
        PlayerProfile profile = GetManager<ProfileManager>().PlayerProfile;
        profile.Email = email;

        PlayerPrefs.SetString(DataManager.EmailSaveKey, email);
        PlayerPrefs.SetString(DataManager.PassSaveKey, password);
        
        GetPlayerCombinedInfoRequestParams requestParameters = new GetPlayerCombinedInfoRequestParams() { GetUserAccountInfo = true };
        var loginRequest = new LoginWithEmailAddressRequest { Email = email, Password = password, InfoRequestParameters = requestParameters, TitleId = TitleID };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnLoginFailure);
    }
    private void OnLoginSuccess(PlayFab.ClientModels.LoginResult result)
    {
        PlayerProfile profile = GetManager<ProfileManager>().PlayerProfile;
        profile.NiceName = result.InfoResultPayload.AccountInfo.Username;
        GetManager<PhotonManager>().SetPlayerNickName(profile.NiceName);
        IsLoggedIn = true;

        DebugManager.Debug("Login Success");
    }
    private void OnLoginFailure(PlayFabError error)
    {
        DebugManager.Debug("Login Fail");
    }
}
