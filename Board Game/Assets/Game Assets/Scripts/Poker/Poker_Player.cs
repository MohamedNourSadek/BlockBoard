using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public enum ScreenPosition { Left, Right, Down, Up}
public class Poker_Player : MonoBehaviour
{
    [Header("Hand Positioning")]
    [SerializeField] Camera_Controller camera_Controller;
    [SerializeField] float Z_Depth;
    [SerializeField] float Vertical;
    [SerializeField] float Vertical_End_Game;
    [SerializeField] Vector2 Spacing;
    [SerializeField] Vector2 Spacing_Left_Right;
    [SerializeField] float GuestVertical;
    [SerializeField] float GuestVertical_EndGame = 0.35f;
    [SerializeField] float Guest_Z_Depth = 0.7f;
    [SerializeField] Vector3 Guest2_ScreenFix;
    [SerializeField] Vector3 Guest3_ScreenFix;
    [SerializeField] float Y_EndGame = 0.1f;
    [SerializeField] float Center_Displacement;
    [SerializeField] float Center_Displacement_Left_Right = -0.07f;
    [SerializeField] float Selected_Delta = 0.02f;

    [Header("Game Parameters")]
    [SerializeField] float NextRound_Delay = 10f;
    [SerializeField] float ShowCards_Delay = 0.5f;
    [SerializeField] float Round_Timer = 10f;
    [SerializeField] float Hinter_Increments = 10;

    [Header("References")]
    [SerializeField] GameObject DealerCards;
    [SerializeField] GameObject AllUi;
    [SerializeField] GameObject Bet_Ui;
    [SerializeField] GameObject Normal_Ui;
    [SerializeField] Text Bet_Amount;
    [SerializeField] Slider BetSlider;
    [SerializeField] Poker_Organizer organizer;
    [SerializeField] Text Turn_Text;
    [SerializeField] Text_Comments_Generator text_comm;
    [SerializeField] ButtonSound sounds;
    [SerializeField] Button Raise_Button;
    [SerializeField] Button Check_Button;
    [SerializeField] GameObject Current_Bet_Obj;
    [SerializeField] Text CurrentBet_Text;
    [SerializeField] Text FinalBet_Text;
    [SerializeField] GameObject End_Round_Pannel;
    [SerializeField] Text EndGame_Text;
    [SerializeField] Text NextRoundTimer_Text;
    [SerializeField] Text Hinter_Text;
    [SerializeField] Slider Hinter_Slider;
    [SerializeField] GameObject Left_Player_Position;
    [SerializeField] GameObject Right_Player_Position;
    [SerializeField] GameObject Player_Down_P;
    [SerializeField] GameObject Player_Up_P;
    [SerializeField] GameObject Player_Right_P;
    [SerializeField] GameObject Player_Left_P;

    //Internal Variables
    public List<int> PlayersInRound = new List<int>();

    public int Final_bet = 0;
    public int Current_bet = 1;
    public identity Turn;
    public int cycles = 0;
    bool Wait = false;
    float Timer;


    public List<Poker_player_structure> Global_players = new List<Poker_player_structure>();

    PhotonView view;

    //Initial and Ui Events

    private void Awake()
    {
        Timer = Round_Timer;
    }
    private void Start()
    {

        view = GetComponent<PhotonView>();
        AllUi.SetActive(false);
        Turn_Text.text = "-----";

        BetSlider.onValueChanged.AddListener(Slider_OnChange);
        Update_Bet_Text();

    }
    public void Slider_OnChange(float value)
    {
        BetSlider.value = ((int)(value / Hinter_Increments)) * Hinter_Increments;
        Update_Bet_Text();
    }
    void Update_Bet_Text()
    {
        Bet_Amount.text = BetSlider.value.ToString();
    }
    public void Raise_Bet()
    {
        Bet_Ui.SetActive(true);
        Normal_Ui.SetActive(false);
    }
    public void Back()
    {
        Bet_Ui.SetActive(false);
        Normal_Ui.SetActive(true);
    }

    public Vector3 one;
    public Vector3 two;
    public Vector3 three;
    //Reorganize
    public void ReOrganize_cards(List<bool> hide, List<bool> ShowWholeCard)
    {
        float Host_Vertical = ShowWholeCard[0] ? Vertical_End_Game : 0f;
        float Guest1_Vertical = ShowWholeCard[1] ? GuestVertical_EndGame : GuestVertical;
        float Guest2_Vertical = ShowWholeCard[2] ? -Y_EndGame : 0f;
        float Guest3_Vertical = ShowWholeCard[3] ? Y_EndGame : 0f;

        bool p1_Turn = Turn == Global_players[0].player_role;
        bool p2_Turn = Turn == Global_players[1].player_role;
        bool p3_Turn = Turn == Global_players[2].player_role;
        bool p4_Turn = Turn == Global_players[3].player_role;



        ReOrganize_Set(Global_players[0].mycards, p1_Turn, ScreenPosition.Down, new Vector2(0, Host_Vertical), hide[0],  Vector3.zero, Z_Depth, ShowWholeCard[0]);

        ReOrganize_Set(Global_players[1].mycards, p2_Turn, ScreenPosition.Up, new Vector2(0, Guest1_Vertical), hide[1], Vector3.zero, Guest_Z_Depth, ShowWholeCard[1]);

        ReOrganize_Set(Global_players[2].mycards, p3_Turn, ScreenPosition.Right, Vector2.zero, hide[2],  Guest2_ScreenFix + new Vector3(0f, Guest2_Vertical, 0f), Guest_Z_Depth,ShowWholeCard[2]);

        ReOrganize_Set(Global_players[3].mycards, p4_Turn, ScreenPosition.Left, Vector2.zero, hide[3],  Guest3_ScreenFix + new Vector3(0f, Guest3_Vertical, 0f), Guest_Z_Depth, ShowWholeCard[3]);
    }
    
    void ReOrganize_Set(List<Play_Card> cards, bool SelectedPlayer, ScreenPosition screenPosition, Vector2 GuestDelta, bool Hide, Vector3 screenFix,float Depth, bool EndGame)
    {
        for (int i = 0; i <= cards.Count - 1; i++)
        {
            Vector2 spacing;
            float center_displacement = 0;
            Vector3 Up;
            Vector3 Forward;
            Vector3 Right;
            Vector3 InitialRotation;

            if (screenPosition == ScreenPosition.Up || screenPosition == ScreenPosition.Down)
            {
                InitialRotation = new Vector3(0f, 90f, 90f);
                Up = -Vector3.right;
                Forward = Vector3.up;
                Right = Vector3.forward;

                spacing = Spacing;
                center_displacement = Center_Displacement;
            }
            else
            {
                if (EndGame)
                {
                    InitialRotation = new Vector3(0f, 90f, 90f);
                    spacing = Spacing_Left_Right;
                    center_displacement = Center_Displacement_Left_Right;

                }
                else
                {
                    InitialRotation = new Vector3(90, 90f, 90f);
                    center_displacement = Center_Displacement;
                    spacing = Spacing;
                }

                Up =  Vector3.forward;
                Right = Vector3.right;
                Forward = Vector3.up;
            }

            float Selected_displacement = 0f;
            float Direction = 1f;

            if (screenPosition == ScreenPosition.Down || screenPosition == ScreenPosition.Right)
                Direction = 1f;
            else
                Direction = -1f;
                
            if (SelectedPlayer)
            { 
                Selected_displacement = Selected_Delta * Direction;
            }


            //Fix Rotation to point to the camera
            if (screenPosition == ScreenPosition.Up)
                cards[i].transform.position = Player_Up_P.transform.position;
            else if (screenPosition == ScreenPosition.Down)
                cards[i].transform.position = Player_Down_P.transform.position;
            else if (screenPosition == ScreenPosition.Right)
                cards[i].transform.position = Player_Right_P.transform.position;
            else if (screenPosition == ScreenPosition.Left)
                cards[i].transform.position = Player_Left_P.transform.position;

            cards[i].transform.LookAt(cards[i].transform.position + Vector3.up);
            cards[i].transform.Rotate(InitialRotation);


            if (Hide)
                cards[i].transform.Rotate(180f, 0f, 0f);

            cards[i].transform.position += 
                (Up * (Vertical + GuestDelta.y + Selected_displacement))
                + (Right * i * spacing.x)
                + (Up * i * spacing.y)
                - (Right * (cards.Count / 2f) * spacing.x)
                - (Up * (cards.Count / 2f) * spacing.y) //Shift to center always
                + (Right * center_displacement);


            //Position Modifcation with different Aspect ratios
            if(screenPosition == ScreenPosition.Right || screenPosition == ScreenPosition.Left)
            {
                Vector3 point;
                if(screenPosition == ScreenPosition.Right)
                    point = Right_Player_Position.transform.position;
                else
                    point = Left_Player_Position.transform.position;

                Vector3 diff = cards[i].transform.position - point;

                cards[i].transform.position -= (Up * (diff.z + Selected_displacement + (spacing.y*i*Direction)));
                cards[i].transform.position += (Up * screenFix.y);
            }

        }
    }
    
    public void Constrain_Play(identity player)
    {
        if (Global_players[(int)player].myChips >= Current_bet)
            Get_Player(player).Potential_Bet = Current_bet;
        else
            Get_Player(player).Potential_Bet = Global_players[(int)player].myChips;
    }
    public void Update_UI()
    {
        //Constraining UI to possible checks and bets
        if (Current_bet > Global_players[0].myChips)
        {
            Raise_Button.interactable = false;
            Check_Button.interactable = false;
        }
        else
        {
            Raise_Button.interactable = true;
            Check_Button.interactable = true;

            BetSlider.minValue = Current_bet + 1;
            BetSlider.maxValue = Global_players[0].myChips;
            Slider_OnChange(BetSlider.minValue);
        }

        CurrentBet_Text.text = Current_bet.ToString();
        FinalBet_Text.text = Final_bet.ToString();
        
        if (Turn == organizer.client && organizer.IsGame_Working)
        {
            AllUi.SetActive(true);
            Current_Bet_Obj.SetActive(true);
            Turn_Text.text = "Your Turn";

            //Reset if you were raising before.
            Normal_Ui.SetActive(true);
            Bet_Ui.SetActive(false);


        }
        else
        {
            AllUi.SetActive(false);
            Current_Bet_Obj.SetActive(false);
            Turn_Text.text = "Other";
        }

        foreach(var p in Global_players)
        {
            if (p.player_role == Turn && !(Manager.GameManager.GameMode == GameMode.Offline))
                p.MyTimer.gameObject.SetActive(true);
            else
                p.MyTimer.gameObject.SetActive(false);

            p.MyChipsText.text = p.myChips.ToString();

            if(p.player_role == identity.master)
            {
                if(p.PlayerName)
                    p.PlayerName.text = "P1";
            }
            else if (p.player_role == identity.Guest)
            {
                if (p.PlayerName)
                    p.PlayerName.text = "P2";
            }
            else if (p.player_role == identity.Guest2)
            {
                if (p.PlayerName)
                    p.PlayerName.text = "P3";
            }
            else if (p.player_role == identity.Guest3)
            {
                if (p.PlayerName)
                    p.PlayerName.text = "P4";
            }
        }
    }


    IEnumerator Switch_Turn()
    {
        Timer = Round_Timer;

        Wait = false;

        List<Poker_player_structure> PlayNext = new List<Poker_player_structure>();

        foreach(var p in PlayersInRound)
        {
            var player = Get_Player((identity)p);

            if((player.Bet < Current_bet) || !player.Played)
            {
                PlayNext.Add(player);
            }
        }

        if (PlayNext.Count > 0)
        {
            Turn = PlayNext[0].player_role;
        }
        else
        {
            Final_bet += (Current_bet*PlayersInRound.Count);
            
            if(PlayersInRound.Count > 0)
                Turn = (identity)PlayersInRound[0];

            foreach(var p in PlayersInRound)
            {
                Get_Player((identity)p).Played = false;
                Get_Player((identity)p).myChips -= Current_bet;
                Get_Player((identity)p).Bet = 0;
            }

            Current_bet = 0;

            if (cycles == 3 || PlayersInRound.Count <= 1)
            {
                organizer.IsGame_Working = false;

                Update_UI();

                StartCoroutine(WhoWon());

                Wait = true;
                
                while (Wait)
                    yield return null;

                if (organizer.client == identity.master)
                    organizer.Start_Round();
            }
            else if (cycles == 0)
            {
                cycles++;
                StartCoroutine(organizer.Reorganize(false));
            }
            else
            {
                organizer.Add_ToCommunityCards();
                StartCoroutine(organizer.Reorganize(false));
                cycles++;
            }
        }

        Constrain_Play(Turn);
        ReOrganize_cards(new List<bool>() { false, true, true, true }, new List<bool>() { false, false, false, false });
        Update_UI();



        if ((Manager.GameManager.GameMode == GameMode.Offline) && Turn != identity.master)
        {
            StartCoroutine(AI_Play());
        }
    }
     

    //Play Functions
    public void Check()
    {
        AllUi.SetActive(false);

        if ((Manager.GameManager.GameMode == GameMode.Offline))
            Check_Sync((int)Turn, Global_players[0].Potential_Bet);
        else
            view.RPC("Check_Sync", RpcTarget.AllBuffered, (int)Turn, Global_players[0].Potential_Bet);

    }
    public void Fold()
    {
        AllUi.SetActive(false);

        if ((Manager.GameManager.GameMode == GameMode.Offline))
            Fold_Sync((int)Turn);
        else
            view.RPC("Fold_Sync", RpcTarget.AllBuffered, (int)Turn);


    }
    public void Raise()
    {
        AllUi.SetActive(false);

        if ((Manager.GameManager.GameMode == GameMode.Offline))
            Raise_Sync((int)Turn,(int)BetSlider.value);
        else
            view.RPC("Raise_Sync", RpcTarget.AllBuffered, (int)Turn, (int)BetSlider.value);

    }


    [PunRPC] void Switch_Turn_Sync()
    {
        StartCoroutine(Switch_Turn());
    }

    [PunRPC] void Check_Sync(int player, int amount)
    {
        Manager.GetManager<SoundManager>().PlayButtonClick();

        Player_Dependent_Text("Check!", player);

        foreach(var p in Global_players)
            if(p.player_role == (identity)player)
            {
                p.Potential_Bet = 0;
                p.Bet = amount;
                p.Played = true;
                Current_bet = amount;
            }

        //ReAdding the player, but at the end of the list
        PlayersInRound.Remove(player);
        PlayersInRound.Add(player);

        if (organizer.client == identity.master && !(Manager.GameManager.GameMode == GameMode.Offline))
            view.RPC("Switch_Turn_Sync", RpcTarget.AllBuffered);
        else if ((Manager.GameManager.GameMode == GameMode.Offline))
            Switch_Turn_Sync();
    }
    [PunRPC] void Fold_Sync(int player)
    {
        PlayersInRound.Remove(player);

        Player_Dependent_Text("Fold!", player);

        if (organizer.client == identity.master && !(Manager.GameManager.GameMode == GameMode.Offline))
            view.RPC("Switch_Turn_Sync", RpcTarget.AllBuffered);
        else if ((Manager.GameManager.GameMode == GameMode.Offline))
            Switch_Turn_Sync();
    }

    [PunRPC] void Raise_Sync(int player, int amount)
    {
        foreach (var p in Global_players)
            if (p.player_role == (identity)player)
            {
                p.Potential_Bet = 0;
                p.Bet = amount;
                p.Played = true;
                Current_bet = amount;
            }

        Player_Dependent_Text("Raise!", player);

        //ReAdding the player, but at the end of the list
        PlayersInRound.Remove(player);
        PlayersInRound.Add(player);

        if (organizer.client == identity.master && !(Manager.GameManager.GameMode == GameMode.Offline))
            view.RPC("Switch_Turn_Sync", RpcTarget.AllBuffered);
        else if ((Manager.GameManager.GameMode == GameMode.Offline))
            Switch_Turn_Sync();
    }


    void Player_Dependent_Text(string text, int player)
    {
        if ((identity)player == Global_players[0].player_role)
        {
            text_comm.PlayText(text, new Vector2(0f, -350f));
        }
        else if ((identity)player == Global_players[1].player_role)
        {
            text_comm.PlayText(text, new Vector2(0f, 350f));
        }
        else if ((identity)player == Global_players[2].player_role)
        {
            text_comm.PlayText(text, new Vector2(650f, 0f));
        }
        else if ((identity)player == Global_players[3].player_role)
        {
            text_comm.PlayText(text, new Vector2(-650f, 0f));
        }
    }


    //AI
    IEnumerator AI_Play()
    {
        yield return new WaitForSeconds(UnityEngine.Random.RandomRange(1f, 2f));

        float i = UnityEngine.Random.RandomRange(0f, 1f);

        if (i > 0.1f)
        {
            Check_Sync((int)Turn, Get_Player(Turn).Potential_Bet);
        }
        else if (i > 0.05f)
        {
            float j = UnityEngine.Random.RandomRange(0f, 0.5f);
            float Raise_Amount = Mathf.Clamp((j * (j * Get_Player(Turn).myChips)), Current_bet, Get_Player(Turn).myChips);
            float Raise_Quantized = ((int)(Raise_Amount / Hinter_Increments)) * Hinter_Increments;
            Raise_Sync((int)Turn, (int)Raise_Amount);
        }
        else
        {
            Fold_Sync((int)Turn);
        }
    }


    /// 
    /// End Game Calculators
    ///
    IEnumerator WhoWon()
    {
        Dictionary<identity, Cards_Value> player_score = new Dictionary<identity, Cards_Value>();
        string Win_Method = "";

        //Calculating Each Player's Score
        if (PlayersInRound.Count > 1)
            foreach (var p in PlayersInRound)
            {
                List<Play_Card> cards = new List<Play_Card>();
                var myPlayer = Get_Player((identity)p);

                foreach (var card in organizer.Community_cards)
                    cards.Add(card);

                foreach (var card in myPlayer.mycards)
                    cards.Add(card);

                Cards_Value myValue = new Cards_Value();

                if (Is_Royal_Flush(cards).Is_True)
                {
                    myValue.Value = 0f;
                    myValue.Value += 10f;
                    myValue.Value += (Is_Royal_Flush(cards).Value / 100f);

                    myValue.Value_Method = "by a Royal Flush";
                }
                else if (Is_Straight_Flush(cards).Is_True)
                {
                    myValue.Value = 0f;
                    myValue.Value += 9f;
                    myValue.Value += (Is_Straight_Flush(cards).Value / 100f);
                    myValue.BackUp_Value = Is_Straight_Flush(cards).BackUp_Value;

                    myValue.Value_Method = "by a Straight Flush";
                }
                else if (Four_of_AKind(cards).Is_True)
                {
                    myValue.Value = 0f;
                    myValue.Value += 8f;
                    myValue.Value += (Four_of_AKind(cards).Value / 100f);
                    myValue.BackUp_Value = Four_of_AKind(cards).BackUp_Value;

                    myValue.Value_Method = "by a Four of a Kind";
                }
                else if (IsFullHouse(cards).Is_True)
                {
                    myValue.Value = 0f;
                    myValue.Value += 7f;
                    myValue.Value += (IsFullHouse(cards).Value / 100f);
                    myValue.BackUp_Value = IsFullHouse(cards).BackUp_Value;

                    myValue.Value_Method = "by a Full House";
                }
                else if (Is_Flush(cards).Is_True)
                {
                    myValue.Value = 0f;
                    myValue.Value += 6f;
                    myValue.Value += (Is_Flush(cards).Value / 100f);
                    myValue.BackUp_Value = Is_Flush(cards).BackUp_Value;

                    myValue.Value_Method = "by a Flush";
                }
                else if (Is_Straight(cards).Is_True)
                {
                    myValue.Value = 0f;
                    myValue.Value += 5f;
                    myValue.Value += (Is_Straight(cards).Value / 100f);
                    myValue.BackUp_Value = Is_Straight(cards).BackUp_Value;

                    myValue.Value_Method = "by a Straight";
                }
                else if (Is_Three_OfAkind(cards).Is_True)
                {
                    myValue.Value = 0f;
                    myValue.Value += 4f;
                    myValue.Value += (Is_Three_OfAkind(cards).Value / 100f);
                    myValue.BackUp_Value = Is_Three_OfAkind(cards).BackUp_Value;

                    myValue.Value_Method = "by a Three of a kind";
                }
                else if (Is_TwoPairs(cards).Is_True)
                {
                    myValue.Value = 0f;
                    myValue.Value += 3f;
                    myValue.Value += (Is_TwoPairs(cards).Value / 100f);
                    myValue.BackUp_Value = Is_TwoPairs(cards).BackUp_Value;

                    myValue.Value_Method = "by Two Pairs";
                }
                else if (Is_One_Pair(cards).Is_True)
                {
                    myValue.Value = 0f;
                    myValue.Value += 2f;
                    myValue.Value += (Is_One_Pair(cards).Value / 100f);
                    myValue.BackUp_Value = Is_One_Pair(cards).BackUp_Value;

                    myValue.Value_Method = "by One Pair";
                }
                else
                {
                    myValue.Value = 0f;
                    myValue.Value += 1f;
                    myValue.Value += (Get_HighestCard(cards) / 100f);
                    myValue.BackUp_Value = 0;

                    myValue.Value_Method = "by a High card";
                }

                player_score.Add((identity)p, myValue);
            }
        else if (PlayersInRound.Count == 1)
            Win_Method = "by Submission";
        else if (PlayersInRound.Count == 0)
            Win_Method = "Because of Submission";

        //Showing Cards
        bool Show_p1 = false;
        bool Show_p2 = false;
        bool Show_p3 = false;

        foreach(var p in PlayersInRound)
        {
            for (int i = 0; i < Global_players.Count; i++)
            {
                if (Global_players[i].player_role == (identity)p)
                {
                    if (i == 1)
                    {
                        Show_p1 = true;
                    }
                    else if (i == 2)
                    {
                        Show_p2 = true;
                    }
                    else if (i == 3)
                    {
                        Show_p3 = true;
                    }
                }
            }
        }


        yield return new WaitForSeconds(ShowCards_Delay);
        ReOrganize_cards(new List<bool>() { false, !Show_p1, true, true }, new List<bool>() { true, Show_p1, false, false });
        Manager.GetManager<SoundManager>().PlayButtonClick();

        yield return new WaitForSeconds(ShowCards_Delay);
        ReOrganize_cards(new List<bool>() { false, !Show_p1, !Show_p2, true }, new List<bool>() { true, Show_p1, Show_p2, false });
        Manager.GetManager<SoundManager>().PlayButtonClick();

        yield return new WaitForSeconds(ShowCards_Delay);
        ReOrganize_cards(new List<bool>() { false, !Show_p1, !Show_p2, !Show_p3 }, new List<bool>() { true, Show_p1, Show_p2, Show_p3 });
        Manager.GetManager<SoundManager>().PlayButtonClick();


        //Calculating the winner
        List<identity> winners = new List<identity>();

        if (PlayersInRound.Count > 1)
        {
            winners.Add((identity)PlayersInRound[0]);
            Win_Method = player_score[(identity)PlayersInRound[0]].Value_Method;

            foreach (var p in player_score)
            {
                bool Greater = false;
                bool Equal = false;

                foreach(var winner in winners)
                {
                    if (winner != p.Key)
                    {
                        if (p.Value.Value > player_score[winner].Value)
                        {
                            Greater = true;
                            Equal = false;
                        }
                        else if (p.Value.Value == player_score[winner].Value)
                        {
                            if (p.Value.BackUp_Value > player_score[winner].BackUp_Value)
                            {
                                Greater = true;
                                Equal = false;
                            }
                            else if (p.Value.BackUp_Value == player_score[winner].BackUp_Value)
                            {
                                Greater = false;
                                Equal = true;
                            }
                            else
                            {
                                Greater = false;
                                Equal = false;
                            }
                        }
                        else
                        {
                            Greater = false;
                            Equal = false;
                        }
                    }
                }

                if(Greater)
                {
                    winners.Clear();
                    winners.Add(p.Key);
                    Win_Method = p.Value.Value_Method;
                }
                else if(Equal)
                {
                    winners.Add(p.Key);
                }

                Debug.Log((identity)p.Key + " has value " + p.Value.Value + ", Backup of " + p.Value.BackUp_Value);
            }
        }
        else if(PlayersInRound.Count == 1)
        {
            winners.Add((identity)PlayersInRound[0]);
        }

        //Declaring the winner
        if(winners.Count == 1)
        {
            Get_Player(winners[0]).myChips += Final_bet;

            AllUi.SetActive(false);
            string PlayerName = "";

            if (winners[0] == organizer.client)
                PlayerName = "You have";
            else
            {
                if ((Manager.GameManager.GameMode == GameMode.Offline))
                {
                    if (winners[0] == identity.Guest)
                        PlayerName = "Player 2 has";
                    else if (winners[0] == identity.Guest2)
                        PlayerName = "Player 3 has";
                    else if (winners[0] == identity.Guest3)
                        PlayerName = "Player 4 has";
                }
                else
                {
                    foreach (var player in Manager.GameManager.players)
                        if (winners[0] == player.Key.Identity)
                            PlayerName = player.Key.PlayerName + " has";
                }
            }

            if (PlayerName == "")
                PlayerName = "AI has";

            if (winners[0] == organizer.client)
                organizer.my_WinState = MyWin_State.Won;

            EndGame_Text.text = PlayerName + " Won " + Win_Method;
        }
        else if(winners.Count > 1)
        {
            EndGame_Text.text = "Tie, Splitting the pots between equals";

            foreach (var winner in winners)
            {
                Get_Player(winner).myChips += (Final_bet / winners.Count);

                if (winner == organizer.client)
                    organizer.my_WinState = MyWin_State.Won;
            }
        }
        else if(winners.Count == 0)
        {
            EndGame_Text.text = "No Winner, all bets back";

            foreach (var player in Manager.GameManager.players)
                Get_Player(player.Key.Identity).myChips += (Final_bet / Manager.GameManager.players.Count);

            organizer.my_WinState = MyWin_State.Draw;
        }
        
        End_Round_Pannel.SetActive(true);
        float Timer = NextRound_Delay;

        if ((Manager.GameManager.CurrentGame == GameType.Poker) && !(Manager.GameManager.GameMode == GameMode.Offline))
        {
            if (!(organizer.my_WinState == MyWin_State.Lost))
                organizer.Handle_Data(organizer.Data_Result);
        }


        while (Timer>=0)
        {
            Timer -= Time.fixedDeltaTime;
            NextRoundTimer_Text.text = ((int)Timer).ToString();
            yield return new WaitForFixedUpdate();
        }

        End_Round_Pannel.SetActive(false);


        Wait = false;
    }

    public void Update_Hinter(List<Play_Card> newCards)
    {
        List<Play_Card> cards = new List<Play_Card>();
        foreach (var card in newCards)
            cards.Add(card);

        if(cycles > 0)
            foreach (var card in organizer.Community_cards)
                cards.Add(card);

        int cards_Added = 0;

        while(cards.Count < 7)
        {
            Play_Card newCard = new Play_Card();
            newCard.card_Type = (type)(50+ cards_Added);
            newCard.card_Rank = (Rank)(50+ cards_Added);
            cards_Added += 5;
            cards.Add(newCard);
        }

        Cards_Value myValue = new Cards_Value();
        
        if (Is_Royal_Flush(cards).Is_True)
        {
            myValue.Value = 0f;
            myValue.Value += 10f;
            myValue.Value += (Is_Royal_Flush(cards).Value / 100f);

            myValue.Value_Method = "Royal Flush";
        }
        else if (Is_Straight_Flush(cards).Is_True)
        {
            myValue.Value = 0f;
            myValue.Value += 9f;
            myValue.Value += (Is_Straight_Flush(cards).Value / 100f);
            myValue.BackUp_Value = Is_Straight_Flush(cards).BackUp_Value;

            myValue.Value_Method = "Straight Flush";
        }
        else if (Four_of_AKind(cards).Is_True)
        {
            myValue.Value = 0f;
            myValue.Value += 8f;
            myValue.Value += (Four_of_AKind(cards).Value / 100f);
            myValue.BackUp_Value = Four_of_AKind(cards).BackUp_Value;

            myValue.Value_Method = "Four of a Kind";
        }
        else if (IsFullHouse(cards).Is_True)
        {
            myValue.Value = 0f;
            myValue.Value += 7f;
            myValue.Value += (IsFullHouse(cards).Value / 100f);
            myValue.BackUp_Value = IsFullHouse(cards).BackUp_Value;

            myValue.Value_Method = "Full House";
        }
        else if (Is_Flush(cards).Is_True)
        {
            myValue.Value = 0f;
            myValue.Value += 6f;
            myValue.Value += (Is_Flush(cards).Value / 100f);
            myValue.BackUp_Value = Is_Flush(cards).BackUp_Value;

            myValue.Value_Method = "Flush";
        }
        else if (Is_Straight(cards).Is_True)
        {
            myValue.Value = 0f;
            myValue.Value += 5f;
            myValue.Value += (Is_Straight(cards).Value / 100f);
            myValue.BackUp_Value = Is_Straight(cards).BackUp_Value;

            myValue.Value_Method = "Straight";
        }
        else if (Is_Three_OfAkind(cards).Is_True)
        {
            myValue.Value = 0f;
            myValue.Value += 4f;
            myValue.Value += (Is_Three_OfAkind(cards).Value / 100f);
            myValue.BackUp_Value = Is_Three_OfAkind(cards).BackUp_Value;

            myValue.Value_Method = "Three of a kind";
        }
        else if (Is_TwoPairs(cards).Is_True)
        {
            myValue.Value = 0f;
            myValue.Value += 3f;
            myValue.Value += (Is_TwoPairs(cards).Value / 100f);
            myValue.BackUp_Value = Is_TwoPairs(cards).BackUp_Value;

            myValue.Value_Method = "Two Pairs";
        }
        else if (Is_One_Pair(cards).Is_True)
        {
            myValue.Value = 0f;
            myValue.Value += 2f;
            myValue.Value += (Is_One_Pair(cards).Value / 100f);
            myValue.BackUp_Value = Is_One_Pair(cards).BackUp_Value;

            myValue.Value_Method = "One Pair";
        }
        else
        {
            myValue.Value = 0f;
            myValue.Value += 1f;
            myValue.Value += (Get_HighestCard(cards) / 100f);
            myValue.BackUp_Value = 0;

            myValue.Value_Method = "High card";
        }


        Hinter_Text.text = myValue.Value_Method;
        Hinter_Slider.value = (int)myValue.Value;

    }


    //Functions
    Cards_Value Is_Royal_Flush(List<Play_Card> cards)
    {
        List<Play_Card> Ordered_Cards = sort_cards(cards);
        Cards_Value v = new Cards_Value();
        v.Is_True = (Is_Straight_Flush(cards).Is_True && (Ordered_Cards[2].card_Rank == Rank.Ten));
        v.Value = 13;
        return v;
    }
    Cards_Value Is_Straight_Flush(List<Play_Card> cards)
    {
        Cards_Value v = new Cards_Value();
        Cards_Value straight = Is_Straight(cards);

        v.Is_True = (Five_Same_Suit(cards).Is_True && straight.Is_True);
        v.Value = straight.Value;
        v.BackUp_Value = straight.BackUp_Value;

        return v;
    }
    Cards_Value Four_of_AKind(List<Play_Card> cards)
    {
        Cards_Value v = new Cards_Value();

        int Number_Of_Duplicates = 0;
        Play_Card theKind = null;

        foreach (var card in cards)
        {
            int i = 0;

            foreach (var other in cards)
            {
                if ((other != card) && (card.card_Rank == other.card_Rank))
                {
                    i++;
                }
            }

            if (i > Number_Of_Duplicates)
            {
                Number_Of_Duplicates = i;
                theKind = card;
                v.Value = (int)card.card_Rank;
            }
        }

        if (Number_Of_Duplicates >= 3)
            v.Is_True = true;
        else
            v.Is_True = false;

        if (theKind)
        {
            foreach (var card in cards)
            {
                if ((card.card_Rank != theKind.card_Rank) && ((int)card.card_Rank > v.BackUp_Value))
                {
                    v.BackUp_Value = (int)card.card_Rank;
                }
            }
        }

        return v;
    }
    Cards_Value IsFullHouse(List<Play_Card> cards)
    {
        Cards_Value v = new Cards_Value();

        if (Is_Three_OfAkind(cards).Is_True)
        {
            var ind_FullHouse = Full_House_Independant(cards);

            v.Is_True = ind_FullHouse.Is_True;
            v.Value = ind_FullHouse.Value;
            v.BackUp_Value = ind_FullHouse.BackUp_Value;
        }
        else
        {
            v.Is_True = false;
        }

        return v;
    }
    Cards_Value Is_Flush(List<Play_Card> cards)
    {
        List<Play_Card> ordered_cards = sort_cards(cards);
        Cards_Value v = new Cards_Value();
        Cards_Value same_suit = Five_Same_Suit(cards);

        v.Is_True = (same_suit.Is_True && !Is_Straight(cards).Is_True);
        v.Value = same_suit.Value;
        v.BackUp_Value = same_suit.BackUp_Value;

        return v;
    }
    Cards_Value Is_Straight(List<Play_Card> cards)
    {
        Cards_Value v = new Cards_Value();

        //Ordering Cards from lower to higher

        List<Play_Card> cards_ordered = sort_cards(cards);

        List<Play_Card> In_Seqence = new List<Play_Card>();
        List<Play_Card> Not_In_Seqence = new List<Play_Card>();


        for (int i = cards_ordered.Count - 1; i > 0; i--)
        {
            if (In_Seqence.Count >= 5)
                break;

            if ((cards_ordered[i].card_Rank - cards_ordered[i - 1].card_Rank == 1))
            {
                bool Already_In = false;

                foreach(var card in In_Seqence)
                {
                    if(card.card_Rank == cards_ordered[i].card_Rank)
                    {
                        Already_In = true;
                    }
                }

                if(!Already_In)
                    In_Seqence.Add(cards_ordered[i]);

                Already_In = false;

                foreach (var card in In_Seqence)
                {
                    if (card.card_Rank == cards_ordered[i-1].card_Rank)
                    {
                        Already_In = true;
                    }
                }

                if (!Already_In)
                    In_Seqence.Add(cards_ordered[i-1]);
            }
            else if((cards_ordered[i].card_Rank - cards_ordered[i - 1].card_Rank > 1))
            {
                In_Seqence.Clear();
            }
        }

        foreach (Play_Card card in cards_ordered)
            if(!In_Seqence.Contains(card))
            {
                Not_In_Seqence.Add(card);
            }

        if (In_Seqence.Count >= 5)
            v.Is_True = true;
        else
            v.Is_True = false;

        if(In_Seqence.Count > 0)
            v.Value = (int)sort_cards_Des(In_Seqence)[0].card_Rank;

        if (Not_In_Seqence.Count > 0)
        {
            v.BackUp_Value = (int)sort_cards_Des(Not_In_Seqence)[0].card_Rank;
        }


        return v;
    }
    Cards_Value Is_Three_OfAkind(List<Play_Card> cards)
    {
        Cards_Value v = new Cards_Value();

        v.Is_True = (Three_Dublicates(cards).Is_True) && (!Four_of_AKind(cards).Is_True);
        v.Value = (Three_Dublicates(cards).Value);
        v.BackUp_Value = (Full_House_Independant(cards).BackUp_Value);

        return v;
    }
    Cards_Value Is_TwoPairs(List<Play_Card> cards)
    {
        Cards_Value v = new Cards_Value();

        if (Is_Three_OfAkind(cards).Is_True)
            v.Is_True  = false;
        else
        {
            List<Play_Card> mycards = sort_cards_Des(cards);
            List<Play_Card> Temp_CopyList = sort_cards_Des(cards);

            List<int> remove_dublicate_index = new List<int>();

            for(int i = 0; i < mycards.Count; i++)
            {
                if(remove_dublicate_index.Count >= 2)
                {
                    break;
                }

                for (int j = 0; j < mycards.Count; j++)
                {
                    if (remove_dublicate_index.Count >= 2)
                    {
                        break;
                    }

                    if ((mycards[i] != mycards[j]) && (mycards[i].card_Rank == mycards[j].card_Rank) && mycards.Contains(mycards[i]) && mycards.Contains(mycards[j]))
                    {
                        remove_dublicate_index.Add(i);
                        remove_dublicate_index.Add(j);

                        v.Value = (int)mycards[i].card_Rank;
                    }
                }
            }

            foreach (int i in remove_dublicate_index)
            {
                mycards.Remove(Temp_CopyList[i]);
            }

            if(TwoCardsOf_The_Same_Kind_Exists(mycards))
            {
                v.Is_True = true;
                int PairIndex = FindPair_Index(mycards);
                v.BackUp_Value = (int)mycards[PairIndex].card_Rank;
            }
            else
            {
                v.Is_True  = false;
                v.BackUp_Value = (int)(sort_cards_Des(mycards)[0].card_Rank);
            }
        }

        return v;
    }
    Cards_Value Is_One_Pair(List<Play_Card> cards)
    {
        Cards_Value v = new Cards_Value();
        v.Is_True = (!Four_of_AKind(cards).Is_True && !Is_Three_OfAkind(cards).Is_True && !(Is_TwoPairs(cards).Is_True) && TwoCardsOf_The_Same_Kind_Exists(cards));
        v.Value = Is_TwoPairs(cards).Value;
        v.BackUp_Value = Is_TwoPairs(cards).BackUp_Value;

        return v;
    }
    
    
    
    //End Game Helper Functions
    int Get_HighestCard(List<Play_Card> cards)
    {
        List<Play_Card> OrderedList = sort_cards(cards);
        return (int)OrderedList[OrderedList.Count - 1].card_Rank;
    }
    Cards_Value Full_House_Independant(List<Play_Card> cards) // Function made to solve the looping problem from Three of a Kind using full house
    {
        Cards_Value v = new Cards_Value();

        List<Play_Card> mycards = new List<Play_Card>();
        foreach (var card in cards)
            mycards.Add(card);

        List<int> remove_dublicate_index = new List<int>();

        for (int i = 0; i < cards.Count; i++)
        {
            if (remove_dublicate_index.Count >= 3)
            {
                break;
            }

            for (int j = 0; j < cards.Count; j++)
            {
                if (remove_dublicate_index.Count >= 3)
                {
                    break;
                }


                for (int k = 0; k < cards.Count; k++)
                {

                    if (remove_dublicate_index.Count >= 3)
                    {
                        break;
                    }

                    bool NotMe = (cards[i] != cards[j]) && (cards[j] != cards[k]) && (cards[i] != cards[k]);
                    bool SameRank = (cards[i].card_Rank == cards[j].card_Rank) && (cards[j].card_Rank == cards[k].card_Rank) && (cards[i].card_Rank == cards[k].card_Rank);
                    bool NotRemoved = (mycards.Contains(cards[i]) && mycards.Contains(cards[j]) && mycards.Contains(cards[k]));

                    if (NotMe && SameRank && NotRemoved)
                    {
                        remove_dublicate_index.Add(i);
                        remove_dublicate_index.Add(j);
                        remove_dublicate_index.Add(k);

                        v.Value = (int)cards[i].card_Rank;
                    }
                }
            }
        }

        foreach (int i in remove_dublicate_index)
        {
            mycards.Remove(cards[i]);
        }

        if (TwoCardsOf_The_Same_Kind_Exists(mycards))
        {
            v.Is_True = true;
            int Pair_Index = FindPair_Index(mycards);

            if(Pair_Index != -1)
                v.BackUp_Value = (int)mycards[Pair_Index].card_Rank;
        }
        else
        {
            List<Play_Card> ordered_List = sort_cards_Des(mycards);
            v.BackUp_Value = (int)ordered_List[0].card_Rank;
            v.Is_True = false;
        }

        return v;
    } 
    bool TwoCardsOf_The_Same_Kind_Exists(List<Play_Card> cards)
    {
        int Number_Of_Duplicates = 0;

        foreach (var card in cards)
        {
            int i = 0;

            foreach (var other in cards)
            {
                if ((other != card) && (card.card_Rank == other.card_Rank))
                {
                    i++;
                }
            }

            if (i > Number_Of_Duplicates)
                Number_Of_Duplicates = i;
        }

        if (Number_Of_Duplicates >= 1)
            return true;
        else
            return false;
    }
    int FindPair_Index(List<Play_Card> cards)
    {
        int i = 0;
        int Value = -1;
        int Selected_Index = -1;

        foreach (var card in cards)
        {
            foreach (var other in cards)
            {
                if ((other != card) && (card.card_Rank == other.card_Rank))
                {
                    if((int)card.card_Rank > Value)
                    {
                        Value = (int)card.card_Rank;
                        Selected_Index = i;
                    }
                }
            }

            i++;
        }

        return Selected_Index;
    }
    Cards_Value Three_Dublicates(List<Play_Card> cards)
    {
        Cards_Value v = new Cards_Value();

        int Number_Of_Duplicates = 0;

        foreach (var card in cards)
        {
            int i = 0;

            foreach (var other in cards)
            {
                if ((other != card) && (card.card_Rank == other.card_Rank))
                {
                    i++;

                    v.Value = (int)card.card_Rank;
                }
            }

            if (i > Number_Of_Duplicates)
                Number_Of_Duplicates = i;
        }

        if (Number_Of_Duplicates >= 2)
            v.Is_True = true;
        else
            v.Is_True = false;

        return v;
    }
    Cards_Value Five_Same_Suit(List<Play_Card> cards)
    {
        Cards_Value v = new Cards_Value();

        List<Play_Card> clubs = new List<Play_Card>();
        List<Play_Card> diamonds = new List<Play_Card>();
        List<Play_Card> hearts = new List<Play_Card>();
        List<Play_Card> spades = new List<Play_Card>();

        foreach (var card in cards)
        {
            if (card.card_Type == type.Club)
            {
                clubs.Add(card);
            }
            else if (card.card_Type == type.Diamond)
            {
                diamonds.Add(card);
            }
            else if (card.card_Type == type.Heart)
            {
                hearts.Add(card);
            }
            else if (card.card_Type == type.Spades)
            {
                spades.Add(card);
            }
        }

        if(clubs.Count >= 5 || diamonds.Count >= 5 || hearts.Count >=5 || spades.Count >= 5)
        {
            v.Is_True = true;

            if(clubs.Count >= 5)
            {
                v.Value = (int)sort_cards_Des(clubs)[0].card_Rank;

                var NotMyType = Remove_5_Suit_OrLess(type.Club, cards);

                if (NotMyType.Count != 0)
                    v.BackUp_Value = (int)sort_cards_Des(NotMyType)[0].card_Rank;
                else
                    v.BackUp_Value = 0;
            }
            else if (diamonds.Count >= 5)
            {
                v.Value = (int)sort_cards_Des(diamonds)[0].card_Rank;

                var NotMyType = Remove_5_Suit_OrLess(type.Diamond, cards);

                if (NotMyType.Count != 0)
                    v.BackUp_Value = (int)sort_cards_Des(NotMyType)[0].card_Rank;
                else
                    v.BackUp_Value = 0;
            }
            else if (hearts.Count >= 5)
            {
                v.Value = (int)sort_cards_Des(hearts)[0].card_Rank;

                var NotMyType = Remove_5_Suit_OrLess(type.Heart, cards);

                if (NotMyType.Count != 0)
                    v.BackUp_Value = (int)sort_cards_Des(NotMyType)[0].card_Rank;
                else
                    v.BackUp_Value = 0;
            }
            else if (spades.Count >= 5)
            {
                v.Value = (int)sort_cards_Des(spades)[0].card_Rank;

                var NotMyType = Remove_5_Suit_OrLess(type.Spades, cards);

                if (NotMyType.Count != 0)
                    v.BackUp_Value = (int)sort_cards_Des(NotMyType)[0].card_Rank;
                else
                    v.BackUp_Value = 0;
            }
        }
        else
        {
            v.Is_True = false;
        }

        return v;
    }
    List<Play_Card> sort_cards(List<Play_Card> cards)
    {
        List<Play_Card> UnOrderedList = new List<Play_Card>();
        foreach (var card in cards)
            UnOrderedList.Add(card);
        
        List<Play_Card> orderedList = new List<Play_Card>();
        
        while(UnOrderedList.Count > 0)
        {
            Play_Card leastCard = UnOrderedList[0];

            foreach (var card in UnOrderedList)
            {
                if(card.card_Rank < leastCard.card_Rank)
                {
                    leastCard = card;
                }
            }

            orderedList.Add(leastCard);
            UnOrderedList.Remove(leastCard);
        }

        return orderedList;
    }
    List<Play_Card> Remove_5_Suit_OrLess(type suit, List<Play_Card> cards)
    {
        List<Play_Card> OrderedList = sort_cards_Des(cards);
        
        List<Play_Card> MyType = new List<Play_Card>();

        foreach (Play_Card card in OrderedList)
        {
            if(MyType.Count >= 5)
            {
                break;
            }

            if (card.card_Type == suit)
            {
                MyType.Add(card);
            }
        }

        foreach (var card in MyType)
            OrderedList.Remove(card);

        return OrderedList;
    }
    public List<Play_Card> sort_cards_Des(List<Play_Card> cards)
    {
        List<Play_Card> orderedList = new List<Play_Card>();
        List<Play_Card> ordered_list_ase = sort_cards(cards);

        for(int i = ordered_list_ase.Count - 1; i >= 0; i--)
        {
            orderedList.Add(ordered_list_ase[i]);
        }

        return orderedList;
    }
    public Poker_player_structure Get_Player(identity player)
    {
        Poker_player_structure p = new Poker_player_structure();
        foreach (var t in Global_players)
            if (t.player_role == player)
            {
                p = t;
            }

        return p;
    }


    public List<Play_Card> test_cards = new List<Play_Card>();

    private void Update()
    {
        Cards_Value myValue = new Cards_Value();

        if (Input.GetKeyDown("y"))
        {
            if (Is_Royal_Flush(test_cards).Is_True)
            {
                myValue.Value = 0f;
                myValue.Value += 10f;
                myValue.Value += (Is_Royal_Flush(test_cards).Value / 100f);

                Debug.Log("It's a Royal Flush With Value of " + myValue.Value + " , And Backup Of " + myValue.BackUp_Value);
            }
            else if (Is_Straight_Flush(test_cards).Is_True)
            {
                myValue.Value = 0f;
                myValue.Value += 9f;
                myValue.Value += (Is_Straight_Flush(test_cards).Value / 100f);
                myValue.BackUp_Value = Is_Straight_Flush(test_cards).BackUp_Value;

                Debug.Log("It's a Straight Flush With Value of " + myValue.Value + " , And Backup Of " + myValue.BackUp_Value);
            }
            else if (Four_of_AKind(test_cards).Is_True)
            {
                myValue.Value = 0f;
                myValue.Value += 8f;
                myValue.Value += (Four_of_AKind(test_cards).Value / 100f);
                myValue.BackUp_Value = Four_of_AKind(test_cards).BackUp_Value;

                Debug.Log("It's a Four of A kind With Value of " + myValue.Value + " , And Backup Of " + myValue.BackUp_Value);
            }
            else if (IsFullHouse(test_cards).Is_True)
            {
                myValue.Value = 0f;
                myValue.Value += 7f;
                myValue.Value += (IsFullHouse(test_cards).Value / 100f);
                myValue.BackUp_Value = IsFullHouse(test_cards).BackUp_Value;

                Debug.Log("It's a Full House With a Value of " + myValue.Value + " , And Backup Of " + myValue.BackUp_Value);
            }
            else if (Is_Flush(test_cards).Is_True)
            {
                myValue.Value = 0f;
                myValue.Value += 6f;
                myValue.Value += (Is_Flush(test_cards).Value / 100f);
                myValue.BackUp_Value = Is_Flush(test_cards).BackUp_Value;

                Debug.Log("It's a Flush With Value of " + myValue.Value + " , And Backup Of " + myValue.BackUp_Value);
            }
            else if (Is_Straight(test_cards).Is_True)
            {
                myValue.Value = 0f;
                myValue.Value += 5f;
                myValue.Value += (Is_Straight(test_cards).Value / 100f);
                myValue.BackUp_Value = Is_Straight(test_cards).BackUp_Value;

                Debug.Log("It's a Straight With Value of " + myValue.Value + " , And Backup Of " + myValue.BackUp_Value);
            }
            else if (Is_Three_OfAkind(test_cards).Is_True)
            {
                myValue.Value = 0f;
                myValue.Value += 4f;
                myValue.Value += (Is_Three_OfAkind(test_cards).Value / 100f);
                myValue.BackUp_Value = Is_Three_OfAkind(test_cards).BackUp_Value;

                Debug.Log("It's a Three of a kind with Value of " + myValue.Value + " , And Backup Of " + myValue.BackUp_Value);
            }
            else if (Is_TwoPairs(test_cards).Is_True)
            {
                myValue.Value = 0f;
                myValue.Value += 3f;
                myValue.Value += (Is_TwoPairs(test_cards).Value / 100f);
                myValue.BackUp_Value = Is_TwoPairs(test_cards).BackUp_Value;

                Debug.Log("It's a Two Pair With Value of " + myValue.Value + " , And Backup Of " + myValue.BackUp_Value);
            }
            else if (Is_One_Pair(test_cards).Is_True)
            {
                myValue.Value = 0f;
                myValue.Value += 2f;
                myValue.Value += (Is_One_Pair(test_cards).Value / 100f);
                myValue.BackUp_Value = Is_One_Pair(test_cards).BackUp_Value;

                Debug.Log("It's a One pair With Value of " + myValue.Value + " , And Backup Of " + myValue.BackUp_Value);
            }
            else
            {
                myValue.Value = 0f;
                myValue.Value += 1f;
                myValue.Value += (Get_HighestCard(test_cards) / 100f);
                myValue.BackUp_Value = 0;

                Debug.Log("It's a high card with Value of " + myValue.Value + " , And Backup Of " + myValue.BackUp_Value);
            }
        }

        if (Input.GetKeyDown("x"))
            ReOrganize_cards(new List<bool> { false, true, true, true },new List<bool> { false,false,false,false});
    }


    private void FixedUpdate()
    {
        if (Timer <= 0)
        {
            if (!(Manager.GameManager.GameMode == GameMode.Offline))
            {
                Timer = Round_Timer;

                if (organizer.client == identity.master)
                {
                    view.RPC("Fold_Sync", RpcTarget.AllBuffered, (int)Turn);
                }
            }
        }
        else
        {
            if (organizer.IsGame_Working)
            {
                Timer -= Time.fixedDeltaTime;

                foreach(Poker_player_structure p in Global_players)
                {
                    if(p.player_role == Turn)
                    {
                        p.MyTimer.value = (Timer / Round_Timer);
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class Poker_player_structure
{
    public List<Play_Card> mycards = new List<Play_Card>();
    public int myChips = 100;
    public int Potential_Bet = 1;
    public int Bet = 0;
    public identity player_role;
    public GameObject MyChips_Obj;
    public Text MyChipsText;
    public GameObject MyName_Obj;
    public Text PlayerName;
    public bool Played = false;
    public bool Connected = true;
    public Slider MyTimer;
}
public class Cards_Value
{
    public bool Is_True;
    public float Value;
    public float BackUp_Value = 0;
    public string Value_Method;
}

