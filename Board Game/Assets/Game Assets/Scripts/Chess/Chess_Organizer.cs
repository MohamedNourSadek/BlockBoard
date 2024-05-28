using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using PlayFab;
using PlayFab.ClientModels;

public class Chess_Organizer : MonoBehaviourPunCallbacks
{
    [SerializeField] General_InGameUI General_UI_Manager;
    [SerializeField] Chess_player myplayer;
    [SerializeField] GameObject board;
    [SerializeField] Material BlackBoardMat;

    //internal variables
    [System.NonSerialized] public bool master_client;
    PhotonView view;

    private void Awake()
    {
        Time.timeScale = 1f;

        if(!(Manager.GameManager.GameMode == GameMode.Offline))
        {
            view = GetComponent<PhotonView>();

            if (PhotonNetwork.IsMasterClient)
            {
                view.RPC("ChangeTime_var", 
                    RpcTarget.AllBuffered, 
                    Manager.GameManager.ChessSettings.ChessTime,
                    Manager.GameManager.ChessSettings.ChessBonusTime);
            }
        }
        else
        {
            ChangeTime_var(
                    Manager.GameManager.ChessSettings.ChessTime,
                    Manager.GameManager.ChessSettings.ChessBonusTime);
        }

        UpdateMaster_Client();

        if (!master_client)
        {
            board.transform.Rotate(0f, 180f, 0f);
            board.GetComponent<MeshRenderer>().material = BlackBoardMat;
        }


        if (!(Manager.GameManager.GameMode == GameMode.Offline))
            Get_Player();
    }

    void Get_Player()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnRecieveError);
    }

    GetUserDataResult Data_Result;
    int BetAmount = 0;
    MyWin_State my_WinState = MyWin_State.Lost;

    private void OnDataRecieved(GetUserDataResult result)
    {
        Data_Result = result;
        BetAmount = Int32.Parse(PhotonNetwork.CurrentRoom.CustomProperties[PhotonManager.PlayerBetKey].ToString());

        if (!(Manager.GameManager.CurrentGame == GameType.Chess))
        {
            Handle_Data(Data_Result);
        }
    }
    void Handle_Data(GetUserDataResult result)
    {
        int Opponent_Total_Skill = 0;
        foreach (var p in Manager.GameManager.Players)
        {
            if (master_client)
            {
                if (!(p.Key.Identity == identity.master))
                    Opponent_Total_Skill = p.Key.Chess_Rating;
            }
            else
            {
                if ((p.Key.Identity == identity.master))
                    Opponent_Total_Skill = p.Key.Chess_Rating;
            }
        }

        int Win = (my_WinState == MyWin_State.Won) ? 1 : 0;
        int Draw = (my_WinState == MyWin_State.Draw) ? 1 : 0;
        int Loss = (my_WinState == MyWin_State.Lost) ? 1 : 0;
        int CurrentGame = (!(Manager.GameManager.CurrentGame == GameType.Chess)) ? 1 : 0;

        Game_Stats mystats = Manager.GameManager.MyStats[1];
        int Current_Credit;

        mystats.Games_Played = Int32.Parse(result.Data[mystats.Games_Played_Key].Value) + CurrentGame;
        mystats.Games_Won = Int32.Parse(result.Data[mystats.Games_Won_Key].Value) + Win;
        mystats.Games_Lost = Int32.Parse(result.Data[mystats.Games_Lost_Key].Value) + Loss - Win;
        mystats.Games_Draw = Int32.Parse(result.Data[mystats.Games_Draw_Key].Value) + Draw;
        
        Current_Credit = Int32.Parse(result.Data[User_Settings.Credit_Saved].Value);

        if (Loss == 1)
        {
            Current_Credit -= BetAmount;
        }
        else if (Win == 1)
        {
            Current_Credit += BetAmount;
        }

        if (!(Manager.GameManager.CurrentGame == GameType.Chess))
        {
            int Past_Avg = Int32.Parse(result.Data[mystats.Avg_Op_Key].Value);
            int AllGames_TotalSkill = (Past_Avg * mystats.Games_Played) + Opponent_Total_Skill;
            mystats.Avg_Op = AllGames_TotalSkill / (mystats.Games_Played);
        }


        mystats.Game_Skill = User_Settings.Compute_Skill(mystats.Games_Won, mystats.Games_Lost, mystats.Avg_Op);

        //Update in Result
        Data_Result.Data[mystats.Games_Played_Key].Value = mystats.Games_Played.ToString();
        Data_Result.Data[mystats.Games_Won_Key].Value = mystats.Games_Won.ToString();
        Data_Result.Data[mystats.Games_Lost_Key].Value = mystats.Games_Lost.ToString();
        Data_Result.Data[mystats.Games_Draw_Key].Value = mystats.Games_Draw.ToString();
        Data_Result.Data[mystats.Avg_Op_Key].Value = mystats.Avg_Op.ToString();
        Data_Result.Data[mystats.Game_Skill_Key].Value = mystats.Game_Skill.ToString();

        //Send data assuming you lost
        Dictionary<string, string> Data_To_Send = new Dictionary<string, string>();

        Data_To_Send.Add(mystats.Game_Skill_Key, mystats.Game_Skill.ToString());
        Data_To_Send.Add(mystats.Games_Played_Key, mystats.Games_Played.ToString());
        Data_To_Send.Add(mystats.Games_Won_Key, mystats.Games_Won.ToString());
        Data_To_Send.Add(mystats.Games_Lost_Key, mystats.Games_Lost.ToString());
        Data_To_Send.Add(mystats.Games_Draw_Key, mystats.Games_Draw.ToString());
        Data_To_Send.Add(mystats.Avg_Op_Key, mystats.Avg_Op.ToString());
        Data_To_Send.Add(User_Settings.Credit_Saved, Current_Credit.ToString());

        var request = new UpdateUserDataRequest
        {
            Data = Data_To_Send
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataSent, OnSendError);
    }
    private void OnDataSent(UpdateUserDataResult result)
    {

    }
    private void OnSendError(PlayFabError error)
    {
        if (error.Error == PlayFabErrorCode.ServiceUnavailable)
        {
            Handle_Data(Data_Result);
        }
    }
    private void OnRecieveError(PlayFabError error)
    {
        Get_Player();
    }

    public void UpdateMaster_Client()
    {
        if ((Manager.GameManager.GameMode == GameMode.Offline))
        {
            master_client = true;
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
                master_client = true;
            else
                master_client = false;
        }
    }
    public void EndGame(bool Master_Is_Winner, bool Draw, string message)
    {
        if(Draw)
        {
            General_UI_Manager.EndGame_Message.text = message;
            General_UI_Manager.EndGame_Menu.SetActive(true);
            my_WinState = MyWin_State.Draw;
        }
        else if ((Master_Is_Winner && master_client) || (!Master_Is_Winner && !master_client))
        {
            General_UI_Manager.EndGame_Message.text = message + General_UI_Manager.WinText;
            General_UI_Manager.EndGame_Menu.SetActive(true);
            my_WinState = MyWin_State.Won;
        }
        else
        {
            General_UI_Manager.EndGame_Message.text = message + General_UI_Manager.LoseText;
            General_UI_Manager.EndGame_Menu.SetActive(true);
            my_WinState = MyWin_State.Lost;
        }

        myplayer.GameIsOn = false;

        if ((Manager.GameManager.CurrentGame == GameType.Chess) && !(Manager.GameManager.GameMode == GameMode.Offline))
        {
            if (!(my_WinState == MyWin_State.Lost))
                Handle_Data(Data_Result);
        }
    }

    [PunRPC] void ChangeTime_var(float time, float bonus)
    {
        myplayer.Max_Time = time*60;
        myplayer.bonus = bonus;
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        PhotonNetwork.LeaveRoom();
        UpdateMaster_Client();

        General_UI_Manager.EndGame_Message.text = General_UI_Manager.SubmissionText + General_UI_Manager.WinText;
        General_UI_Manager.EndGame_Menu.SetActive(true);
        myplayer.GameIsOn = false;

        my_WinState = MyWin_State.Won;

        if ((Manager.GameManager.CurrentGame == GameType.Chess) && !(Manager.GameManager.GameMode == GameMode.Offline))
        {
            if(!(my_WinState == MyWin_State.Lost))
                Handle_Data(Data_Result);
        }
    }
}
