using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


//Moralis
using MoralisWeb3ApiSdk;
using Moralis.Platform.Objects;
using Moralis.Platform.Operations;


//WalletConnect
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Unity;
using Nethereum.Hex.HexTypes;


public class AppManager : MonoBehaviour
{
    #region PUBLIC_FIELDS

    public MoralisController moralisController;
    public WalletConnect walletConnect;

    [Header("UI Elements")]
    public GameObject editorPanel;
    public GameObject mobilePanel;
    public GameObject loggedInPanel;
    public GameObject connectButton;
    public Text walletAddress;
    public Text infoText;
    public Crypto_Payment Payment_Manger;
    public Text User_Guide;
    private String Address;

    #endregion

    #region UNITY_LIFECYCLE

    private async void Start()
    {
        if (moralisController != null)
        {
            await moralisController.Initialize();

            if (Application.isEditor)
            {
                mobilePanel.SetActive(false);
            }
            else
            {
                editorPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("MoralisController not found.");
        }
    }

    private void OnApplicationQuit()
    {
        LogOut();
    }

    #endregion

    #region WALLET_CONNECT

    public Text obj;
    public async void WalletConnectHandler(WCSessionData data)
    {
        Debug.Log("Wallet connection received");
        // Extract wallet address from the Wallet Connect Session data object.
        string address = data.accounts[0].ToLower();
        string appId = MoralisInterface.GetClient().ApplicationId;
        long serverTime = 0;

        // Retrieve server time from Moralis Server for message signature
        Dictionary<string, object> serverTimeResponse = await MoralisInterface.GetClient().Cloud.RunAsync<Dictionary<string, object>>("getServerTime", new Dictionary<string, object>());

        if (serverTimeResponse == null || !serverTimeResponse.ContainsKey("dateTime") ||
            !long.TryParse(serverTimeResponse["dateTime"].ToString(), out serverTime))
        {
            Debug.Log("Failed to retrieve server time from Moralis Server!");
        }

        Debug.Log($"Sending sign request for {address} ...");

        try
        {
            string signMessage = $"Moralis Authentication\n\nId: {appId}:{serverTime}";
            string response = await walletConnect.Session.EthPersonalSign(address, signMessage);

            Debug.Log($"Signature {response} for {address} was returned.");

            // Create moralis auth data from message signing response.
            Dictionary<string, object> authData = new Dictionary<string, object> { { "id", address }, { "signature", response }, { "data", signMessage } };

            obj.text = "Logged in";
            Debug.Log("Logging in user.");

            // Attempt to login user.
            MoralisUser user = await MoralisInterface.LogInAsync(authData);
            obj.text = "Logged in 2";

            Debug.Log($"User {user.username} logged in successfully. ");
            infoText.text = "Logged in successfully!";

            Payment_Manger.BNB_Show_Payment_Parameters();
        }
        catch (Exception exp)
        {
            infoText.text = exp.Message;
            obj.text = exp.Message;

        }

        UserLoggedInHandler();
    }

    public void WalletConnectSessionEstablished(WalletConnectUnitySession session)
    {
        InitializeWeb3();
    }

    private void InitializeWeb3()
    {
        MoralisInterface.SetupWeb3();
    }

    #endregion

    #region PRIVATE_METHODS

    private async void UserLoggedInHandler()
    {
        var user = await MoralisInterface.GetUserAsync();

        if (user != null)
        {
            mobilePanel.SetActive(false);
            editorPanel.SetActive(false);
            loggedInPanel.SetActive(true);
        }
    }

    private async void LogOut()
    {
        await walletConnect.Session.Disconnect();
        walletConnect.CLearSession();

        await MoralisInterface.LogOutAsync();
    }

    #endregion

    #region EDITOR_METHODS

    public void HandleWalletConnected()
    {
        connectButton.SetActive(false);
        infoText.text = "Connection successful. Please sign message";


    }


    public async void ExecuteTransaction()
    {
        string toAddress = "0x72DBAf32Af4a2fe9c60751020CaAF71E92aFDF12";

        try
        {
            User_Guide.text = "Switch Screen to your wallet";

            var txnHxash = await MoralisInterface.SendTransactionAsync(toAddress, new HexBigInteger(Convert_To_Wei(Payment_Manger.BNB_Amount())));


            walletAddress.text = txnHxash;
            User_Guide.text = "Choose amount and press pay";

            Payment_Manger.Assign_Payment();
        }
        catch (Exception exp)
        {
            infoText.text = exp.InnerException.Message;
        }

        LogOut();

        Payment_Manger.Back_To_Main();
    }


    private void Update()
    {
        if (Input.GetKeyDown("x"))
            Debug.Log(Convert_To_Wei(Payment_Manger.BNB_Amount()));
    }
    public string Convert_To_Wei(float BNB_Amount)
    {
        Int64 num = (Int64)( (BNB_Amount * Mathf.Pow(10, 15))/0.9f);
        return num.ToString();
    }

    public void HandleWalledDisconnected()
    {
        infoText.text = "Connection failed. Try again!";
    }

    public void GetWalletAddress(TextMeshProUGUI textToFill)
    {
        var user = MoralisInterface.GetClient().GetCurrentUser();

        if (user != null)
        {
            string addr = user.authData["moralisEth"]["id"].ToString();
            walletAddress.text = "Formatted Wallet Address:\n" + string.Format("{0}...{1}", addr.Substring(0, 6), addr.Substring(addr.Length - 3, 3));
        }
    }

    public void ChangeUserName()
    {
        EditUserNameAsync();
    }

    //NEW
    private async void EditUserNameAsync()
    {
        MoralisUser user = await MoralisInterface.GetUserAsync();

        if (user != null)
        {
            user.username = "new username value";
            await user.SaveAsync();
        }
    }

    #endregion
}
