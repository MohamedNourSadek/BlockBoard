using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System;

using System.Threading;
using TMPro;

public class Crypto_Payment : MonoBehaviour
{

    [SerializeField] int MinAllowed_Withdraw = 2000;

    [Header("General")]

    [SerializeField] User_Settings user_Settings;
    [SerializeField] WalletConnectSharp.Unity.WalletConnect WalletConnector;

    [SerializeField] public GameObject Payment_Main;



    [Header("Buy")]
    [SerializeField] GameObject BuyPage;
    [SerializeField] GameObject Buy_Main;
    [SerializeField] GameObject BNB_Payment_Parameters;
    [SerializeField] GameObject BNB_Payment_Successful;
    [SerializeField] GameObject BNB_Payment_Waiting;
    [SerializeField] InputField Amount_InputField;
    [SerializeField] Text Conversion_Text;

    [Header("Sell")]
    [SerializeField] GameObject SellPage;
    [SerializeField] GameObject Not_enough_Credit;
    [SerializeField] Text Not_Enough_Credit_Text;
    [SerializeField] GameObject BNB_Sell_Parameters;
    [SerializeField] InputField Address_Field;
    [SerializeField] Slider Amount_Slider;
    [SerializeField] Text AmountSlider_Text;
    [SerializeField] Text Equivilant_BNB_Field;
    [SerializeField] GameObject Request_Sucess;

    public int Current_Credit = 0;


    private void Start()
    {
        user_Settings.DataRecieved += OnRecieve;
    }

    private void OnRecieve()
    {
        Current_Credit = Int32.Parse(user_Settings.Credit_Text.text);
    }

    //Main pages
    public void Back_To_Main()
    {
        Payment_Main.SetActive(true);

        BuyPage.SetActive(false);
        SellPage.SetActive(false);
        Not_enough_Credit.SetActive(true);
        BNB_Payment_Parameters.SetActive(false);
        BNB_Payment_Waiting.SetActive(false);
        BNB_Payment_Successful.SetActive(false);
    }


    //Buy Functions
    public void Show_Buy_Page()
    {
        BuyPage.SetActive(true);

        SellPage.SetActive(false);
        Payment_Main.SetActive(false);
    }
    public void BNB_Show_Payment_Parameters()
    {
        BNB_Payment_Parameters.SetActive(true);
        
        BNB_Payment_Waiting.SetActive(false);
        BNB_Payment_Successful.SetActive(false);
        Amount_Slider_OnChange(0);
    }
    public void BNB_Show_Payment_Waiting()
    {
        Cross_Scene_Data.where = WhereTo.GameSelection;
        BNB_Payment_Waiting.SetActive(true);

        Buy_Main.SetActive(false);
        this.gameObject.SetActive(true);
        Payment_Main.SetActive(false);
        BNB_Payment_Parameters.SetActive(false);
        BNB_Payment_Successful.SetActive(false);
    }
    public void BNB_Show_PaymentSuccessful()
    {
        Cross_Scene_Data.where = WhereTo.GameSelection;

        BNB_Payment_Successful.SetActive(true);
        BNB_Payment_Waiting.SetActive(false);
        BNB_Payment_Parameters.SetActive(false);
    }
    public void Pay()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnRecieveError);
    }
    private void OnDataRecieved(GetUserDataResult result)
    {
        Current_Credit = Int32.Parse(result.Data[User_Settings.Credit_Saved].Value);

        Open_payement(true);
    }
    private void OnRecieveError(PlayFabError Error)
    {
        Pay();
    }
    async public void Open_payement(bool OpenPage)
    {
        BNB_Show_Payment_Waiting();
        WalletConnector.OpenDeepLink();
    }
    public int Get_Buy_Eqiv()
    {
        return (int)(float.Parse(Amount_InputField.text) * 10000);
    }
    public float BNB_Amount()
    {
        return (float.Parse(Amount_InputField.text));
    }
    public void Assign_Payment()
    {
        Current_Credit += Get_Buy_Eqiv();
        Set_New_Credit();
        BNB_Show_PaymentSuccessful();
    }
    public void Set_New_Credit()
    {
        Dictionary<string, string> Data_To_Send = new Dictionary<string, string>();

        Data_To_Send.Add(User_Settings.Credit_Saved, Current_Credit.ToString());

        var request = new UpdateUserDataRequest
        {
            Data = Data_To_Send,
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnSendError);
    }
    private void OnDataSend(UpdateUserDataResult result)
    {
        user_Settings.GetData();
    }
    private void OnSendError(PlayFabError Error)
    {
        Set_New_Credit();
    }
    public void Amount_Slider_OnChange(float v)
    {
        Conversion_Text.text = BNB_Amount() + " BNB = " + Get_Buy_Eqiv();
    }


    //Sell Functions

    int credit;
    const int Transmission_Multi = 10000;
    const float Sell_Equiv = 12000f;
    const float ReBuy_Punishment = 1.02f;

    public void Show_Sell_Page()
    {
        Payment_Main.SetActive(false);

        user_Settings.DataRecieved += Show_sell_PostDataRecival;

        user_Settings.GetData();
    }

    public void Show_sell_PostDataRecival()
    {
        user_Settings.DataRecieved -= Show_sell_PostDataRecival;


        SellPage.SetActive(true);

        credit = Int32.Parse(user_Settings.Credit_Text.text);

        bool Request_In_Progress = user_Settings.Requested_Amount != 0;

        if (Request_In_Progress)
        {
            Request_Sucess.SetActive(true);

            Not_enough_Credit.SetActive(false);
            BNB_Sell_Parameters.SetActive(false);
        }
        else if (credit < MinAllowed_Withdraw)
        {
            Not_Enough_Credit_Text.text = "You must at least have " + MinAllowed_Withdraw.ToString() + " credits";

            Not_enough_Credit.SetActive(true);
            Request_Sucess.SetActive(false);
            BNB_Sell_Parameters.SetActive(false);
        }
        else if (credit >= MinAllowed_Withdraw)
        {
            Amount_Slider.minValue = MinAllowed_Withdraw;
            Amount_Slider.maxValue = credit;
            Amount_Slider.value = credit;
            Address_Field.text = user_Settings.WalletAddress.text;

            BNB_Sell_Parameters.SetActive(true);
            Not_enough_Credit.SetActive(false);
            Request_Sucess.SetActive(false);
        }
    }

    public Double Get_Sell_Eqiv()
    {
        return Math.Round(((Amount_Slider.value) / (Sell_Equiv* ReBuy_Punishment)),3,MidpointRounding.ToEven);
    }
    public int Get_Sell_Eqiv_cred()
    {
        return Mathf.RoundToInt((((user_Settings.Requested_Amount*1f) / (Transmission_Multi*1f)) * Sell_Equiv));
    }
    public void OnSellAmount_Change()
    {
        AmountSlider_Text.text = Amount_Slider.value.ToString();
        Equivilant_BNB_Field.text = Get_Sell_Eqiv().ToString() + " BNB";
    }
    public void Paste_Text()
    {
        Address_Field.text = GUIUtility.systemCopyBuffer;
    }

    public void Submit_Request()
    {
        BNB_Sell_Parameters.SetActive(false);


        Current_Credit -= Int32.Parse(AmountSlider_Text.text);

        Dictionary<string, string> Data_To_Send = new Dictionary<string, string>();

        Data_To_Send.Add(User_Settings.WithDraw_Req, ((int)(Get_Sell_Eqiv() * Transmission_Multi)).ToString());
        Data_To_Send.Add(User_Settings.Credit_Saved, Current_Credit.ToString());
        Data_To_Send.Add(User_Settings.Saved_Address, Address_Field.text);


        var request = new UpdateUserDataRequest
        {
            Data = Data_To_Send,
        };

        PlayFabClientAPI.UpdateUserData(request, OnSubmit, OnSubmitError);
    }
    private void OnSubmit(UpdateUserDataResult obj)
    {
        Request_Sucess.SetActive(true);

        user_Settings.Requested_Amount = (int)(Get_Sell_Eqiv() * Transmission_Multi);
        user_Settings.Credit_Text.text = Current_Credit.ToString();

        user_Settings.GetData();
    }
    private void OnSubmitError(PlayFabError obj)
    {
        Payment_Main.SetActive(true);
    }


    public void Cancel_Request()
    {
        Request_Sucess.SetActive(false);

        Dictionary<string, string> Data_To_Send = new Dictionary<string, string>();

        Data_To_Send.Add(User_Settings.WithDraw_Req, "0");
        Data_To_Send.Add(User_Settings.Credit_Saved, (Current_Credit + Get_Sell_Eqiv_cred()).ToString());

        var request = new UpdateUserDataRequest
        {
            Data = Data_To_Send,
        };

        PlayFabClientAPI.UpdateUserData(request, OnCancel, OnCancelError);
    }

    private void OnCancel(UpdateUserDataResult obj)
    {
        Current_Credit += Get_Sell_Eqiv_cred();
        user_Settings.Credit_Text.text = Current_Credit.ToString();
        user_Settings.Requested_Amount = 0;

        user_Settings.GetData();

        Back_To_Main();
    }
    private void OnCancelError(PlayFabError obj)
    {
        Request_Sucess.SetActive(true);
    }



}
