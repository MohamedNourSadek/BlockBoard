using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using System;



public class Organizer : MonoBehaviourPunCallbacks
{
    private GetUserDataResult Data_Result;
    private int BetAmount = 0;

    private void Awake()
    {
        if (!(Manager.GameManager.GameMode == GameMode.Offline))
            Get_Player();
    }

    void Get_Player()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnRecieveError);
    }



    private void OnDataRecieved(GetUserDataResult result)
    {
        Data_Result = result;
        BetAmount = Int32.Parse(PhotonNetwork.CurrentRoom.CustomProperties[PhotonManager.PlayerBetKey].ToString());

        if (!(Manager.GameManager.CurrentGame == GameType.Domino))
        {
            Handle_Data(Data_Result);
        }
    }
    void Handle_Data(GetUserDataResult result)
    {
        int Opponent_Total_Skill = 0;
        foreach (var p in Manager.GameManager.Players)
        {
            if (GameManager.IsMasterClient)
            {
                if (!(p.Key.Identity == identity.master))
                    Opponent_Total_Skill = p.Key.Domino_Rating;
            }
            else
            {
                if ((p.Key.Identity == identity.master))
                    Opponent_Total_Skill = p.Key.Domino_Rating;
            }
        }

        int Win = (DominoController.Instance.myWinState == MyWinState.Won) ? 1 : 0;
        int Draw = (DominoController.Instance.myWinState == MyWinState.Draw) ? 1 : 0;
        int Loss = (DominoController.Instance.myWinState == MyWinState.Lost) ? 1 : 0;

        int CurrentGame = (!(Manager.GameManager.CurrentGame == GameType.Domino)) ? 1 : 0;

        Game_Stats mystats = Manager.GameManager.MyStats[0];
        int Current_Credit;

        mystats.Games_Played = Int32.Parse(result.Data[mystats.Games_Played_Key].Value) + CurrentGame;
        mystats.Games_Won = Int32.Parse(result.Data[mystats.Games_Won_Key].Value) + Win;
        mystats.Games_Lost = Int32.Parse(result.Data[mystats.Games_Lost_Key].Value) + Loss - Win;
        mystats.Games_Draw = Int32.Parse(result.Data[mystats.Games_Draw_Key].Value) + Draw;

        Current_Credit = Int32.Parse(result.Data[User_Settings.Credit_Saved].Value);

        if(Loss == 1)
        {
            Current_Credit -= BetAmount;
        }
        else if(Win == 1)
        {
            Current_Credit += BetAmount;
        }
        else
        {
            Current_Credit += BetAmount;
        }

        if (!(Manager.GameManager.CurrentGame == GameType.Domino))
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



    //End game
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        PhotonNetwork.LeaveRoom();
        Manager.GameManager.CurrentScore[0] = 200;

        DominoController.Instance.myWinState = MyWinState.Lost;
        DominoController.Instance.EndRound(Winner.Master,false,true);

        Manager.GameManager.CurrentScore[0] = 0;
        Manager.GameManager.CurrentScore[1] = 0;
        Manager.GameManager.MasterWonLastGame = true;
    }
}
