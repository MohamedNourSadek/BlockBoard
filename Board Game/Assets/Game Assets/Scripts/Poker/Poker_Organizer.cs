using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System;
using PlayFab;
using PlayFab.ClientModels;

public class Poker_Organizer : MonoBehaviourPunCallbacks
{
    [Header("References")]
    [SerializeField] public List<Play_Card> cards = new List<Play_Card>();
    [SerializeField] Poker_Player player;
    [SerializeField] public List<Play_Card> Community_cards = new List<Play_Card>();
    [SerializeField] Text_Comments_Generator text_comm;
    [SerializeField] public Button_Sound sounds;
    [SerializeField] GameObject EndGame_Menu;
    [SerializeField] Text EndGame_Text;

    [Header("Community Cards positions")]
    [SerializeField] Transform Community_Cards_Position;
    [SerializeField] float Spacing = 0.1f;
    [SerializeField] Vector3 CommunityCards_Rotation;

    [Header("Animation Time")]
    [SerializeField] public float Delay_To_SendCards = 0.15f;
    [SerializeField] public identity client;

    [Header("Game parameters")]
    [SerializeField] public float InBet = 100f;

    //Internal Variables
    public bool IsGame_Working = false;
    PhotonView view;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        UpdateMaster_Client();

        Initialize_Players();

        if (!Cross_Scene_Data.AI)
        {
            view = GetComponent<PhotonView>();

            if (PhotonNetwork.IsMasterClient && Cross_Scene_Data.UseNewMaxScore)
            {
                Change_Bet(BetAmount);
                view.RPC("Change_Bet", RpcTarget.OthersBuffered, BetAmount);
            }
        }
        else
        {
            Change_Bet(Cross_Scene_Data.BetAmount);
        }

        if (client == identity.master)
            Start_Round();
    }

    public GetUserDataResult Data_Result;
    int BetAmount = 0;
    public MyWin_State my_WinState = MyWin_State.Lost;

    void Get_Player()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnRecieveError);
    }

    private void OnDataRecieved(GetUserDataResult result)
    {
        Data_Result = result;
        BetAmount = Int32.Parse(PhotonNetwork.CurrentRoom.CustomProperties[Connect_ToServer.RoomBet_String].ToString());

        if (!Cross_Scene_Data.In_Poker_Game)
        {
            Handle_Data(Data_Result);
        }
    }
    public void Handle_Data(GetUserDataResult result)
    {
        int Opponent_Total_Skill = 0;

        foreach (var p in Cross_Scene_Data.players)
        {
            if (!(client == p.Key.Identity))
            {
                Opponent_Total_Skill += p.Key.Poker_Rating;
            }
        }

        int Win = (my_WinState == MyWin_State.Won) ? 1 : 0;
        int Draw = (my_WinState == MyWin_State.Draw) ? 1 : 0;
        int Loss = (my_WinState == MyWin_State.Lost) ? 1 : 0;
        int CurrentGame = (!Cross_Scene_Data.In_Poker_Game) ? 1 : 0;

        Game_Stats mystats = Cross_Scene_Data.mystats[2];
        int Current_Credit;

        mystats.Games_Played = Int32.Parse(result.Data[mystats.Games_Played_Key].Value) + CurrentGame;
        mystats.Games_Won = Int32.Parse(result.Data[mystats.Games_Won_Key].Value) + Win;
        mystats.Games_Lost = Int32.Parse(result.Data[mystats.Games_Lost_Key].Value) + Loss - Win - Draw;
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


        if (!Cross_Scene_Data.In_Poker_Game)
        {
            int Past_Avg = Int32.Parse(result.Data[mystats.Avg_Op_Key].Value);
            int AllGames_TotalSkill = (Past_Avg * mystats.Games_Played) + (Opponent_Total_Skill/Cross_Scene_Data.players.Count - 1);
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

        Cross_Scene_Data.In_Poker_Game = true;
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


    public void Start_Round()
    {
        if (client == identity.master)
        {
            //Generate a random Seqence
            string RandomSeq = Generate_Random(52);
            int RandomPlayer_ind = 0;
            int RandomPlayer2_ind = 0;

            while(RandomPlayer_ind == RandomPlayer2_ind)
            {
                if (Cross_Scene_Data.AI)
                { 
                    RandomPlayer_ind = UnityEngine.Random.Range(0, 4);
                    RandomPlayer2_ind = UnityEngine.Random.Range(0, 4);
                }
                else
                {
                    RandomPlayer_ind = UnityEngine.Random.Range(0, Cross_Scene_Data.players.Count);
                    RandomPlayer2_ind = UnityEngine.Random.Range(0, Cross_Scene_Data.players.Count);
                }
            }

            Shuffle_Hand(RandomSeq,RandomPlayer_ind,RandomPlayer2_ind);

            if (!Cross_Scene_Data.AI)
                view.RPC("Shuffle_Hand", RpcTarget.OthersBuffered, RandomSeq, RandomPlayer_ind, RandomPlayer2_ind);
        }


    }
    public void Reset_Round()
    {
        foreach (var p in player.Global_players)
        {
            foreach (var card in p.mycards)
                cards.Add(card);

            p.mycards.Clear();

            p.Potential_Bet = 20;
            p.Bet = 0;
        }

        foreach (var card in Community_cards)
            cards.Add(card);

        Community_cards.Clear();

        player.cycles = 0;
        player.Final_bet = 0;
        player.Current_bet = 20;
        Initialize_Players();

        for (int i = 0; i <= cards.Count - 1; i++)
        {
            cards[i].gameObject.transform.position = transform.position + (i * 0.1f * Vector3.up);
        }

        cards = player.sort_cards_Des(cards);

        player.PlayersInRound.Clear();

        if (Cross_Scene_Data.AI)
        {
            player.PlayersInRound.Add(0);
            player.PlayersInRound.Add(1);
            player.PlayersInRound.Add(2);
            player.PlayersInRound.Add(3);
        }
        else
        {
            foreach (var p in player.Global_players)
            {
                p.MyChips_Obj.SetActive(false);

                if (p.MyName_Obj)
                    p.MyName_Obj.SetActive(false);
            }

            foreach (var p in Cross_Scene_Data.players)
            {
                player.PlayersInRound.Add((int)p.Key.Identity);

                //Swithcing on only players that are in the game.
                foreach (var a_player in player.Global_players)
                    if (a_player.player_role == p.Key.Identity)
                    {
                        a_player.MyChips_Obj.SetActive(true);

                        if (a_player.MyName_Obj)
                            a_player.MyName_Obj.SetActive(true);
                    }
            }
        }
    }
    void Initialize_Players()
    {
        if (client == identity.master)
        {
            player.Global_players[0].player_role = identity.master;
            player.Global_players[1].player_role = identity.Guest;
            player.Global_players[2].player_role = identity.Guest2;
            player.Global_players[3].player_role = identity.Guest3;
        }
        else if(client == identity.Guest)
        {
            player.Global_players[0].player_role = identity.Guest;
            player.Global_players[1].player_role = identity.master;
            player.Global_players[2].player_role = identity.Guest2;
            player.Global_players[3].player_role = identity.Guest3;
        }
        else if (client == identity.Guest2)
        {
            player.Global_players[0].player_role = identity.Guest2;
            player.Global_players[1].player_role = identity.master;
            player.Global_players[2].player_role = identity.Guest;
            player.Global_players[3].player_role = identity.Guest3;
        }
        else if (client == identity.Guest3)
        {
            player.Global_players[0].player_role = identity.Guest3;
            player.Global_players[1].player_role = identity.master;
            player.Global_players[2].player_role = identity.Guest;
            player.Global_players[3].player_role = identity.Guest2;
        }
    }
    void UpdateMaster_Client()
    {
        if (Cross_Scene_Data.AI)
        {
            client = identity.master;
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
                client = identity.master;
            else
            {
                foreach (var player in Cross_Scene_Data.players)
                {
                    if(player.Value == PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        client = player.Key.Identity;
                    }
                }
            }
        }
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

    [PunRPC] void Shuffle_Hand(string RandomSquence_Strg, int P1, int P2)
    {
        IsGame_Working = false;
        Reset_Round();

        //Shuffle and replace cards
        var seq = RandomSquence_Strg.Split(',');
        List<Play_Card> New_Cards = new List<Play_Card>();

        for (int i = 0; i < seq.Length - 1; i++)
            New_Cards.Add(cards[System.Convert.ToInt32(seq[i])]);
        cards = New_Cards;

        //Send new cards to players
        StartCoroutine(Send_Cards(P1, P2));

        my_WinState = MyWin_State.Lost;

        if (!Cross_Scene_Data.AI)
            Get_Player();
    }
    [PunRPC] void Change_Bet(float bet)
    {
        InBet = bet;

        foreach (var p in player.Global_players)
            p.myChips = (int)InBet;
    }

    IEnumerator Send_Cards(int P1, int P2)
    {
        string Player1_Name = "";
        string Player2_Name = "";
        
        if ((identity)P1 == client)
            Player1_Name = "You";

        if ((identity)P2 == client)
            Player2_Name = "You";


        if (Cross_Scene_Data.AI)
        {
            if ((identity)P1 == identity.Guest)
                Player1_Name = "Player 2";
            else if ((identity)P1 == identity.Guest2)
                Player1_Name = "Player 3";
            else if ((identity)P1 == identity.Guest3)
                Player1_Name = "Player 4";

            if ((identity)P2 == identity.Guest)
                Player2_Name = "Player 2";
            else if ((identity)P2 == identity.Guest2)
                Player2_Name = "Player 3";
            else if ((identity)P2 == identity.Guest3)
                Player2_Name = "Player 4";
        }
        else
        {
            foreach (var p in Cross_Scene_Data.players)
            {
                if ((identity)P1 == p.Key.Identity)
                    Player1_Name = p.Key.PlayerName;

                if ((identity)P2 == p.Key.Identity)
                    Player2_Name = p.Key.PlayerName;
            }
        }


        text_comm.PlayText("Small Bet from " + Player1_Name);
        player.Get_Player((identity)player.PlayersInRound[P1]).Bet = 10;
        player.Get_Player((identity)player.PlayersInRound[P1]).Played = true;
        yield return new WaitForSeconds(Delay_To_SendCards * 5);

        text_comm.PlayText("Big Bet from " + Player2_Name);
        player.Get_Player((identity)player.PlayersInRound[P2]).Bet = 20;
        player.Get_Player((identity)player.PlayersInRound[P2]).Played = true;
        player.Current_bet = 20;
        yield return new WaitForSeconds(Delay_To_SendCards * 3);


        //adding two cards for each player
        if (Cross_Scene_Data.AI)
        {
            foreach (var p in player.Global_players)
            {
                for (int j = 0; j <= 1; j++)
                {
                    p.mycards.Add(cards[j]);
                    cards.Remove(cards[j]);
                    player.ReOrganize_cards(new List<bool>() { false, true, true, true }, new List<bool>() { false, false, false, false });
                    sounds.Play_OnPress();
                    yield return new WaitForSeconds(Delay_To_SendCards);
                }
            }
        }
        else
        {
            foreach (var p in player.PlayersInRound)
            {
                Poker_player_structure myPlayer = player.Get_Player((identity)p);

                for (int j = 0; j <= 1; j++)
                {
                    myPlayer.mycards.Add(cards[j]);
                    cards.Remove(cards[j]);
                    player.ReOrganize_cards(new List<bool>() { false, true, true, true }, new List<bool>() { false, false, false, false });
                    sounds.Play_OnPress();
                    yield return new WaitForSeconds(Delay_To_SendCards);
                }
            }
        }

        //Adding Three Community Cards
        for (int i = 0; i <= 2; i++)
        {
            Add_ToCommunityCards();
        }

        StartCoroutine(Reorganize(true));

    }
    public void Add_ToCommunityCards()
    {
        Community_cards.Add(cards[0]);
        cards.Remove(cards[0]);
    }
    public IEnumerator Reorganize(bool Hide)
    {
        for (int i = 0; i < Community_cards.Count; i++)
        {
            yield return new WaitForSeconds(Delay_To_SendCards);
            
            sounds.Play_OnPress();

            Community_cards[i].transform.position = Community_Cards_Position.position + (Community_Cards_Position.right * i * Spacing);

            Vector3 Rotation = new Vector3(CommunityCards_Rotation.x, CommunityCards_Rotation.y, 0f);

            if (Hide)
                Rotation.z = 180f;
            else
                Rotation.z = 0f;

            Community_cards[i].transform.eulerAngles = Rotation;

        }

        IsGame_Working = true;
        player.Update_UI();

        //Hinter
        player.Update_Hinter(player.Global_players[0].mycards);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            foreach (var p in player.Global_players)
                foreach (var a_player in Cross_Scene_Data.players)
                    if (p.player_role == a_player.Key.Identity)
                    {
                        p.myChips += player.Final_bet;
                        p.MyChipsText.text = p.myChips.ToString();
                    }

            PhotonNetwork.LeaveRoom();
            EndGame_Menu.SetActive(true);
            EndGame_Text.text = "Everyone left, You win!";
            IsGame_Working = false;

            my_WinState = MyWin_State.Won;

            if (Cross_Scene_Data.In_Poker_Game && !Cross_Scene_Data.AI)
            {
                if (!(my_WinState == MyWin_State.Lost))
                    Handle_Data(Data_Result);

                Cross_Scene_Data.In_Poker_Game = false;
            }
        }
        else
        {
            UpdateMaster_Client();
            Initialize_Players();
        }
    }
}
