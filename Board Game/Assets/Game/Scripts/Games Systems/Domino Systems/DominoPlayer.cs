using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class DominoPlayer : MonoBehaviour
{
    public static DominoPlayer Instance;

    [Header("References")]
    [SerializeField] GameObject OtherPlay_Tiles;
    [SerializeField] GameObject OutCards;
    [SerializeField] Camera_Controller camera_Controller;
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
    [System.NonSerialized] public List<DominoTile> MasterCards = new List<DominoTile>();
    [System.NonSerialized] public List<DominoTile> GuestCards = new List<DominoTile>();
    [System.NonSerialized] public bool Game_Is_On = true;


    float Slider_Adding;
    bool Master_Turn = false;
    float Timer;
    bool Played_Locally;
    bool master_client;
    ButtonSound mybutton_Sound;
    
    private void Awake()
    {
        Instance = this;

        mybutton_Sound = GetComponent<ButtonSound>();
        
        UpdateMaster_Client();

        MyScore.text = Manager.GameManager.CurrentScore[0].ToString();
        OtherScore.text = Manager.GameManager.CurrentScore[1].ToString();

        Timer = Max_Time;
        
        if(!(Manager.GameManager.GameMode == GameMode.Offline))
            view = GetComponent<PhotonView>();

        if (Manager.GameManager.MasterWonLastGame)
            Master_Turn = false;
        else
            Master_Turn = true;

        if (!(Manager.GameManager.GameMode == GameMode.Offline))
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

        if ((Manager.GameManager.GameMode == GameMode.Offline))
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

        if (ground_side == (int)GroundSide.Center)
        {
            if ((Manager.GameManager.GameMode == GameMode.Offline))
                Center(IsMaster);
            else
                view.RPC("Center", RpcTarget.AllBuffered, IsMaster);
        }
        else if (ground_side == (int)GroundSide.Left)
        {
            if ((Manager.GameManager.GameMode == GameMode.Offline))
                Left(IsMaster);
            else
                view.RPC("Left", RpcTarget.AllBuffered, IsMaster);
        }
        else if (ground_side == (int)GroundSide.Right)
        {
            if ((Manager.GameManager.GameMode == GameMode.Offline))
                Right(IsMaster);
            else
                view.RPC("Right", RpcTarget.AllBuffered, IsMaster);
        }

        Manager.GetManager<SoundManager>().PlayButtonClick();
    }
    [PunRPC] void Center(bool IsMaster)
    {
        Play_Card(GroundSide.Center, CardSide.Up, IsMaster);
    }
    [PunRPC] void Left(bool IsMaster)
    {
        int Selected_Card = (IsMaster) ? Master_Selected_Card : Guest_Selected_Card;
        List<DominoTile> cards = IsMaster ? MasterCards : GuestCards;

        CardSide card_side = (DominoGeometery.Instance.LeftAvailable == cards[Selected_Card].Up) ? CardSide.Up : CardSide.Down;
        Play_Card(GroundSide.Left, card_side, IsMaster);
    }
    [PunRPC] void Right(bool IsMaster)
    {
        int Selected_Card = (IsMaster) ? Master_Selected_Card : Guest_Selected_Card;
        List<DominoTile> cards = IsMaster ? MasterCards : GuestCards;

        CardSide card_side = (DominoGeometery.Instance.RightAvailable == cards[Selected_Card].Up) ? CardSide.Up : CardSide.Down;
        Play_Card(GroundSide.Right, card_side, IsMaster);
    }
    void Play_Card(GroundSide ground_side, CardSide card_Side, bool IsMaster)
    {
        int Selected_Card = IsMaster ? Master_Selected_Card : Guest_Selected_Card;
        List<DominoTile> cards = IsMaster ? MasterCards : GuestCards;

        if (cards.Count > 0)
        {
            DominoGeometery.Instance.PlayCardOnGround(cards[Selected_Card], ground_side, card_Side);
            cards.Remove(cards[Selected_Card]);
        }
        if (MasterCards.Count == 0)
        {
            DominoController.Instance.EndRound(Winner.Master, true,false);
            
            if(master_client)
                text_comm.PlayText("Round Winner!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);

            return;
        }
        else if (GuestCards.Count == 0)
        {
            DominoController.Instance.EndRound(Winner.Guest, true,false);

            if (!master_client)
                text_comm.PlayText("Round Winner!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);

            return;
        }

        if (master_client)
        {
            if ((Manager.GameManager.GameMode == GameMode.Offline))
                Switch_Turn_trigger();
            else
                view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
        }
        Referesh_Selection_UI();
        ReOrganizeCardsInHands();
    }


    //Selected card functions
    public void Change_Selected_UI_Trigger(int i)
    {
        bool IsMaster = master_client;

        if ((Manager.GameManager.GameMode == GameMode.Offline))
            Change_Selected(i, IsMaster);
        else
            view.RPC("Change_Selected", RpcTarget.AllBuffered, i, IsMaster);
    }
    [PunRPC] void Change_Selected(int i, bool IsMaster)
    {
        //Change Selected card
        if (IsMaster)
        {
            if ((Master_Selected_Card >= 0) && (Master_Selected_Card <= (MasterCards.Count - 1)))
                Master_Selected_Card += i;
        }
        else
        {
            if ((Guest_Selected_Card >= 0) && (Guest_Selected_Card <= (GuestCards.Count - 1)))
                Guest_Selected_Card += i;
        }

        Referesh_Selection_UI();
        ReOrganizeCardsInHands();
    }
    void Change_Selection_Directly(int Selected, bool IsMaster)
    {
        if ((Manager.GameManager.GameMode == GameMode.Offline))
            Change_Selected_Directly(Selected, IsMaster);
        else
            view.RPC("Change_Selected_Directly", RpcTarget.AllBuffered, Selected, IsMaster);

        Referesh_Selection_UI();
        ReOrganizeCardsInHands();
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
        ReOrganizeCardsInHands();
    }
    public void ReOrganizeCardsInHands()
    {
        if (master_client)
        {
            Re_Organize(MasterCards, Master_Selected_Card,0f, false, Z_Depth);
            Re_Organize(GuestCards, Guest_Selected_Card, GuestVertical, true, Guest_Z_Depth);
        }
        else
        {
            Re_Organize(GuestCards, Guest_Selected_Card, 0f, false, Z_Depth);
            Re_Organize(MasterCards, Master_Selected_Card, GuestVertical, true, Guest_Z_Depth);
        }

        Re_OrganizeExtra();
    }
    void Re_Organize(List<DominoTile> card_set, int Selected_Card, float Displacement, bool Hide, float z_Depth)
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
        for (int i = 0; i <= DominoController.Instance.GetTilesOutSide().Count - 1; i++)
        {
            //Fix Rotation to point to the camera
            DominoController.Instance.GetTilesOutSide()[i].transform.rotation = Quaternion.Euler(180f,90f,0f);
            DominoController.Instance.GetTilesOutSide()[i].transform.position = OutCards.transform.position + (Vector3.right * i * Spacing);
        }

        Refresh_PlayableSides(master_client);
    }
    void Refresh_PlayableSides(bool IsMaster)
    {
        if (DominoGeometery.Instance.CenterCard)
        {
            Play_Center.interactable = false;
            Play_Center_obj.SetActive(false);

            int Selected_Card = IsMaster ? Master_Selected_Card : Guest_Selected_Card;
            List<DominoTile> cards = IsMaster ? MasterCards : GuestCards;

            bool leftAv = (DominoGeometery.Instance.LeftAvailable == cards[Selected_Card].Up) || (DominoGeometery.Instance.LeftAvailable == cards[Selected_Card].Down);
            bool RightAv = (DominoGeometery.Instance.RightAvailable == cards[Selected_Card].Up) || (DominoGeometery.Instance.RightAvailable == cards[Selected_Card].Down);
            
            Play_Left_obj.SetActive(leftAv);
            Play_Left_obj.transform.position = DominoGeometery.Instance.GetNextTilePosition(GroundSide.Left);
            Play_Right_obj.SetActive(RightAv);
            Play_Right_obj.transform.position = DominoGeometery.Instance.GetNextTilePosition(GroundSide.Right);

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
        List<DominoTile> cards = new List<DominoTile>();

        Fix_Selected();

        if (master_client)
        {
            Selected_Card = Master_Selected_Card;
            cards = MasterCards;
        }
        else if (!master_client)
        {
            Selected_Card = Guest_Selected_Card;
            cards = GuestCards;
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
            if (MasterCards.Count > 0)
            {
                foreach (DominoTile card in MasterCards)
                {
                    bool RightSide = (DominoGeometery.Instance.RightAvailable == card.Up) || (DominoGeometery.Instance.RightAvailable == card.Down);
                    bool LeftSide = (DominoGeometery.Instance.LeftAvailable == card.Up) || (DominoGeometery.Instance.LeftAvailable == card.Down);

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
            if (GuestCards.Count > 0)
            {
                foreach (DominoTile card in GuestCards)
                {
                    bool RightSide = (DominoGeometery.Instance.RightAvailable == card.Up) || (DominoGeometery.Instance.RightAvailable == card.Down);
                    bool LeftSide = (DominoGeometery.Instance.LeftAvailable == card.Up) || (DominoGeometery.Instance.LeftAvailable == card.Down);

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
        else if (Master_Selected_Card > (MasterCards.Count - 1))
            Master_Selected_Card = MasterCards.Count - 1;

        if (Guest_Selected_Card < 0)
            Guest_Selected_Card = 0;
        else if (Guest_Selected_Card > (GuestCards.Count - 1))
            Guest_Selected_Card = GuestCards.Count - 1;
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


        if ((Stuck(true) && Stuck(false)) && DominoController.Instance.GetTilesOutSide().Count == 0)
        {
            DominoController.Instance.EndRound(Winner.Draw, true,false);
        }
        else if (Master_Turn && Stuck(true) && DominoController.Instance.GetTilesOutSide().Count == 0)
        {
            if ((Manager.GameManager.GameMode == GameMode.Offline))
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
        else if (!Master_Turn && Stuck(false) && DominoController.Instance.GetTilesOutSide().Count == 0)
        {
            if ((Manager.GameManager.GameMode == GameMode.Offline))
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
                bool Master_Client = (Manager.GameManager.GameMode == GameMode.Offline) ? true : master_client;

                if (Master_Client && Stuck(true) && DominoGeometery.Instance.CenterCard)
                {

                    if (DominoController.Instance.GetTilesOutSide().Count > 0)
                    {
                        BorrwoingANim.Play(borrowing_Anim.name);
                        BorrowingTimer.gameObject.SetActive(true);
                    }
                    
                    float BorrowMaxtime = Borrow_MaxTime;
                    BorrowingTimer.value = BorrowMaxtime;

                    while (Stuck(true) && DominoController.Instance.GetTilesOutSide().Count > 0)
                    {
                        yield return null;
                        
                        Borrowing = true;

                        while (Borrowing && BorrowMaxtime >= 0f)
                        {
                            if(!(Manager.GameManager.GameMode == GameMode.Offline) || (Manager.GameManager.GameMode == GameMode.Offline) && Game_Is_On)
                                BorrowMaxtime -= (Time.fixedDeltaTime*Time.timeScale);

                            BorrowingTimer.value = BorrowMaxtime;
                            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime); 
                        }

                        if(BorrowMaxtime < 0f)
                        {
                            if((Manager.GameManager.GameMode == GameMode.Offline))
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

                        if ((Manager.GameManager.GameMode == GameMode.Offline))
                        {
                            Borrow(true);
                        }
                        else
                        {
                            Borrow(true);
                            view.RPC("Borrow", RpcTarget.OthersBuffered, true);
                        }

                        ReOrganizeCardsInHands();
                    }
                    
                    BorrwoingANim.Play(borrowingOut_Anim.name);
                    BorrowingTimer.gameObject.SetActive(false);

                    if (Stuck(true) && DominoController.Instance.GetTilesOutSide().Count == 0)
                    {
                        if (!(Manager.GameManager.GameMode == GameMode.Offline))
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
                bool Master_Client = (Manager.GameManager.GameMode == GameMode.Offline) ? false : master_client;

                if (!Master_Client && Stuck(false) && DominoGeometery.Instance.CenterCard)
                {
                    if (DominoController.Instance.GetTilesOutSide().Count > 0 && !(Manager.GameManager.GameMode == GameMode.Offline))
                    {
                        BorrwoingANim.Play(borrowing_Anim.name);
                        BorrowingTimer.gameObject.SetActive(true);
                    }

                    float BorrowMaxtime = Borrow_MaxTime;
                    BorrowingTimer.value = BorrowMaxtime;

                    while (Stuck(false) && DominoController.Instance.GetTilesOutSide().Count > 0)
                    {
                        Borrowing = true;

                        if ((Manager.GameManager.GameMode == GameMode.Offline))
                            yield return new WaitForSecondsRealtime(borrow_Delay);
                        else
                            yield return null;

                        if (!(Manager.GameManager.GameMode == GameMode.Offline))
                        {
                            while (Borrowing && BorrowMaxtime >= 0f)
                            {
                                if (!(Manager.GameManager.GameMode == GameMode.Offline) || (Manager.GameManager.GameMode == GameMode.Offline) && Game_Is_On)
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

                        if ((Manager.GameManager.GameMode == GameMode.Offline))
                        {
                            Borrow(false);
                        }
                        else
                        {
                            Borrow(false);
                            view.RPC("Borrow", RpcTarget.OthersBuffered, false);
                        }

                        ReOrganizeCardsInHands();
                    }

                    if (!(Manager.GameManager.GameMode == GameMode.Offline))
                    {
                        BorrwoingANim.Play(borrowingOut_Anim.name);
                        BorrowingTimer.gameObject.SetActive(false);
                    }

                    if (Stuck(false) && DominoController.Instance.GetTilesOutSide().Count == 0)
                    {
                        if (!(Manager.GameManager.GameMode == GameMode.Offline))
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

        if (!yourTurn && (Manager.GameManager.GameMode == GameMode.Offline))
            StartCoroutine(Ai_Play());

        ReOrganizeCardsInHands();
    }
    bool Borrowing = false;
    [PunRPC] void Borrow(bool Master)
    {
        if(Master)
        {
            MasterCards.Add(DominoController.Instance.GetTilesOutSide()[0]);
        }
        else
        {
            GuestCards.Add(DominoController.Instance.GetTilesOutSide()[0]);
        }

        bool yourTurn = Master_Turn && master_client || !Master_Turn && !master_client;

        if (yourTurn)
            text_comm.PlayText("Borrowing!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);

        var tilesOutside = DominoController.Instance.GetTilesOutSide();

        tilesOutside.Remove(tilesOutside[0]);
        ReOrganizeCardsInHands();
        Referesh_Selection_UI();
    }
    public int Calculate_Score(List<DominoTile> cards)
    {
        int Score = 0;

        foreach (DominoTile t in cards)
        {
            if (t.Up == TileName.Bland)
                Score += 0;
            else if (t.Up == TileName.One)
                Score += 1;
            else if (t.Up == TileName.Two)
                Score += 2;
            else if (t.Up == TileName.Three)
                Score += 3;
            else if (t.Up == TileName.Four)
                Score += 4;
            else if (t.Up == TileName.Five)
                Score += 5;
            else if (t.Up == TileName.Six)
                Score += 6;

            if (t.Down == TileName.Bland)
                Score += 0;
            else if (t.Down == TileName.One)
                Score += 1;
            else if (t.Down == TileName.Two)
                Score += 2;
            else if (t.Down == TileName.Three)
                Score += 3;
            else if (t.Down == TileName.Four)
                Score += 4;
            else if (t.Down == TileName.Five)
                Score += 5;
            else if (t.Down == TileName.Six)
                Score += 6;
        }

        return Score;
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
                        Play_Trigger((int)GroundSide.Center);
                    else if (hit.collider.gameObject == Play_Right_obj)
                        Play_Trigger((int)GroundSide.Right);
                    else if (hit.collider.gameObject == Play_Left_obj)
                        Play_Trigger((int)GroundSide.Left);

                    if (hit.collider.tag == "Tile")
                    {
                        DominoTile t = hit.collider.GetComponent<DominoTile>();

                        if(master_client)
                        {
                            if(MasterCards.Contains(t))
                            {
                                int Selected = MasterCards.IndexOf(t);
                                Change_Selection_Directly(Selected, true);
                            }
                        }
                        else
                        {
                            if (GuestCards.Contains(t))
                            {
                                int Selected = GuestCards.IndexOf(t);
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
                            if ((Manager.GameManager.GameMode == GameMode.Offline))
                                Remove_AnExtraTile(i);
                            else
                                view.RPC("Remove_AnExtraTile", RpcTarget.All, i);
                        }
                    }

                }
            }
        }

        if (Input.GetKeyDown("x"))
            ReOrganizeCardsInHands();
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
                if ((Manager.GameManager.GameMode == GameMode.Offline))
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
            if (!DominoGeometery.Instance.CenterCard)
            {
                Play_Trigger((int)GroundSide.Center);
            }
            else
            {
                for (int i = 0; i <= GuestCards.Count - 1; i++)
                {
                    Guest_Selected_Card = i;

                    int Selected_Card = Guest_Selected_Card;
                    List<DominoTile> cards = GuestCards;

                    bool leftAv = (DominoGeometery.Instance.LeftAvailable == cards[Selected_Card].Up) || (DominoGeometery.Instance.LeftAvailable == cards[Selected_Card].Down);
                    bool RightAv = (DominoGeometery.Instance.RightAvailable == cards[Selected_Card].Up) || (DominoGeometery.Instance.RightAvailable == cards[Selected_Card].Down);

                    if (leftAv)
                    {
                        Play_Trigger((int)GroundSide.Left);
                        break;
                    }
                    else if (RightAv)
                    {
                        Play_Trigger((int)GroundSide.Right);
                        break;
                    }
                }
            }
        }
    }
}
