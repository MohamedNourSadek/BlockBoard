using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using System;

public enum Winner {Master, Guest, Draw, NoWinner}
public enum MyWin_State { Won, Draw, Lost }

public class Organizer : MonoBehaviourPunCallbacks
{
    [Header("Parameters")]
    [SerializeField] int Player_Cards_Numbers = 7;

    [Header("References")]
    [SerializeField] General_InGameUI General_UI_Manager;
    [SerializeField] public List<Tile> Cards = new List<Tile>();
    [SerializeField] Player player1;
    [SerializeField] GameObject OutObjects;
    [SerializeField] int MaxScore = 100;

    PhotonView view;
    bool master_client;

    //Game Start
    void Awake()
    {
        Time.timeScale = 1f;

        if (Manager.GameManager.GameMode == GameMode.Offline)
        {
            ChangeMaxScore((int)Cross_Scene_Data.AI_MaxScore);
        }
        else
        {
            view = GetComponent<PhotonView>();

            if (PhotonNetwork.IsMasterClient && Cross_Scene_Data.UseNewMaxScore)
            {
                view.RPC("ChangeMaxScore", RpcTarget.AllBuffered, (int)Cross_Scene_Data.AI_MaxScore);
            }
        }

        UpdateMaster_Client();

        if (master_client)
        {
            //Generate a random Seqence
            string RandomSeq = Generate_Random(Cards.Count);
            Shuffle_Hand(RandomSeq);
            
            if(!(Manager.GameManager.GameMode == GameMode.Offline))
                view.RPC("Shuffle_Hand", RpcTarget.OthersBuffered, RandomSeq);
        }

        if(!(Manager.GameManager.GameMode == GameMode.Offline))
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
        BetAmount = Int32.Parse(PhotonNetwork.CurrentRoom.CustomProperties[Connect_ToServer.RoomBet_String].ToString());

        if (!Cross_Scene_Data.In_Domino_Game)
        {
            Handle_Data(Data_Result);
        }
    }
    void Handle_Data(GetUserDataResult result)
    {
        int Opponent_Total_Skill = 0;
        foreach (var p in Cross_Scene_Data.players)
        {
            if (master_client)
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

        int Win = (my_WinState == MyWin_State.Won) ? 1 : 0;
        int Draw = (my_WinState == MyWin_State.Draw) ? 1 : 0;
        int Loss = (my_WinState == MyWin_State.Lost) ? 1 : 0;

        int CurrentGame = (!Cross_Scene_Data.In_Domino_Game) ? 1 : 0;

        Game_Stats mystats = Cross_Scene_Data.mystats[0];
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

        if (!Cross_Scene_Data.In_Domino_Game)
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

        Cross_Scene_Data.In_Domino_Game = true;
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


    [PunRPC] void ChangeMaxScore(int i)
    {
        MaxScore = i;
    }
    [PunRPC] void Shuffle_Hand(string RandomSquence_Strg)
    {
        //Shuffle and replace cards
        var seq = RandomSquence_Strg.Split(',');
        List<Tile> New_Cards = new List<Tile>();
        for (int i = 0; i < seq.Length - 1; i++)
            New_Cards.Add(Cards[System.Convert.ToInt32(seq[i])]);
        Cards = New_Cards;
        
        //Send new cards to players
        Send_Cards();
    }
    void Send_Cards()
    {
        for (int i = 0; i < Player_Cards_Numbers; i++)
        {
            player1.Master_Cards.Add(Cards[0]);
            Cards.Remove(Cards[0]);
        }
        for (int i = 0; i < Player_Cards_Numbers; i++)
        {
            player1.Guest_Cards.Add(Cards[0]);
            Cards.Remove(Cards[0]);
        }

        int increment = 0;
        foreach (Tile t in Cards)
        {
            t.transform.position = OutObjects.transform.position + new Vector3(0.05f * increment, 0f, 0f);
            increment++;
        }

        player1.ReOrganize_Cards_InHand();
    }
    string Generate_Random(int Length)
    {
        List<int> RandomSquence = new List<int>();
        string RandomSquence_Strg = "";
        for (int i = 0; i < Length; i++)
        {
            bool exists = true;
            int num = 0;

            while (exists)
            {
                num = UnityEngine.Random.Range(0, Length);
                exists = RandomSquence.Exists(e => e.Equals(num));
            }

            RandomSquence.Add(num);
        }

        foreach (int i in RandomSquence)
            RandomSquence_Strg += i.ToString() + ",";

        return RandomSquence_Strg;
    }

    //End game
    public void End_Round(Winner Round_Winner, bool rematch, bool BySubmission)
    {
        player1.Game_Is_On = false;

        if (Round_Winner == Winner.Master)
        {
            Cross_Scene_Data.Current_Master_score += player1.Calculate_Score(player1.Guest_Cards);
            Cross_Scene_Data.Master_Won_LastRound = true;
        }
        else if (Round_Winner == Winner.Guest)
        {
            Cross_Scene_Data.Current_Guest_score += player1.Calculate_Score(player1.Master_Cards);
            Cross_Scene_Data.Master_Won_LastRound = false;
        }

        Winner MatchWinner = Winner.Draw;

        if (Cross_Scene_Data.Current_Master_score >= MaxScore)
            MatchWinner = Winner.Master;
        else if (Cross_Scene_Data.Current_Guest_score >= MaxScore)
            MatchWinner = Winner.Guest;
        else
            MatchWinner = Winner.NoWinner;

        player1.MyScore.text = master_client ? Cross_Scene_Data.Current_Master_score.ToString() : Cross_Scene_Data.Current_Guest_score.ToString();
        player1.OtherScore.text = master_client ? Cross_Scene_Data.Current_Guest_score.ToString() : Cross_Scene_Data.Current_Master_score.ToString();

        string Message = "";

        if (BySubmission)
        {
            Message = General_UI_Manager.SubmissionText;
        }


        if ((MatchWinner == Winner.Master && master_client) || (MatchWinner == Winner.Guest && !master_client))
        {
            Message += General_UI_Manager.WinText;
            my_WinState = MyWin_State.Won;
        }
        else if ((MatchWinner == Winner.Guest && master_client) || (MatchWinner == Winner.Master && !master_client))
        {
            Message += General_UI_Manager.LoseText;
            my_WinState = MyWin_State.Lost;
        }
        else if (MatchWinner == Winner.NoWinner)
        {
            if ((Round_Winner == Winner.Master && master_client) || (Round_Winner == Winner.Guest && !master_client))
            {
                Message = "You've won this round!";

            }
            else if ((Round_Winner == Winner.Guest && master_client) || (Round_Winner == Winner.Master && !master_client))
            {
                Message = "You've Lost this round!";
            }
            else if (Round_Winner == Winner.Draw)
            {
                Message = "Draw!";
            }

            General_UI_Manager.EndRound_Message.text = Message;
            General_UI_Manager.EndRound_Menu.SetActive(true);
            player1.Game_Is_On = false;
            StartCoroutine(NextRound());

            return;
        }

        if (!(Manager.GameManager.GameMode == GameMode.Offline) && Cross_Scene_Data.In_Domino_Game )
        {
            if(!(my_WinState == MyWin_State.Lost))
                Handle_Data(Data_Result);

            Cross_Scene_Data.In_Domino_Game = false;
        }

        General_UI_Manager.EndGame_Message.text = Message;
        General_UI_Manager.EndGame_Menu.SetActive(true);

        General_UI_Manager.Game_UI.SetActive(false);
        Cross_Scene_Data.Current_Master_score = 0;
        Cross_Scene_Data.Current_Guest_score = 0;
        Cross_Scene_Data.Master_Won_LastRound = true;
    }
    void UpdateMaster_Client()
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
    IEnumerator NextRound()
    {
        yield return new WaitForSecondsRealtime(2f);

        if (master_client)
            General_UI_Manager.Rematch();
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        PhotonNetwork.LeaveRoom();
        Cross_Scene_Data.Current_Master_score = 200;
        UpdateMaster_Client();

        my_WinState = MyWin_State.Lost;
        End_Round(Winner.Master,false,true);

        Cross_Scene_Data.Current_Guest_score = 0;
        Cross_Scene_Data.Current_Master_score = 0;
        Cross_Scene_Data.Master_Won_LastRound = true;
    }
    private void Update()
    {
        DebugManager.Instance.Log(MaxScore);
    }
}
