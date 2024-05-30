using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class DominoPlayer : MonoBehaviour
{
    public static DominoPlayer Instance;

    [SerializeField] Camera_Controller camera_Controller;
    [SerializeField] LayerMask tiles;
    [SerializeField] Animation BorrwoingANim;
    [SerializeField] AnimationClip borrowing_Anim;
    [SerializeField] AnimationClip borrowingOut_Anim;

    [Header("Score and Turns")]
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
    
    //System Variables
    PhotonView view;
    [System.NonSerialized] public int Master_Selected_Card = 3;
    [System.NonSerialized] public int Guest_Selected_Card = 3;
    [System.NonSerialized] public List<DominoTile> MasterCards = new List<DominoTile>();
    [System.NonSerialized] public List<DominoTile> GuestCards = new List<DominoTile>();
    [System.NonSerialized] public bool GameIsOn = true;


    float Slider_Adding;
    bool Master_Turn = false;
    float Timer;
    bool Played_Locally;
    bool isMasterClient;
    ButtonSound mybutton_Sound;
    
    DominoPlayer()
    {
        Instance = this;
    }

    private void Awake()
    {

        mybutton_Sound = GetComponent<ButtonSound>();
        
        UpdateMaster_Client();

        Panel.GetPanel<DominoPanel>().SetPlayersScore(Manager.GameManager.CurrentScore[0], Manager.GameManager.CurrentScore[1]);

        Timer = DominoController.Instance.TurnTime;
        
        if(!(Manager.GameManager.GameMode == GameMode.Offline))
            view = GetComponent<PhotonView>();

        if (Manager.GameManager.MasterWonLastGame)
            Master_Turn = false;
        else
            Master_Turn = true;

        if (!(Manager.GameManager.GameMode == GameMode.Offline))
        {
            if (isMasterClient)
                view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
        }
        else
        {
            if (isMasterClient)
                Switch_Turn_trigger();
        }
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
            IsMaster = isMasterClient;
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
            
            if(isMasterClient)
                Panel.GetPanel<TextPopUpsPanel>().PlayText("Round Winner!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);

            return;
        }
        else if (GuestCards.Count == 0)
        {
            DominoController.Instance.EndRound(Winner.Guest, true,false);

            if (!isMasterClient)
                Panel.GetPanel<TextPopUpsPanel>().PlayText("Round Winner!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);

            return;
        }

        if (isMasterClient)
        {
            if ((Manager.GameManager.GameMode == GameMode.Offline))
                Switch_Turn_trigger();
            else
                view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
        }
        Referesh_Selection_UI();
        ReOrganizeAllCards();
    }


    //Selected card functions
    public void Change_Selected_UI_Trigger(int i)
    {
        bool IsMaster = isMasterClient;

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
        ReOrganizeAllCards();
    }
    void Change_Selection_Directly(int Selected, bool IsMaster)
    {
        if ((Manager.GameManager.GameMode == GameMode.Offline))
            Change_Selected_Directly(Selected, IsMaster);
        else
            view.RPC("Change_Selected_Directly", RpcTarget.AllBuffered, Selected, IsMaster);

        Referesh_Selection_UI();
        ReOrganizeAllCards();
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


    public void ReOrganizeAllCards()
    {
        if (isMasterClient)
        {
            DominoGeometery.Instance.ReOrganize(MasterCards, Master_Selected_Card,0f, false, Z_Depth);
            DominoGeometery.Instance.ReOrganize(GuestCards, Guest_Selected_Card, GuestVertical, true, Guest_Z_Depth);
        }
        else
        {
            DominoGeometery.Instance.ReOrganize(GuestCards, Guest_Selected_Card, 0f, false, Z_Depth);
            DominoGeometery.Instance.ReOrganize(MasterCards, Master_Selected_Card, GuestVertical, true, Guest_Z_Depth);
        }

        DominoGeometery.Instance.ReOrganizeExtras();

        RefreshPlayableSides(isMasterClient);
    }
 
    private void RefreshPlayableSides(bool isMaster)
    {
        int selectedCards = isMaster ? Master_Selected_Card : Guest_Selected_Card;
        List<DominoTile> cards = isMaster ? MasterCards : GuestCards;

        bool centerState = !DominoGeometery.Instance.CenterCard;
        bool leftState = !centerState && (DominoGeometery.Instance.LeftAvailable == cards[selectedCards].Up) || (DominoGeometery.Instance.LeftAvailable == cards[selectedCards].Down);
        bool rightState = !centerState && (DominoGeometery.Instance.RightAvailable == cards[selectedCards].Up) || (DominoGeometery.Instance.RightAvailable == cards[selectedCards].Down);

        DominoGeometery.Instance.RefreshPlayableSides(centerState, leftState, rightState);
    }
    void Referesh_Selection_UI()
    {
        List<DominoTile> cards = new List<DominoTile>();

        Fix_Selected();

        if (isMasterClient)
        {
            cards = MasterCards;
        }
        else if (!isMasterClient)
        {
            cards = GuestCards;
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
        Timer = DominoController.Instance.TurnTime;
        is_4_yet = false;
        is_3_yet = false;
        is_2_yet = false;
        is_1_yet = false;

        Panel.GetPanel<DominoPanel>().SetTimer(1f);

        bool yourTurn = Master_Turn && isMasterClient || !Master_Turn && !isMasterClient;

        if (yourTurn && !Stuck(true) && !Stuck(false))
            Panel.GetPanel<TextPopUpsPanel>().PlayText("Your Turn!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);


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
                if (isMasterClient)
                    view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
            }

            Panel.GetPanel<TextPopUpsPanel>().PlayText("Pass, No Cards!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);

        }
        else if (!Master_Turn && Stuck(false) && DominoController.Instance.GetTilesOutSide().Count == 0)
        {
            if ((Manager.GameManager.GameMode == GameMode.Offline))
            {
                Switch_Turn_trigger();
            }
            else
            {
                if (isMasterClient)
                    view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
            }

            Panel.GetPanel<TextPopUpsPanel>().PlayText("Pass, No Cards!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);
        }
        else
        {
            if (Master_Turn)
            {
                bool Master_Client = (Manager.GameManager.GameMode == GameMode.Offline) ? true : isMasterClient;

                if (Master_Client && Stuck(true) && DominoGeometery.Instance.CenterCard)
                {

                    if (DominoController.Instance.GetTilesOutSide().Count > 0)
                    {
                        BorrwoingANim.Play(borrowing_Anim.name);
                        
                        Panel.GetPanel<DominoPanel>().ShowBorrowTimer(true);
                    }
                    
                    float borrowTimer = DominoController.Instance.BorrowTime;
                    Panel.GetPanel<DominoPanel>().SetBorrowTimer(1f);


                    while (Stuck(true) && DominoController.Instance.GetTilesOutSide().Count > 0)
                    {
                        yield return null;
                        
                        Borrowing = true;

                        while (Borrowing && borrowTimer >= 0f)
                        {
                            if(!(Manager.GameManager.GameMode == GameMode.Offline) || (Manager.GameManager.GameMode == GameMode.Offline) && GameIsOn)
                                borrowTimer -= (Time.fixedDeltaTime*Time.timeScale);

                            Panel.GetPanel<DominoPanel>().SetBorrowTimer(borrowTimer/ DominoController.Instance.BorrowTime);
                            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime); 
                        }

                        if(borrowTimer < 0f)
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

                        ReOrganizeAllCards();
                    }
                    
                    BorrwoingANim.Play(borrowingOut_Anim.name);
                    Panel.GetPanel<DominoPanel>().ShowBorrowTimer(false);

                    if (Stuck(true) && DominoController.Instance.GetTilesOutSide().Count == 0)
                    {
                        if (!(Manager.GameManager.GameMode == GameMode.Offline))
                            view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
                        else
                            Switch_Turn_trigger();
                    }
                }

                if (isMasterClient)
                {
                    Panel.GetPanel<GameNormalPanel>().SetTurnUi(Turn.MyTurn);
                }
                else
                {
                    Panel.GetPanel<GameNormalPanel>().SetTurnUi(Turn.OpponentTurn);
                }
            }
            else
            {
                bool Master_Client = (Manager.GameManager.GameMode == GameMode.Offline) ? false : isMasterClient;

                if (!Master_Client && Stuck(false) && DominoGeometery.Instance.CenterCard)
                {
                    if (DominoController.Instance.GetTilesOutSide().Count > 0 && !(Manager.GameManager.GameMode == GameMode.Offline))
                    {
                        BorrwoingANim.Play(borrowing_Anim.name);
                        Panel.GetPanel<DominoPanel>().ShowBorrowTimer(true);
                    }

                    float borrowTimer = DominoController.Instance.BorrowTime;
                    Panel.GetPanel<DominoPanel>().SetBorrowTimer(1f);


                    while (Stuck(false) && DominoController.Instance.GetTilesOutSide().Count > 0)
                    {
                        Borrowing = true;

                        if ((Manager.GameManager.GameMode == GameMode.Offline))
                            yield return new WaitForSecondsRealtime(borrow_Delay);
                        else
                            yield return null;

                        if (!(Manager.GameManager.GameMode == GameMode.Offline))
                        {
                            while (Borrowing && borrowTimer >= 0f)
                            {
                                if (!(Manager.GameManager.GameMode == GameMode.Offline) || (Manager.GameManager.GameMode == GameMode.Offline) && GameIsOn)
                                    borrowTimer -= (Time.fixedDeltaTime*Time.timeScale);

                                Panel.GetPanel<DominoPanel>().SetBorrowTimer(borrowTimer/ DominoController.Instance.BorrowTime);
                                yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
                            }

                            if (borrowTimer < 0f)
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

                        ReOrganizeAllCards();
                    }

                    if (!(Manager.GameManager.GameMode == GameMode.Offline))
                    {
                        BorrwoingANim.Play(borrowingOut_Anim.name);
                        Panel.GetPanel<DominoPanel>().ShowBorrowTimer(false);
                    }

                    if (Stuck(false) && DominoController.Instance.GetTilesOutSide().Count == 0)
                    {
                        if (!(Manager.GameManager.GameMode == GameMode.Offline))
                            view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
                        else
                            Switch_Turn_trigger();
                    }
                }

                if (!isMasterClient)
                {
                    Panel.GetPanel<GameNormalPanel>().SetTurnUi(Turn.MyTurn);
                }
                else
                {
                    Panel.GetPanel<GameNormalPanel>().SetTurnUi(Turn.OpponentTurn);
                }
            }
        }

        if (!yourTurn && (Manager.GameManager.GameMode == GameMode.Offline))
            StartCoroutine(Ai_Play());

        ReOrganizeAllCards();
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

        bool yourTurn = Master_Turn && isMasterClient || !Master_Turn && !isMasterClient;

        if (yourTurn)
            Panel.GetPanel<TextPopUpsPanel>().PlayText("Borrowing!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);

        var tilesOutside = DominoController.Instance.GetTilesOutSide();

        tilesOutside.Remove(tilesOutside[0]);
        ReOrganizeAllCards();
        Referesh_Selection_UI();
    }
    public int CalculateScore(List<DominoTile> cards)
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
            isMasterClient = true;
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
                isMasterClient = true;
            else
                isMasterClient = false;
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
                    GroundSide side = DominoGeometery.Instance.GetWhichSideHit(hit.collider.gameObject);
                    
                    if(side != GroundSide.None)
                        Play_Trigger((int)side);

                    if (hit.collider.tag == "Tile")
                    {
                        DominoTile t = hit.collider.GetComponent<DominoTile>();

                        if(isMasterClient)
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
                        int i =  DominoGeometery.Instance.GetFakeBorrowTilesIndex(t);
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
            ReOrganizeAllCards();
    }

    [PunRPC] void Remove_AnExtraTile(int i)
    {
            GameObject extraTile = DominoGeometery.Instance.GetFakeBorrowTile(i);
            DominoGeometery.Instance.RemoveFakeBorrowTile(extraTile);
            DestroyImmediate(extraTile.gameObject);
            Borrowing = false;
    }

    public bool IsMyTurn()
    {
        return Master_Turn && isMasterClient || !Master_Turn && !isMasterClient;
    }
    private void FixedUpdate()
    {
        if (Timer <= 0)
        {
            bool yourTurn = IsMyTurn();

            if (yourTurn)
                Panel.GetPanel<TextPopUpsPanel>().PlayText("Switching Round!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);

            Timer = DominoController.Instance.TurnTime;

            if (isMasterClient)
            {
                if ((Manager.GameManager.GameMode == GameMode.Offline))
                    Switch_Turn_trigger();
                else
                    view.RPC("Switch_Turn_trigger", RpcTarget.AllBuffered);
            }
        }
        else
        {
            if (GameIsOn)
            {
                Timer -= Time.fixedDeltaTime;
                
                float timeRatioLeft= (Timer / DominoController.Instance.TurnTime);

                Panel.GetPanel<DominoPanel>().SetTimer(timeRatioLeft);

                bool yourTurn = Master_Turn && isMasterClient || !Master_Turn && !isMasterClient;

                if (yourTurn)
                {
                    if (Timer <= 4 && !is_4_yet)
                    {
                        is_4_yet = true;
                        Panel.GetPanel<TextPopUpsPanel>().PlayText("Time!", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);
                    }
                    else if (Timer <= 3 && !is_3_yet)
                    {
                        is_3_yet = true;
                        Panel.GetPanel<TextPopUpsPanel>().PlayText("3", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);
                    }
                    else if (Timer <= 2 && !is_2_yet)
                    {
                        is_2_yet = true;
                        Panel.GetPanel<TextPopUpsPanel>().PlayText("2", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);
                    }
                    else if (Timer <= 1 && !is_1_yet)
                    {
                        is_1_yet = true;
                        Panel.GetPanel<TextPopUpsPanel>().PlayText("1", text_initial_Color, text_Speed, text_initialScale, text_final_Scale);
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
