using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public enum Card_Side { Up, Down}
public class Player : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject OtherPlay_Tiles;
    [SerializeField] GameObject OutCards;
    [SerializeField] Camera_Controller camera_Controller;
    [SerializeField] Ground ground;
    [SerializeField] Organizer organizer;
    [SerializeField] Button Select_Right;
    [SerializeField] Button Select_Left;
    [SerializeField] Button Play_Center;
    [SerializeField] Button Play_Left;
    [SerializeField] Button Play_Right;
    [SerializeField] GameObject Play_Center_obj;
    [SerializeField] GameObject Play_Left_obj;
    [SerializeField] GameObject Play_Right_obj;
    [SerializeField] GameObject Play_Tools;
    [SerializeField] Text TurnText;
    [SerializeField] public Text MyScore;
    [SerializeField] Slider time_slider;
    [SerializeField] public Text OtherScore;
    [SerializeField] Scrollbar scrollbar;
    [SerializeField] LayerMask tiles;
    [SerializeField] Animation BorrwoingANim;
    [SerializeField] AnimationClip borrowing_Anim;
    [SerializeField] AnimationClip borrowingOut_Anim;
    [SerializeField] List<GameObject> FakeExtraTiles = new List<GameObject>();
    [SerializeField] Slider BorrowingTimer;
    [SerializeField] GameObject MyCards_Position;
    [SerializeField] GameObject OtherCards_Position;

    [Header("Score and Turns")]
    [SerializeField] string yourTurn_text = "Your Turn";
    [SerializeField] Color yourturn_color = Color.green;
    [SerializeField] string otherTurn_text = "Other Turn";
    [SerializeField] Color otherTurn_color = Color.red;
    [SerializeField] Text_Comments_Generator text_comm;
    [SerializeField] float text_Speed;
    [SerializeField] float text_initialScale;
    [SerializeField] Color text_initial_Color;
    [SerializeField] float text_final_Scale;
    [SerializeField] float borrow_Delay = 1f;

    [Header("Hand Positioning")]
    [SerializeField] float Z_Depth;
    [SerializeField] float Guest_Z_Depth;
    [SerializeField] float Vertical;
    [SerializeField] float Selected_Vertical = 0.05f;
    [SerializeField] float Spacing;
    [SerializeField] float GuestVertical;
    [SerializeField] float Slider_Constant = 0.5f;
    [SerializeField] Vector3 Extra_cards_Posititon;

    [Header("Timer")]
    [SerializeField] float Max_Time = 30f;
    [SerializeField] float Borrow_MaxTime = 15f;

    
    //System Variables
    PhotonView view;
    [System.NonSerialized] public int Master_Selected_Card = 3;
    [System.NonSerialized] public int Guest_Selected_Card = 3;
    [System.NonSerialized] public List<Tile> Master_Cards = new List<Tile>();
    [System.NonSerialized] public List<Tile> Guest_Cards = new List<Tile>();
    [System.NonSerialized] public bool Game_Is_On = true;


    float Slider_Adding;
    bool Master_Turn = false;
    float Timer;
    bool Played_Locally;
    bool master_client;
    Button_Sound mybutton_Sound;
    
    private void Awake()
    {
        mybutton_Sound = GetComponent<Button_Sound>();
        
        UpdateMaster_Client();

        MyScore.text = master_client ? Cross_Scene_Data.Current_Master_score.ToString() : Cross_Scene_Data.Current_Guest_score.ToString();
        OtherScore.text = master_client ? Cross_Scene_Data.Current_Guest_score.ToString() : Cross_Scene_Data.Current_Master_score.ToString();

        Timer = Max_Time;
        
        if(!Cross_Scene_Data.AI)
            view = GetComponent<PhotonView>();

        if (Cross_Scene_Data.Master_Won_LastRound)
            Master_Turn = false;
        else
            Master_Turn = true;

        if (!Cross_Scene_Data.AI)
        {
            if (master_client)
                view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
        }
        else
        {
            if (master_client)
                Switch_Turn_trigger();
        }

        scrollbar.onValueChanged.AddListener(Update_Slider_Card);
        Update_Slider_Card(scrollbar.value);
    }

    //Play card functions
    public void Play_Trigger(int ground_side)
    {
        Played_Locally = true;

        bool IsMaster = false;

        if (Cross_Scene_Data.AI)
        {
            if (Master_Turn)
                IsMaster = true;
            else
                IsMaster = false;
        }
        else
        {
            IsMaster = master_client;
        }

        if (ground_side == (int)Ground_Side.Center)
        {
            if (Cross_Scene_Data.AI)
                Center(IsMaster);
            else
                view.RPC("Center", RpcTarget.AllBuffered, IsMaster);
        }
        else if (ground_side == (int)Ground_Side.Left)
        {
            if (Cross_Scene_Data.AI)
                Left(IsMaster);
            else
                view.RPC("Left", RpcTarget.AllBuffered, IsMaster);
        }
        else if (ground_side == (int)Ground_Side.Right)
        {
            if (Cross_Scene_Data.AI)
                Right(IsMaster);
            else
                view.RPC("Right", RpcTarget.AllBuffered, IsMaster);
        }

        mybutton_Sound.Play_OnPress();
    }
    [PunRPC] void Center(bool IsMaster)
    {
        Play_Card(Ground_Side.Center, Card_Side.Up, IsMaster);
    }
    [PunRPC] void Left(bool IsMaster)
    {
        int Selected_Card = (IsMaster) ? Master_Selected_Card : Guest_Selected_Card;
        List<Tile> cards = IsMaster ? Master_Cards : Guest_Cards;

        Card_Side card_side = (ground.left_Avaiable == cards[Selected_Card].Up) ? Card_Side.Up : Card_Side.Down;
        Play_Card(Ground_Side.Left, card_side, IsMaster);
    }
    [PunRPC] void Right(bool IsMaster)
    {
        int Selected_Card = (IsMaster) ? Master_Selected_Card : Guest_Selected_Card;
        List<Tile> cards = IsMaster ? Master_Cards : Guest_Cards;

        Card_Side card_side = (ground.right_Avaiable == cards[Selected_Card].Up) ? Card_Side.Up : Card_Side.Down;
        Play_Card(Ground_Side.Right, card_side, IsMaster);
    }
    void Play_Card(Ground_Side ground_side, Card_Side card_Side, bool IsMaster)
    {
        int Selected_Card = IsMaster ? Master_Selected_Card : Guest_Selected_Card;
        List<Tile> cards = IsMaster ? Master_Cards : Guest_Cards;

        if (cards.Count > 0)
        {
            ground.Set_Card(cards[Selected_Card], ground_side, card_Side);
            cards.Remove(cards[Selected_Card]);
        }
        if (Master_Cards.Count == 0)
        {
            organizer.End_Round(Winner.Master, true,false);
            
            if(master_client)
                text_comm.PlayText("Round Winner!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);

            return;
        }
        else if (Guest_Cards.Count == 0)
        {
            organizer.End_Round(Winner.Guest, true,false);

            if (!master_client)
                text_comm.PlayText("Round Winner!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);

            return;
        }

        if (master_client)
        {
            if (Cross_Scene_Data.AI)
                Switch_Turn_trigger();
            else
                view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
        }
        Referesh_Selection_UI();
        ReOrganize_Cards_InHand();
    }


    //Selected card functions
    public void Change_Selected_UI_Trigger(int i)
    {
        bool IsMaster = master_client;

        if (Cross_Scene_Data.AI)
            Change_Selected(i, IsMaster);
        else
            view.RPC("Change_Selected", RpcTarget.AllBuffered, i, IsMaster);
    }
    [PunRPC] void Change_Selected(int i, bool IsMaster)
    {
        //Change Selected card
        if (IsMaster)
        {
            if ((Master_Selected_Card >= 0) && (Master_Selected_Card <= (Master_Cards.Count - 1)))
                Master_Selected_Card += i;
        }
        else
        {
            if ((Guest_Selected_Card >= 0) && (Guest_Selected_Card <= (Guest_Cards.Count - 1)))
                Guest_Selected_Card += i;
        }

        Referesh_Selection_UI();
        ReOrganize_Cards_InHand();
    }
    void Change_Selection_Directly(int Selected, bool IsMaster)
    {
        if (Cross_Scene_Data.AI)
            Change_Selected_Directly(Selected, IsMaster);
        else
            view.RPC("Change_Selected_Directly", RpcTarget.AllBuffered, Selected, IsMaster);

        Referesh_Selection_UI();
        ReOrganize_Cards_InHand();
    }
    [PunRPC] void Change_Selected_Directly(int newSelected, bool IsMaster)
    {
        if (IsMaster)
        {
            Master_Selected_Card = newSelected;
        }
        else
        {
            Guest_Selected_Card = newSelected;
        }
    }


    //Hand Organization functions
    public void Update_Slider_Card(float f)
    {
        Slider_Adding = f;
        ReOrganize_Cards_InHand();
    }
    public void ReOrganize_Cards_InHand()
    {
        if (master_client)
        {
            Re_Organize(Master_Cards, Master_Selected_Card,0f, false, Z_Depth);
            Re_Organize(Guest_Cards, Guest_Selected_Card, GuestVertical, true, Guest_Z_Depth);
        }
        else
        {
            Re_Organize(Guest_Cards, Guest_Selected_Card, 0f, false, Z_Depth);
            Re_Organize(Master_Cards, Master_Selected_Card, GuestVertical, true, Guest_Z_Depth);
        }

        Re_OrganizeExtra();
    }
    void Re_Organize(List<Tile> card_set, int Selected_Card, float Displacement, bool Hide, float z_Depth)
    {
        //Reposition cards in Hand
        for (int i = 0; i <= card_set.Count - 1; i++)
        {
            //Fix Rotation to point to the camera
            card_set[i].transform.position = MyCards_Position.transform.position;

            card_set[i].transform.LookAt(card_set[i].transform.position + Vector3.up);
            card_set[i].transform.Rotate(new Vector3(90f, 90f, 90f));

            if (Hide)
            {
                card_set[i].transform.Rotate(180f, 0f, 0f);
            }

            float Added_Position = 0f;
            if ((i == Selected_Card) && !Hide)
                Added_Position = Selected_Vertical;

            Vector3 Up = -Vector3.right;
            Vector3 Right = Vector3.forward;

            //Position the cards
            float Slider_add = Hide ? 0f : Slider_Adding;

            card_set[i].transform.position += (Up * (Vertical + Displacement + Added_Position))
                + (Right * i * Spacing)
                - (Right * (card_set.Count / 2f) * Spacing); //Shift to center always

            //Slider deactivated.
            //if (!Hide)
                //card_set[i].transform.position += (camera_Controller.Camera_Transform.transform.right * Slider_Constant * ((Slider_add) - 0.5f)); 
        }

        Refresh_PlayableSides(master_client);
    }

    void Re_OrganizeExtra()
    {
        //Reposition cards in Hand
        for (int i = 0; i <= organizer.Cards.Count - 1; i++)
        {
            //Fix Rotation to point to the camera
            organizer.Cards[i].transform.rotation = Quaternion.Euler(180f,90f,0f);
            organizer.Cards[i].transform.position = OutCards.transform.position + (Vector3.right * i * Spacing);
        }

        Refresh_PlayableSides(master_client);
    }
    void Refresh_PlayableSides(bool IsMaster)
    {
        if (ground.Center_Card)
        {
            Play_Center.interactable = false;
            Play_Center_obj.SetActive(false);

            int Selected_Card = IsMaster ? Master_Selected_Card : Guest_Selected_Card;
            List<Tile> cards = IsMaster ? Master_Cards : Guest_Cards;

            bool leftAv = (ground.left_Avaiable == cards[Selected_Card].Up) || (ground.left_Avaiable == cards[Selected_Card].Down);
            bool RightAv = (ground.right_Avaiable == cards[Selected_Card].Up) || (ground.right_Avaiable == cards[Selected_Card].Down);
            
            Play_Left_obj.SetActive(leftAv);
            Play_Left_obj.transform.position = ground.GetNextPosition(Ground_Side.Left);
            Play_Right_obj.SetActive(RightAv);
            Play_Right_obj.transform.position = ground.GetNextPosition(Ground_Side.Right);

            Play_Right.interactable = RightAv;
            Play_Left.interactable = leftAv;
        }

        if (Master_Turn)
        {
            Play_Center.gameObject.SetActive(master_client && !Played_Locally &&Game_Is_On);
            Play_Left.gameObject.SetActive(master_client && !Played_Locally &&Game_Is_On);
            Play_Right.gameObject.SetActive(master_client && !Played_Locally && Game_Is_On);

            Play_Tools.SetActive(master_client && !Played_Locally && Game_Is_On);
        }
        else
        {
            Play_Center.gameObject.SetActive(!master_client && !Played_Locally && Game_Is_On);
            Play_Left.gameObject.SetActive(!master_client && !Played_Locally && Game_Is_On);
            Play_Right.gameObject.SetActive(!master_client && !Played_Locally && Game_Is_On);

            Play_Tools.SetActive(!master_client && !Played_Locally && Game_Is_On);
        }
    }
    void Referesh_Selection_UI()
    {
        int Selected_Card = 0;
        List<Tile> cards = new List<Tile>();

        Fix_Selected();

        if (master_client)
        {
            Selected_Card = Master_Selected_Card;
            cards = Master_Cards;
        }
        else if (!master_client)
        {
            Selected_Card = Guest_Selected_Card;
            cards = Guest_Cards;
        }


        if (Selected_Card == 0)
        {
            Select_Left.interactable = false;
            Select_Right.interactable = true;
        }
        else if (Selected_Card == (cards.Count - 1))
        {
            Select_Left.interactable = true;
            Select_Right.interactable = false;
        }
        else if (cards.Count == 1)
        {
            Select_Left.interactable = false;
            Select_Right.interactable = false;
        }
        else
        {
            Select_Left.interactable = true;
            Select_Right.interactable = true;
        }
    }


    //Helping functions
    bool Stuck(bool Master)
    {
        bool Stuck = true;

        if (Master)
        {
            if (Master_Cards.Count > 0)
            {
                foreach (Tile card in Master_Cards)
                {
                    bool RightSide = (ground.right_Avaiable == card.Up) || (ground.right_Avaiable == card.Down);
                    bool LeftSide = (ground.left_Avaiable == card.Up) || (ground.left_Avaiable == card.Down);

                    if(RightSide || LeftSide)
                        Stuck = false;
                }
            }
            else
            {
                Stuck = false;
            }
        }
        else
        {
            if (Guest_Cards.Count > 0)
            {
                foreach (Tile card in Guest_Cards)
                {
                    bool RightSide = (ground.right_Avaiable == card.Up) || (ground.right_Avaiable == card.Down);
                    bool LeftSide = (ground.left_Avaiable == card.Up) || (ground.left_Avaiable == card.Down);

                    if (RightSide || LeftSide)
                        Stuck = false;
                }
            }
            else
            {
                Stuck = false;
            }
        }

        return Stuck;
    }
    void Fix_Selected()
    {
        if (Master_Selected_Card < 0)
            Master_Selected_Card = 0;
        else if (Master_Selected_Card > (Master_Cards.Count - 1))
            Master_Selected_Card = Master_Cards.Count - 1;

        if (Guest_Selected_Card < 0)
            Guest_Selected_Card = 0;
        else if (Guest_Selected_Card > (Guest_Cards.Count - 1))
            Guest_Selected_Card = Guest_Cards.Count - 1;
    }


    [PunRPC] void Switch_Turn_trigger()
    {
        StartCoroutine(Switch_Turn());
    }
    IEnumerator Switch_Turn()
    {
        Played_Locally = false;
        Master_Turn = !Master_Turn;
        Timer = Max_Time;
        is_4_yet = false;
        is_3_yet = false;
        is_2_yet = false;
        is_1_yet = false;
        time_slider.value = 1f;

        bool yourTurn = Master_Turn && master_client || !Master_Turn && !master_client;

        if (yourTurn && !Stuck(true) && !Stuck(false))
            text_comm.PlayText("Your Turn!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);


        if ((Stuck(true) && Stuck(false)) && organizer.Cards.Count == 0)
        {
            organizer.End_Round(Winner.Draw, true,false);
        }
        else if (Master_Turn && Stuck(true) && organizer.Cards.Count == 0)
        {
            if (Cross_Scene_Data.AI)
            {
                Switch_Turn_trigger();

            }
            else
            {
                if (master_client)
                    view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
            }

            text_comm.PlayText("Pass, No Cards!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);

        }
        else if (!Master_Turn && Stuck(false) && organizer.Cards.Count == 0)
        {
            if (Cross_Scene_Data.AI)
            {
                Switch_Turn_trigger();
            }
            else
            {
                if (master_client)
                    view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
            }

            text_comm.PlayText("Pass, No Cards!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);
        }
        else
        {
            if (Master_Turn)
            {
                bool Master_Client = Cross_Scene_Data.AI ? true : master_client;

                if (Master_Client && Stuck(true) && ground.Center_Card)
                {

                    if (organizer.Cards.Count > 0)
                    {
                        BorrwoingANim.Play(borrowing_Anim.name);
                        BorrowingTimer.gameObject.SetActive(true);
                    }
                    
                    float BorrowMaxtime = Borrow_MaxTime;
                    BorrowingTimer.value = BorrowMaxtime;

                    while (Stuck(true) && organizer.Cards.Count > 0)
                    {
                        yield return null;
                        
                        Borrowing = true;

                        while (Borrowing && BorrowMaxtime >= 0f)
                        {
                            if(!Cross_Scene_Data.AI || Cross_Scene_Data.AI && Game_Is_On)
                                BorrowMaxtime -= (Time.fixedDeltaTime*Time.timeScale);

                            BorrowingTimer.value = BorrowMaxtime;
                            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime); 
                        }

                        if(BorrowMaxtime < 0f)
                        {
                            if(Cross_Scene_Data.AI)
                            {
                                yield return new WaitForSecondsRealtime(borrow_Delay);
                                Remove_AnExtraTile(0);
                            }
                            else
                            {
                                yield return new WaitForSecondsRealtime(borrow_Delay);
                                view.RPC("Remove_AnExtraTile", RpcTarget.All, 0);
                            }
                        }

                        if (Cross_Scene_Data.AI)
                        {
                            Borrow(true);
                        }
                        else
                        {
                            Borrow(true);
                            view.RPC("Borrow", RpcTarget.OthersBuffered, true);
                        }

                        ReOrganize_Cards_InHand();
                    }
                    
                    BorrwoingANim.Play(borrowingOut_Anim.name);
                    BorrowingTimer.gameObject.SetActive(false);

                    if (Stuck(true) && organizer.Cards.Count == 0)
                    {
                        if (!Cross_Scene_Data.AI)
                            view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
                        else
                            Switch_Turn_trigger();
                    }
                }

                if (master_client)
                {
                    TurnText.text = yourTurn_text;
                    TurnText.color = yourturn_color;
                }
                else
                {
                    TurnText.text = otherTurn_text;
                    TurnText.color = otherTurn_color;
                }
            }
            else
            {
                bool Master_Client = Cross_Scene_Data.AI ? false : master_client;

                if (!Master_Client && Stuck(false) && ground.Center_Card)
                {
                    if (organizer.Cards.Count > 0 && !Cross_Scene_Data.AI)
                    {
                        BorrwoingANim.Play(borrowing_Anim.name);
                        BorrowingTimer.gameObject.SetActive(true);
                    }

                    float BorrowMaxtime = Borrow_MaxTime;
                    BorrowingTimer.value = BorrowMaxtime;

                    while (Stuck(false) && organizer.Cards.Count > 0)
                    {
                        Borrowing = true;

                        if (Cross_Scene_Data.AI)
                            yield return new WaitForSecondsRealtime(borrow_Delay);
                        else
                            yield return null;

                        if (!Cross_Scene_Data.AI)
                        {
                            while (Borrowing && BorrowMaxtime >= 0f)
                            {
                                if (!Cross_Scene_Data.AI || Cross_Scene_Data.AI && Game_Is_On)
                                    BorrowMaxtime -= (Time.fixedDeltaTime*Time.timeScale);

                                BorrowingTimer.value = BorrowMaxtime;
                                yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
                            }

                            if (BorrowMaxtime < 0f)
                            {
                                yield return new WaitForSecondsRealtime(borrow_Delay);
                                view.RPC("Remove_AnExtraTile", RpcTarget.All, 0);
                            }
                        }
                        else
                        {
                            Remove_AnExtraTile(0);
                        }

                        if (Cross_Scene_Data.AI)
                        {
                            Borrow(false);
                        }
                        else
                        {
                            Borrow(false);
                            view.RPC("Borrow", RpcTarget.OthersBuffered, false);
                        }

                        ReOrganize_Cards_InHand();
                    }

                    if (!Cross_Scene_Data.AI)
                    {
                        BorrwoingANim.Play(borrowingOut_Anim.name);
                        BorrowingTimer.gameObject.SetActive(false);
                    }

                    if (Stuck(false) && organizer.Cards.Count == 0)
                    {
                        if (!Cross_Scene_Data.AI)
                            view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
                        else
                            Switch_Turn_trigger();
                    }
                }

                if (!master_client)
                {
                    TurnText.text = yourTurn_text;
                    TurnText.color = yourturn_color;
                }
                else
                {
                    TurnText.text = otherTurn_text;
                    TurnText.color = otherTurn_color;
                }
            }
        }

        if (!yourTurn && Cross_Scene_Data.AI)
            StartCoroutine(Ai_Play());

        ReOrganize_Cards_InHand();
    }
    bool Borrowing = false;
    [PunRPC] void Borrow(bool Master)
    {
        if(Master)
        {
            Master_Cards.Add(organizer.Cards[0]);
        }
        else
        {
            Guest_Cards.Add(organizer.Cards[0]);
        }

        bool yourTurn = Master_Turn && master_client || !Master_Turn && !master_client;

        if (yourTurn)
            text_comm.PlayText("Borrowing!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);
        
        organizer.Cards.Remove(organizer.Cards[0]);
        ReOrganize_Cards_InHand();
        Referesh_Selection_UI();
    }
    public int Calculate_Score(List<Tile> cards)
    {
        int Score = 0;

        foreach (Tile t in cards)
        {
            if (t.Up == tile_name.blank)
                Score += 0;
            else if (t.Up == tile_name.one)
                Score += 1;
            else if (t.Up == tile_name.two)
                Score += 2;
            else if (t.Up == tile_name.three)
                Score += 3;
            else if (t.Up == tile_name.four)
                Score += 4;
            else if (t.Up == tile_name.five)
                Score += 5;
            else if (t.Up == tile_name.six)
                Score += 6;

            if (t.Down == tile_name.blank)
                Score += 0;
            else if (t.Down == tile_name.one)
                Score += 1;
            else if (t.Down == tile_name.two)
                Score += 2;
            else if (t.Down == tile_name.three)
                Score += 3;
            else if (t.Down == tile_name.four)
                Score += 4;
            else if (t.Down == tile_name.five)
                Score += 5;
            else if (t.Down == tile_name.six)
                Score += 6;
        }

        return Score;
    }

    void UpdateMaster_Client()
    {
        if (Cross_Scene_Data.AI)
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

    bool is_4_yet;
    bool is_3_yet;
    bool is_2_yet;
    bool is_1_yet;

    private void Update()
    {
        if (Input.touchCount > 0 || Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray;

            if (Input.touchCount > 0)
            {
                ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            }
            else
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            }

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit,50,tiles))
            {
                if (hit.collider != null)
                {
                    if (hit.collider.gameObject == Play_Center_obj)
                        Play_Trigger((int)Ground_Side.Center);
                    else if (hit.collider.gameObject == Play_Right_obj)
                        Play_Trigger((int)Ground_Side.Right);
                    else if (hit.collider.gameObject == Play_Left_obj)
                        Play_Trigger((int)Ground_Side.Left);

                    if (hit.collider.tag == "Tile")
                    {
                        Tile t = hit.collider.GetComponent<Tile>();

                        if(master_client)
                        {
                            if(Master_Cards.Contains(t))
                            {
                                int Selected = Master_Cards.IndexOf(t);
                                Change_Selection_Directly(Selected, true);
                            }
                        }
                        else
                        {
                            if (Guest_Cards.Contains(t))
                            {
                                int Selected = Guest_Cards.IndexOf(t);
                                Change_Selection_Directly(Selected, false);
                            }
                        }
                    }

                    if(hit.collider.tag == "BorrowTiles")
                    {
                        GameObject t = hit.collider.gameObject;
                        int i = FakeExtraTiles.IndexOf(t);

                        if (Borrowing)
                        {
                            if (Cross_Scene_Data.AI)
                                Remove_AnExtraTile(i);
                            else
                                view.RPC("Remove_AnExtraTile", RpcTarget.All, i);
                        }
                    }

                }
            }
        }

        if (Input.GetKeyDown("x"))
            ReOrganize_Cards_InHand();
    }

    [PunRPC] void Remove_AnExtraTile(int i)
    {
            GameObject extraTile = FakeExtraTiles[i];
            FakeExtraTiles.Remove(FakeExtraTiles[i]);
            DestroyImmediate(extraTile.gameObject);
            Borrowing = false;
    }

    private void FixedUpdate()
    {
        if (Timer <= 0)
        {
            bool yourTurn = Master_Turn && master_client || !Master_Turn && !master_client;

            if (yourTurn)
                text_comm.PlayText("Switching Round!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);

            Timer = Max_Time;

            if (master_client)
            {
                if (Cross_Scene_Data.AI)
                    Switch_Turn_trigger();
                else
                    view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
            }
        }
        else
        {
            if (Game_Is_On)
            {
                Timer -= Time.fixedDeltaTime;
                time_slider.value = (Timer / Max_Time);

                bool yourTurn = Master_Turn && master_client || !Master_Turn && !master_client;

                if (yourTurn)
                {
                    if (Timer <= 4 && !is_4_yet)
                    {
                        is_4_yet = true;
                        text_comm.PlayText("Time!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);
                    }
                    else if (Timer <= 3 && !is_3_yet)
                    {
                        is_3_yet = true;
                        text_comm.PlayText("3", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);
                    }
                    else if (Timer <= 2 && !is_2_yet)
                    {
                        is_2_yet = true;
                        text_comm.PlayText("2", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);
                    }
                    else if (Timer <= 1 && !is_1_yet)
                    {
                        is_1_yet = true;
                        text_comm.PlayText("1", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);
                    }
                }
            }
        }
    }

    IEnumerator Ai_Play()
    {
        yield return new WaitForSecondsRealtime(Random.Range(2f,3f));

        //Play Code
        if (!Master_Turn)
        {
            if (!ground.Center_Card)
            {
                Play_Trigger((int)Ground_Side.Center);
            }
            else
            {
                for (int i = 0; i <= Guest_Cards.Count - 1; i++)
                {
                    Guest_Selected_Card = i;

                    int Selected_Card = Guest_Selected_Card;
                    List<Tile> cards = Guest_Cards;

                    bool leftAv = (ground.left_Avaiable == cards[Selected_Card].Up) || (ground.left_Avaiable == cards[Selected_Card].Down);
                    bool RightAv = (ground.right_Avaiable == cards[Selected_Card].Up) || (ground.right_Avaiable == cards[Selected_Card].Down);

                    if (leftAv)
                    {
                        Play_Trigger((int)Ground_Side.Left);
                        break;
                    }
                    else if (RightAv)
                    {
                        Play_Trigger((int)Ground_Side.Right);
                        break;
                    }
                }
            }
        }
    }
}
