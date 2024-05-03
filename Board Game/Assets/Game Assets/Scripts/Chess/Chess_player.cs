using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System;


public class Chess_player : MonoBehaviour
{
    [Header("System parameters")]
    [SerializeField] LayerMask Chess_Pieces;
    [SerializeField] List<Tile_Row> Tile_Columns = new List<Tile_Row>();
    [SerializeField] List<Chess_piece> White_Pieces = new List<Chess_piece>();
    [SerializeField] List<Chess_piece> Black_Pieces = new List<Chess_piece>();
    [SerializeField] List<Chess_piece> AllPieces_Used = new List<Chess_piece>();
    [SerializeField] string TileNormal_Tag = "Tile";
    [SerializeField] string TileEnemy_Tag = "Enemy";
    [SerializeField] string TileCasle_Tag = "Castle";

    [Header("References")]
    [SerializeField] Chess_Organizer organizer;
    [SerializeField] Text timer_text;
    [SerializeField] Text Guest_timer_text;
    [SerializeField] Text_Comments_Generator text_comm;
    [SerializeField] Text TurnText;
    [SerializeField] GameObject chat_ui;
    [SerializeField] GameObject BlackQueen;
    [SerializeField] GameObject BlackKnight;
    [SerializeField] GameObject BlackBishop;
    [SerializeField] GameObject BlackRock;
    [SerializeField] GameObject WhiteQueen;
    [SerializeField] GameObject WhiteKnight;
    [SerializeField] GameObject WhiteBishop;
    [SerializeField] GameObject WhiteRock;
    [SerializeField] Chess_Piece_Type promote_To;
    [SerializeField] GameObject ChoosePromotion_UI;
    [SerializeField] Messages_Manager mymessages;
    [SerializeField] Messages_Manager MyLogBook;

    [Header("Out positioning")]
    [SerializeField] GameObject my_Out;
    [SerializeField] List<Chess_piece> my_Out_pieces = new List<Chess_piece>();
    [SerializeField] GameObject Enemy_Out;
    [SerializeField] List<Chess_piece> Enemy_out_pieces = new List<Chess_piece>();
    [SerializeField] Vector2 spacing;

    [Header("Turn Text")]
    [SerializeField] string yourTurn_text = "Your Turn";
    [SerializeField] Color yourturn_color = Color.green;
    [SerializeField] string otherTurn_text = "Other Turn";
    [SerializeField] Color otherTurn_color = Color.red;

    [Header("Timer")]
    [SerializeField] public float Max_Time = 5f;
    [SerializeField] public float bonus = 1f;


    //internal variables
    [System.NonSerialized] public bool GameIsOn = true;
    Chess_piece Currently_Selected;
    ButtonSound sounds;
     bool Master_Turn;
    public float Timer;
    public float Guest_Timer;
    bool is_4_yet;
    bool is_3_yet;
    bool is_2_yet;
    bool is_1_yet;
    PhotonView view;
    bool _CurrentView_3d = false;

    private void Start()
    {
        sounds = GetComponent<ButtonSound>();
        Timer = Max_Time;
        Guest_Timer = Max_Time;
        Master_Turn = true;
        Update_TurnUI();

        foreach (Chess_piece p in Black_Pieces)
            AllPieces_Used.Add(p);
        foreach (Chess_piece p in White_Pieces)
            AllPieces_Used.Add(p);

        bool data = PlayerPrefs.GetInt("view2D") == 1 ? false : true;
        _CurrentView_3d = !data;
        ToggleView();

        if (!Cross_Scene_Data.AI)
        {
            view = GetComponent<PhotonView>();
        }

        if (!organizer.master_client)
            foreach (Chess_piece p in AllPieces_Used)
                p.GetComponent<ChessView>()._2DView.transform.Rotate(0, 180f, 0);

        Destroy_Enemy_Colliders();
    }
    private void Update()
    {
        if (Input.touchCount > 0 || Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (Myturn() && GameIsOn && !Choosing)
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

                if (Physics.Raycast(ray, out hit, 50, Chess_Pieces))
                {
                    if(Myturn())
                        StartCoroutine(InputHandle(hit.collider.gameObject,Currently_Selected, organizer.master_client));
                }
            }
        }
    }

    IEnumerator InputHandle(GameObject _Ojbect, Chess_piece current, bool master)
    {
        if (_Ojbect != null)
        {
            float board_Dir = organizer.master_client ? 1f : -1f;

            //if it's a tile
            if (_Ojbect.tag == TileNormal_Tag || _Ojbect.tag == TileEnemy_Tag || _Ojbect.tag == TileCasle_Tag)
            {
                //kicking a piece out if it was a move and a kick
                List<Chess_piece> mypieces = master ? White_Pieces : Black_Pieces;
                string newPosition = _Ojbect.name.ToLower();
                Vector2 newPosition_vec = Convert_Text_Num(newPosition);
                string pieceName = Convert_Object_ToPieceName(current);
                string enemyPiece = "";

                if (_Ojbect.tag == TileEnemy_Tag)
                {
                    List<Chess_piece> enemypieces = master ? Black_Pieces : White_Pieces;

                    foreach (Chess_piece enemy in enemypieces)
                    {
                        bool EnPassentTarget = (enemy.currentposition == newPosition_vec + new Vector2(0f, -board_Dir)) && enemy.EnPassent_Possible;

                        if ((enemy.currentposition == newPosition_vec) || EnPassentTarget)
                        {
                            enemyPiece = Convert_Object_ToPieceName(enemy);
                        }
                    }
                }
                if (_Ojbect.tag == TileCasle_Tag)
                {
                    //getting rock
                    Chess_piece myRock = new Chess_piece();
                    foreach (Chess_piece friendly in mypieces)
                    {
                        if (friendly.piece_type == Chess_Piece_Type.Rock)
                            if (Mathf.Abs((newPosition_vec - friendly.currentposition).x) < 4)
                                myRock = friendly;
                    }

                    //moving the rock
                    Vector2 Delta_newposition;

                    if ((myRock.currentposition - current.currentposition).x > 0)
                    {
                        Delta_newposition = new Vector2(-1f, 0);
                    }
                    else
                    {
                        Delta_newposition = new Vector2(1f, 0f);
                    }

                    string newRockPosition = Convert_Num_Text(newPosition_vec + Delta_newposition);


                    if (Cross_Scene_Data.AI)
                        Move_Piece(Convert_Object_ToPieceName(myRock), newRockPosition, "", false, false, 0);
                    else
                        view.RPC("Move_Piece", RpcTarget.AllBuffered, Convert_Object_ToPieceName(myRock), newRockPosition, "", false, false, 0);
                }

                //promotion handle
                bool promote = false;
                if (current.piece_type == Chess_Piece_Type.Pawn)
                {
                    bool WhitePromote = (current.color == Piece_Color.White) && (newPosition_vec.y == 8);
                    bool BlackPromote = (current.color == Piece_Color.Black) && (newPosition_vec.y == 1);

                    promote = WhitePromote || BlackPromote;
                }
                if (promote)
                {
                    if(Cross_Scene_Data.AI && !Master_Turn)
                    {
                        promote_To = Chess_Piece_Type.Queen;
                    }
                    else
                    {
                        ChoosePromotion_UI.SetActive(true);
                        Choosing = true;

                        while (Choosing)
                        {
                            yield return null;
                        }
                    }
                }

                Chess_Piece_Type promotionType = promote_To;

                //Move Piece
                if (Cross_Scene_Data.AI)
                {
                    if(Myturn())
                    {
                        if(bonus > 0)
                            text_comm.PlayText("+ " + bonus + " Seconds");

                        MyLogBook.Add_message("P1", ComputeMoveFEN(current, enemyPiece, newPosition), MyLogBook.myColor);
                    }
                    else
                    {
                        MyLogBook.Add_message("P2", ComputeMoveFEN(current, enemyPiece, newPosition), MyLogBook.OtherColor);
                    } 

                    Move_Piece(pieceName, newPosition, enemyPiece, true, promote, (int)promotionType);

                }
                else
                {
                    MyLogBook.Logg_Message(ComputeMoveFEN(current, enemyPiece, newPosition));

                    view.RPC("Move_Piece", RpcTarget.AllBuffered, pieceName, newPosition, enemyPiece, true, promote, (int)promotionType);

                    if (bonus > 0)
                        text_comm.PlayText("+ " + bonus + " Seconds");
                }
            }

            //if it's a piece
            bool myPieces = (_Ojbect.tag == "BlackPiece" && !master) || (_Ojbect.tag == "WhitePiece" && master);
            if (myPieces)
            {
                Chess_piece piece = _Ojbect.GetComponent<Chess_piece>();

                if (Cross_Scene_Data.AI)
                {
                    if(Master_Turn)
                        UpdateSelectedPiece(piece);

                    HighLight_PossibleMoves(piece, Master_Turn);
                }
                else
                {
                    UpdateSelectedPiece(piece);
                    HighLight_PossibleMoves(piece, organizer.master_client);
                }
            }
        }
    }
    private void FixedUpdate()
    {
        if (Timer <= 0 || Guest_Timer <= 0)
        {
            if (Myturn())
                text_comm.PlayText("Time Out!");

            HighLight_PossibleMoves(null, false);
            HighLight_PossibleMoves(null, true);
            Choosing = false;

            if (organizer.master_client)
            {
                if (Cross_Scene_Data.AI)
                {
                    if (Timer <= 0)
                        EndGame(!organizer.master_client, false,true);
                    else if(Guest_Timer <=0)
                        EndGame(organizer.master_client, false,true);
                }
                else
                {
                    if (organizer.master_client)
                        if (Timer <= 0)
                            view.RPC("EndGame", RpcTarget.AllBuffered, !organizer.master_client, false, true);
                        else if (Guest_Timer <= 0)
                            view.RPC("EndGame", RpcTarget.AllBuffered, organizer.master_client, false, true);
                }
            }

            Timer = 0.01f;
            Guest_Timer = 0.01f;
        }
        else
        {
            if (GameIsOn)
            {
                if(Master_Turn)
                    Timer -= Time.fixedDeltaTime;
                else
                    Guest_Timer -= Time.fixedDeltaTime;

                if (organizer.master_client)
                {
                    timer_text.text = Min_Sec(Timer);
                    Guest_timer_text.text = Min_Sec(Guest_Timer);
                }
                else
                {
                    timer_text.text = Min_Sec(Guest_Timer);
                    Guest_timer_text.text = Min_Sec(Timer);
                }

                if (Myturn())
                {
                    if (Timer <= 4 && !is_4_yet)
                    {
                        is_4_yet = true;
                        text_comm.PlayText("Time!");
                    }
                    else if (Timer <= 3 && !is_3_yet)
                    {
                        is_3_yet = true;
                        text_comm.PlayText("3");
                    }
                    else if (Timer <= 2 && !is_2_yet)
                    {
                        is_2_yet = true;
                        text_comm.PlayText("2");
                    }
                    else if (Timer <= 1 && !is_1_yet)
                    {
                        is_1_yet = true;
                        text_comm.PlayText("1");
                    }
                }
            }
        }
    }

    bool Choosing = false;
    public void Choose_Promotion(int i)
    {
        ChoosePromotion_UI.SetActive(false);
        if (i == 0)
            promote_To = Chess_Piece_Type.Queen;
        else if (i == 1)
            promote_To = Chess_Piece_Type.Rock;
        else if (i == 2)
            promote_To = Chess_Piece_Type.Bishop;
        else if (i == 3)
            promote_To = Chess_Piece_Type.Knight;

        Choosing = false;
    }
    void HighLight_PossibleMoves(Chess_piece piece, bool master)
    {
        //Deactivate all
        float board_Dir = master ? 1f : -1f;
        List<Chess_piece> enemies = master ? Black_Pieces : White_Pieces;
        foreach (Tile_Row r in Tile_Columns)
        {
            foreach (GameObject tile in r.tile_row)
            {
                tile.SetActive(false);
                tile.tag = TileNormal_Tag;
                tile.GetComponent<MeshRenderer>().material.SetFloat("_Friend", 1f);
                tile.GetComponent<MeshRenderer>().material.SetFloat("_Castle", 0f);
            }
        }
        foreach(Chess_piece enemy in enemies)
        {
            enemy.GetComponent<MeshRenderer>().material.SetFloat("_Highlight", 0f);
            enemy.GetComponent<ChessView>()._2DView_mesh.material.SetFloat("_Highlight", 0f);
        }

        //Activate Only legal tiles
        if (piece != null)
        {
            List<Vector2> legalMoves = LegalMoves(piece, master,true,true);

            foreach (Vector2 legalMove in legalMoves)
            {
                GetTile(legalMove).SetActive(true);

                //if an enemy sets on it
                foreach(Chess_piece enemy in enemies)
                {
                    bool EnPassentTarget = (enemy.currentposition == legalMove + new Vector2(0f, -board_Dir)) && enemy.EnPassent_Possible && (piece.piece_type == Chess_Piece_Type.Pawn);

                    if ((enemy.currentposition == legalMove) || EnPassentTarget)
                    {
                        enemy.GetComponent<MeshRenderer>().material.SetFloat("_Highlight", 1f);
                        enemy.GetComponent<MeshRenderer>().material.SetFloat("_Friend", 0f);

                        enemy.GetComponent<ChessView>()._2DView_mesh.material.SetFloat("_Highlight", 1f);
                        enemy.GetComponent<ChessView>()._2DView_mesh.material.SetFloat("_Friend", 0f);

                        if (EnPassentTarget)
                        {
                            GetTile(enemy.currentposition + new Vector2(0f, board_Dir)).GetComponent<MeshRenderer>().material.SetFloat("_Friend", 0f);
                            GetTile(enemy.currentposition + new Vector2(0f, board_Dir)).tag = TileEnemy_Tag;
                        }
                        else
                        {
                            GetTile(enemy.currentposition).GetComponent<MeshRenderer>().material.SetFloat("_Friend", 0f);
                            GetTile(enemy.currentposition).tag = TileEnemy_Tag;
                        }

                        GetTile(enemy.currentposition).tag = TileEnemy_Tag;
                    }
                }

                //castle
                if (piece.piece_type == Chess_Piece_Type.King)
                {
                    List<Vector2> castleMoves = GetCastle(piece, master);

                    foreach (Vector2 move in castleMoves)
                    {
                        GetTile(move).tag = TileCasle_Tag;
                        GetTile(move).GetComponent<MeshRenderer>().material.SetFloat("_Castle", 1f);
                    }
                }
            }

        }
    }
    void UpdateSelectedPiece(Chess_piece piece)
    {
        Currently_Selected = piece;

        //reset old
        if (organizer.master_client)
        {
            foreach (Chess_piece p in White_Pieces)
            {
                p.GetComponent<MeshRenderer>().material.SetFloat("_Highlight", 0f);
                p.GetComponent<ChessView>()._2DView_mesh.material.SetFloat("_Highlight", 0f);
            }
        }
        else
        {
            foreach (Chess_piece p in Black_Pieces)
            {
                p.GetComponent<MeshRenderer>().material.SetFloat("_Highlight", 0f);
                p.GetComponent<ChessView>()._2DView_mesh.material.SetFloat("_Highlight", 0f);
            }
        }

        if (piece != null)
        {
            piece.GetComponent<MeshRenderer>().material.SetFloat("_Highlight", 1f);
            piece.GetComponent<ChessView>()._2DView_mesh.material.SetFloat("_Highlight", 1f);

            piece.GetComponent<MeshRenderer>().material.SetFloat("_Friend", 1f);
            piece.GetComponent<ChessView>()._2DView_mesh.material.SetFloat("_Friend", 1f);
        }
    }
    GameObject GetTile(Vector2 tilePosition)
    {
        return Tile_Columns[(int)tilePosition.x - 1].tile_row[(int)tilePosition.y - 1];
    }

    bool Myturn()
    {
        if (organizer.master_client && Master_Turn || ((!organizer.master_client) && (!Master_Turn)))
            return true;
        else
            return false;
    }
    void Update_TurnUI()
    {
        if(Myturn())
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
    [PunRPC] void Switch_Turn(float masterTimer, float GuestTimer)
    {
        Master_Turn = !Master_Turn;
        Timer = masterTimer;
        Guest_Timer = GuestTimer;
        Choosing = false;
        is_4_yet = false;
        is_3_yet = false;
        is_2_yet = false;
        is_1_yet = false;
        
        HighLight_PossibleMoves(null,organizer.master_client);
        UpdateSelectedPiece(null);

        bool Me_noLegalMoves = NoLegalMoves(organizer.master_client);
        bool Enemy_EnemyKingIsStuck = NoLegalMoves(!organizer.master_client);


        //Check if game should end.
        if (Me_noLegalMoves && Myturn())
        {
            if (IsAKingChecked(organizer.master_client))
            {
                //handel checkmate
                if (Cross_Scene_Data.AI)
                    EndGame(!organizer.master_client, false, false);
                else
                    view.RPC("EndGame", RpcTarget.AllBuffered, !organizer.master_client, false, false);
            }
            else
            {   
                //handel stalemate
                if (Cross_Scene_Data.AI)
                    EndGame(!organizer.master_client, true, false);
                else
                    view.RPC("EndGame", RpcTarget.AllBuffered, !organizer.master_client, true, false);
            }
        }
        else if(Enemy_EnemyKingIsStuck && Cross_Scene_Data.AI && !Myturn())
        {
            if (IsAKingChecked(!organizer.master_client))
                //handel checkmate
                EndGame(organizer.master_client, false, false);
            else
                //handel stalemate
                EndGame(organizer.master_client, true, false);
        }
        else
        {
            if(!Myturn() && Cross_Scene_Data.AI)
                StartCoroutine(playRandomAI());
        }

        //always reset king check, because a round can't switch without the king being unchecked
        if (IsAKingChecked(organizer.master_client))
        {
            text_comm.PlayText("Your king is checked");
            findMyKing(organizer.master_client).GetComponent<MeshRenderer>().material.SetFloat("_Check", 1f);
            findMyKing(organizer.master_client).GetComponent<ChessView>()._2DView_mesh.material.SetFloat("_Check", 1f);
        }
        else
        {
            findMyKing(organizer.master_client).GetComponent<MeshRenderer>().material.SetFloat("_Check", 0f);
            findMyKing(organizer.master_client).GetComponent<ChessView>()._2DView_mesh.material.SetFloat("_Check", 0f);

            if (Myturn())
                text_comm.PlayText("Your Turn!");
        }

        Update_TurnUI();
    }
    [PunRPC] void Move_Piece(string piece_name, string NewPosition, string enemyPiece, bool SwitchTurn, bool promote, int Promotion)
    {
        Chess_piece piece = Convert_PieceNameto_Object(piece_name);
        Vector2 newPosition_vec = Convert_Text_Num(NewPosition);
        GameObject tile = GetTile(newPosition_vec);

        if (enemyPiece != "")
        {
            //Which list it belongs to
            Chess_piece enemy = Convert_PieceNameto_Object(enemyPiece);
            Piece_Color color = enemy.color;
            GameObject outPosition;
            List<Chess_piece> outList;
            List<Chess_piece> mypieces = enemy.color == Piece_Color.White ? White_Pieces : Black_Pieces;

            if ((organizer.master_client && color == Piece_Color.Black) || (!organizer.master_client && color == Piece_Color.White))
            {
                outPosition = my_Out;
                outList = my_Out_pieces;
            }
            else
            {
                outPosition = Enemy_Out;
                outList = Enemy_out_pieces;
            }
            
            outList.Add(enemy);
            mypieces.Remove(enemy);
            enemy.GetComponent<MeshRenderer>().material.SetFloat("_Highlight", 0f);
            enemy.GetComponent<ChessView>()._2DView_mesh.material.SetFloat("_Highlight", 0f);

            //reorganize
            for (int i = 0; i < outList.Count; i++)
            {
                outList[i].currentposition = new Vector2(91, 91);

                if (i % 2 == 0)
                {
                    outList[i].transform.position = outPosition.transform.position + (spacing.x * outPosition.transform.right * (i/2));
                }
                else
                {
                    outList[i].transform.position = outPosition.transform.position + (spacing.x * outPosition.transform.right * ((i-1)/2));
                    outList[i].transform.position += (spacing.y * outPosition.transform.up);
                }
            }
        }

        //reset EnPassent if it wasn't used
        foreach (Chess_piece white in White_Pieces)
            white.EnPassent_Possible = false;
        foreach (Chess_piece black in Black_Pieces)
            black.EnPassent_Possible = false;
        if (piece.piece_type == Chess_Piece_Type.Pawn)
        {
            //En Passent Spacial Move
            Vector2 difference = newPosition_vec - piece.currentposition;
            if (Mathf.Abs(difference.y) == 2 && piece.firstMove)
            {
                piece.EnPassent_Possible = true;
            }
            else
            {
                piece.EnPassent_Possible = false;
            }
        }

        piece.currentposition = newPosition_vec;
        piece.FinalPosition = new Vector3(tile.transform.position.x, piece.transform.position.y, tile.transform.position.z);
        StartCoroutine(Move_Animation(piece));

        //promotion handle
        if (promote)
        {
            List<Chess_piece> pieces = piece.color == Piece_Color.White ? White_Pieces : Black_Pieces;
            GameObject ObjectToPromote = new GameObject();

            if(piece.color == Piece_Color.Black)
            {
                if(Promotion == (int)Chess_Piece_Type.Queen)
                {
                    ObjectToPromote = BlackQueen;
                }
                else if(Promotion == (int)Chess_Piece_Type.Knight)
                {
                    ObjectToPromote = BlackKnight;
                }
                else if (Promotion == (int)Chess_Piece_Type.Rock)
                {
                    ObjectToPromote = BlackRock;
                }
                else if (Promotion == (int)Chess_Piece_Type.Bishop)
                {
                    ObjectToPromote = BlackBishop;
                }
            }
            else
            {
                if (Promotion == (int)Chess_Piece_Type.Queen)
                {
                    ObjectToPromote = WhiteQueen;
                }
                else if (Promotion == (int)Chess_Piece_Type.Knight)
                {
                    ObjectToPromote = WhiteKnight;
                }
                else if (Promotion == (int)Chess_Piece_Type.Rock)
                {
                    ObjectToPromote = WhiteRock;
                }
                else if (Promotion == (int)Chess_Piece_Type.Bishop)
                {
                    ObjectToPromote = WhiteBishop;
                }
            }

            GameObject NewObject = Instantiate(ObjectToPromote.gameObject);

            NewObject.transform.position = piece.FinalPosition;

            Chess_piece newpiece = NewObject.GetComponent<Chess_piece>();

            newpiece.currentposition = newPosition_vec;

            pieces.Remove(piece);
            pieces.Add(newpiece);
            AllPieces_Used.Add(newpiece);
            AllPieces_Used.Remove(piece);

            DestroyImmediate(piece.gameObject);
            Destroy_Enemy_Colliders();

            //refreshView
            ToggleView();
            ToggleView();
        }

        if (SwitchTurn)
        {
            HighLight_PossibleMoves(null,organizer.master_client);
            Manager.GetManager<SoundManager>().PlayButtonClick();

            if (organizer.master_client)
            {
                if (Master_Turn)
                    Timer = Mathf.Clamp(Timer + bonus, 0f, Max_Time);
                else
                    Guest_Timer = Mathf.Clamp(Timer + bonus, 0f, Max_Time);
            }

            if (Cross_Scene_Data.AI)
                Switch_Turn(Timer,Guest_Timer);
            else
            {
                if (organizer.master_client)
                    view.RPC("Switch_Turn", RpcTarget.AllBuffered, Timer,Guest_Timer);
            }


        }

        piece.firstMove = false;
    }
    IEnumerator Move_Animation(Chess_piece myOjb)
    {
        float speed = 0.1f;

        Vector3 Point2 = myOjb.FinalPosition + (0.1f * Vector3.up); ;
        Vector3 FinalPosition = myOjb.FinalPosition;

        while ((myOjb.gameObject.transform.position - Point2).magnitude >= 0.001f && myOjb.currentposition.x <= 8 && myOjb.currentposition.y <= 8)
        {
            myOjb.gameObject.transform.position = Vector3.Lerp(myOjb.transform.position, Point2, Time.deltaTime / speed);
            yield return new WaitForSecondsRealtime(Time.deltaTime* speed);
        }

        while ((myOjb.gameObject.transform.position - FinalPosition).magnitude >= 0.001f && myOjb.currentposition.x <= 8 && myOjb.currentposition.y <= 8)
        {
            myOjb.gameObject.transform.position = Vector3.Lerp(myOjb.transform.position, FinalPosition, Time.deltaTime / speed);
            yield return new WaitForSecondsRealtime(Time.deltaTime* speed);
        }
    }

    [PunRPC] void EndGame(bool masterWon, bool draw, bool TimeOut)
    {
        if (TimeOut)
        {
            organizer.EndGame(masterWon, false, "Time Out! \n");
        }
        else
        {
            if(draw)
            {
                organizer.EndGame(true, true, "Stalemate - Draw! \n");
            }
            else 
            {
                organizer.EndGame(masterWon, false, "Checkmate! \n");
            }
        }
    }
    public void ToggleView()
    {
        _CurrentView_3d = !_CurrentView_3d;

        if(_CurrentView_3d)
        {
            foreach (Chess_piece p in AllPieces_Used)
                p.GetComponent<ChessView>().Turn_3D();
        }
        else
        {
            foreach (Chess_piece p in AllPieces_Used)
                p.GetComponent<ChessView>().Turn2D();
        }

        int data = _CurrentView_3d ? 0 : 1;
        PlayerPrefs.SetInt("view2D", data);
    }

    //AI functions
    IEnumerator playRandomAI()
    {
        yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(1f, 2f));

        if (GameIsOn)
        {
            float AI = UnityEngine.Random.Range(0f, 1f);
            bool Easy = true;


            if (AI > (1f - (Cross_Scene_Data.chess_difficulty / 100f)))
            {
                Easy = false;
            }

            if (Easy)
            {
                List<Vector2> legalMoves = new List<Vector2>();
                int randomPiece = 0;
                while (legalMoves.Count == 0)
                {
                    randomPiece = UnityEngine.Random.Range(0, Black_Pieces.Count);
                    legalMoves = LegalMoves(Black_Pieces[randomPiece], false, true, false);
                    yield return null;
                }
                int random_Move = UnityEngine.Random.Range(0, legalMoves.Count);
                HighLight_PossibleMoves(Black_Pieces[randomPiece], false);

                yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(2f, 3f));

                if (GameIsOn)
                    StartCoroutine(InputHandle(GetTile(legalMoves[random_Move]), Black_Pieces[randomPiece], false));
            }
            else
            {
                List<Move_Score> moves = new List<Move_Score>();
                foreach (Chess_piece p in Black_Pieces)
                {
                    List<Vector2> legalMoves = LegalMoves(p, false, true, true);

                    foreach (Vector2 move in legalMoves)
                    {
                        Vector2 myOrigianlPosition = p.currentposition;
                        p.currentposition = move;

                        Move_Score m = new Move_Score();
                        m.piece = p;
                        m.move = p.currentposition;
                        Chess_piece enemy = new Chess_piece();
                        Vector2 enemyOriginal_position = new Vector2();

                        //Will i eat any enemy?
                        foreach (Chess_piece p_enemy in White_Pieces)
                        {
                            //Am i going to eat an enemy
                            if (p.currentposition == p_enemy.currentposition)
                            {
                                enemy = p_enemy;
                                enemyOriginal_position = p_enemy.currentposition;
                                p_enemy.currentposition = new Vector2(91, 91);
                                m.gain = GetPieceValue(p_enemy);

                                m.log = "Gain is : " + m.gain + " from eating " + p_enemy.name;
                            }
                        }


                        //Will any of my pieces be eaten or git checked or checkmated
                        foreach (Chess_piece p_enemy in White_Pieces)
                        {
                            List<Vector2> Enemy_legalMoves = LegalMoves(p_enemy, true, true, false);

                            foreach (Vector2 legalMove in Enemy_legalMoves)
                            {
                                Vector2 original_enemy = p_enemy.currentposition;
                                p_enemy.currentposition = legalMove;

                                foreach (Chess_piece friend in Black_Pieces)
                                {
                                    if (friend.currentposition == legalMove)
                                    {
                                        if (GetPieceValue(friend) > m.loss)
                                        {
                                            m.loss = GetPieceValue(friend);

                                            m.Log2 = " Loss is : " + m.loss + " from " + friend.name + " Getting eaten";
                                        }
                                    }
                                }

                                //Will i be checked or checkmated
                                if (IsAKingChecked(false))
                                {
                                    m.loss += 0.7f;

                                    if(NoLegalMoves(false))
                                    {
                                        m.loss = int.MaxValue;
                                        m.Log2 = "CheckMated";
                                    }
                                }

                                p_enemy.currentposition = original_enemy;
                            }
                        }

                        //Will i check or checkmate
                        if(IsAKingChecked(true))
                        {
                            m.gain += 0.7f;

                            if (NoLegalMoves(true))
                            {
                                m.gain = int.MaxValue;
                                m.log = "CheckMate!";
                            }
                        }

                        //Check openings
                        if (IsThis_An_Opening_Move(Convert_Num_Text(move), p))
                        {
                            m.gain += 0.5f;
                        }


                        moves.Add(m);
                        p.currentposition = myOrigianlPosition;
                        if (enemy)
                            enemy.currentposition = enemyOriginal_position;


                        //Check castleing (must be after positions back)
                        if (GetCastle(findMyKing(false), false).Count > 0)
                        {
                            List<Vector2> CastleMoves = GetCastle(findMyKing(false), false);

                            if (p.piece_type == Chess_Piece_Type.King)
                            {
                                foreach (Vector2 castle_Move in CastleMoves)
                                    if (move == castle_Move)
                                        m.gain += 1.5f;
                            }
                        }
                    }
                }
                Move_Score bestMove = ReturnBestMove(moves);

                //Debug Moves
                string calc = "";
                foreach (Move_Score m in moves)
                {
                    calc += Convert_Num_Text(m.move) + " gets ";

                    if (m.gain > 0)
                        calc += m.log;
                    else
                        calc += "No gain";

                    calc += " and ";

                    if (m.loss > 0)
                        calc += m.Log2;
                    else
                        calc += "No loss";

                    calc += " and total is : " + (m.gain - m.loss) + "\n";
                }
                Debug.Log(calc);


                HighLight_PossibleMoves(bestMove.piece, false);
                yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0.5f, 1f));

                if (GameIsOn)
                    StartCoroutine(InputHandle(GetTile(bestMove.move), bestMove.piece, false));
            }
        }
    }
    Move_Score ReturnBestMove(List<Move_Score> moves)
    {
        Move_Score bestMove = moves[0];

        foreach (Move_Score move in moves)
        {
            float score = move.gain - move.loss;

            if (score >= (bestMove.gain - bestMove.loss))
                bestMove = move;
        }

        return bestMove;
    }
    bool IsThis_An_Opening_Move(string move, Chess_piece p)
    {
        bool An_Openning = false;

        if (p.firstMove)
        {
            if (p.color == Piece_Color.White)
            {
                if (p.piece_type == Chess_Piece_Type.Pawn)
                {
                    if (move == "e4" || move == "e3" || move == "d4" || move == "d3")
                        An_Openning = true;
                }
                else if (p.piece_type == Chess_Piece_Type.Knight)
                {
                    if (move == "f3" || move == "c3")
                        An_Openning = true;
                }
                else if (p.piece_type == Chess_Piece_Type.Bishop)
                {
                    if (move == "e3" || move == "d3")
                        An_Openning = true;
                }
            }
            else
            {
                if (p.piece_type == Chess_Piece_Type.Pawn)
                {
                    if (move == "e6" || move == "e5" || move == "d6" || move == "d5")
                        An_Openning = true;
                }
                else if (p.piece_type == Chess_Piece_Type.Knight)
                {
                    if (move == "f6" || move == "c6")
                        An_Openning = true;
                }
                else if (p.piece_type == Chess_Piece_Type.Bishop)
                {
                    if (move == "e6" || move == "d6")
                        An_Openning = true;
                }
            }
        }

        return An_Openning;
    }

    //Helping functions
    void Destroy_Enemy_Colliders()
    {
        List<Chess_piece> enemyPieces = organizer.master_client ? Black_Pieces : White_Pieces;

        foreach (Chess_piece enemy in enemyPieces)
        {
            if(enemy.GetComponent<Collider>())
                DestroyImmediate(enemy.GetComponent<Collider>());
        }
    }
    List<Vector2> GetCastle(Chess_piece piece, bool master)
    {
        List<Vector2> additional_Moves = new List<Vector2>();

        if (piece.piece_type == Chess_Piece_Type.King)
        {
            if (piece.firstMove)
            {
                List<Chess_piece> mypieces = master ? White_Pieces : Black_Pieces;
                List<Chess_piece> enemypieces = master ? Black_Pieces : White_Pieces;

                foreach (Chess_piece mypiece in mypieces)
                {
                    if (mypiece.piece_type == Chess_Piece_Type.Rock)
                    {
                        if (mypiece.firstMove)
                        {
                            List<Vector2> tiles_InBetween = new List<Vector2>();
                            Vector2 distance = piece.currentposition - mypiece.currentposition;

                            for (int i = 1; i < Mathf.Abs(distance.x); i++)
                                tiles_InBetween.Add(new Vector2(piece.currentposition.x - ((distance.x / Mathf.Abs(distance.x)) * i), piece.currentposition.y));

                            bool Empty_InBetween = true;

                            foreach (Chess_piece friendly in mypieces)
                            {
                                foreach (Vector2 tile in tiles_InBetween)
                                {
                                    if (friendly.currentposition == tile)
                                    {
                                        Empty_InBetween = false;
                                    }
                                }
                            }


                            bool TilesAreChecked = false;

                            foreach (Vector2 tile in tiles_InBetween)
                            {
                                foreach (Chess_piece enemypiece in enemypieces)
                                {
                                    List<Vector2> legalMoves = LegalMoves(enemypiece, !master, false, false);

                                    foreach (Vector2 legalMove in legalMoves)
                                    {
                                        if (tile == legalMove)
                                            TilesAreChecked = true;
                                    }
                                }
                            }

                            if (Empty_InBetween && !TilesAreChecked && !IsAKingChecked(master))
                            {
                                Vector2 tile = new Vector2(piece.currentposition.x - ((distance.x / Mathf.Abs(distance.x)) * 2), piece.currentposition.y);
                                additional_Moves.Add(tile);
                            }

                        }
                    }
                }
            }
        }

        return additional_Moves;
    }
    Chess_piece findMyKing(bool master)
    {
        List<Chess_piece> mypieces = master ? White_Pieces : Black_Pieces;
        Chess_piece king = new Chess_piece();

        foreach (Chess_piece friend in mypieces)
        {
            if (friend.piece_type == Chess_Piece_Type.King)
                king = friend;
        }

        return king;
    }
    bool NoLegalMoves(bool master)
    {
        //is there's any legal moves
        int Num_Of_Legal_Moves = 0;
        List<Chess_piece> mypieces = master ? White_Pieces : Black_Pieces;

        foreach (Chess_piece piece in mypieces)
            Num_Of_Legal_Moves += LegalMoves(piece, master, true, false).Count;

        return (Num_Of_Legal_Moves == 0);
    }
    bool IsAKingChecked(bool master)
    {
        //find king's position
        Vector2 myKing_position = findMyKing(master).currentposition;

        //find if any enemy is checking my king
        List<Chess_piece> enemypieces = master ? Black_Pieces : White_Pieces;

        bool isKingChecked = false;
        foreach (Chess_piece enemy in enemypieces)
        {
            List<Vector2> legalMoves = LegalMoves(enemy, !master, false, false);

            foreach (Vector2 legalMove in legalMoves)
            {
                if (legalMove == myKing_position)
                    isKingChecked = true;
            }
        }

        return isKingChecked;
    }
    string Convert_Num_Text(Vector2 position)
    {
        string position_String = "";

        if (position.x == 1)
            position_String += "a";
        else if (position.x == 2)
            position_String += "b";
        else if (position.x == 3)
            position_String += "c";
        else if (position.x == 4)
            position_String += "d";
        else if (position.x == 5)
            position_String += "e";
        else if (position.x == 6)
            position_String += "f";
        else if (position.x == 7)
            position_String += "g";
        else if (position.x == 8)
            position_String += "h";

        position_String += position.y;

        return position_String;
    }
    Vector2 Convert_Text_Num(string position_String)
    {
        Vector2 position = new Vector2(1, 1);

        if (position_String[0] == 'a')
            position.x = 1;
        else if (position_String[0] == 'b')
            position.x = 2;
        else if (position_String[0] == 'c')
            position.x = 3;
        else if (position_String[0] == 'd')
            position.x = 4;
        else if (position_String[0] == 'e')
            position.x = 5;
        else if (position_String[0] == 'f')
            position.x = 6;
        else if (position_String[0] == 'g')
            position.x = 7;
        else if (position_String[0] == 'h')
            position.x = 8;

        if (position_String[1] == '1')
            position.y = 1;
        else if (position_String[1] == '2')
            position.y = 2;
        else if (position_String[1] == '3')
            position.y = 3;
        else if (position_String[1] == '4')
            position.y = 4;
        else if (position_String[1] == '5')
            position.y = 5;
        else if (position_String[1] == '6')
            position.y = 6;
        else if (position_String[1] == '7')
            position.y = 7;
        else if (position_String[1] == '8')
            position.y = 8;

        return position;
    }
    Chess_piece Convert_PieceNameto_Object(string piece_Name)
    {
        float x = (float)Char.GetNumericValue(piece_Name[2]);
        float y = (float)Char.GetNumericValue(piece_Name[3]);

        Vector2 position = new Vector2(x, y);
        Chess_piece piece = GetPieceAt(position);
        return piece;
    }
    string Convert_Object_ToPieceName(Chess_piece piece)
    {
        string piece_Name = "";

        if(piece.color == Piece_Color.White)
        {
            piece_Name += "w";
        }
        else
        {
            piece_Name += "b";
        }

        if (piece.piece_type == Chess_Piece_Type.King)
        {
            piece_Name += "K";
        }
        else if(piece.piece_type == Chess_Piece_Type.Queen)
        {
            piece_Name += "Q";
        }
        else if (piece.piece_type == Chess_Piece_Type.Rock)
        {
            piece_Name += "R";
        }
        else if (piece.piece_type == Chess_Piece_Type.Bishop)
        {
            piece_Name += "B";
        }
        else if (piece.piece_type == Chess_Piece_Type.Knight)
        {
            piece_Name += "N";
        }
        else if (piece.piece_type == Chess_Piece_Type.Pawn)
        {
            piece_Name += "P";
        }

        piece_Name += piece.currentposition.x.ToString() + piece.currentposition.y.ToString();

        return piece_Name;
    }
    Chess_piece GetPieceAt(Vector2 position)
    {
        List<Chess_piece> allPieces = new List<Chess_piece>();

        foreach (Chess_piece piece in White_Pieces)
            allPieces.Add(piece);
        foreach (Chess_piece piece in Black_Pieces)
            allPieces.Add(piece);

        foreach (Chess_piece piece in allPieces)
            if (piece.currentposition == position)
                return piece;

        return null;
    }
    List<Vector2> LegalMoves(Chess_piece piece, bool master, bool check_KingChecks, bool CheckCastleTile)
    {
        //Game Rules logic To be replaced
        float board_Dir = master ? 1f : -1f;

        //Pieces Basic moves
        List<Vector2> moves = new List<Vector2>();
        if (piece.piece_type == Chess_Piece_Type.King)
        {
            moves.Add(piece.currentposition + new Vector2(1, 1));
            moves.Add(piece.currentposition + new Vector2(-1, 1));
            moves.Add(piece.currentposition + new Vector2(1, -1));
            moves.Add(piece.currentposition + new Vector2(-1, -1));
            moves.Add(piece.currentposition + new Vector2(0, 1));
            moves.Add(piece.currentposition + new Vector2(0, -1));
            moves.Add(piece.currentposition + new Vector2(1, 0));
            moves.Add(piece.currentposition + new Vector2(-1, 0));

            if (CheckCastleTile)
            {
                foreach (Vector2 move in GetCastle(piece, master))
                    moves.Add(move);
            }
        }
        else if (piece.piece_type == Chess_Piece_Type.Pawn)
        {
            //if enemy exists pawn can move up right or up left.
            bool EnemyInFront = false;
            bool EnemyInFront2 = false;
            bool EnPassent_PawnOnSide = false;
            Vector2 EnPassent_Tile = new Vector2();

            List<Chess_piece> enemies = master ? Black_Pieces : White_Pieces;

            foreach (Chess_piece enemy in enemies)
            {
                bool enemy_OnLeft = (enemy.currentposition == (piece.currentposition + new Vector2(1, board_Dir * 1)));
                bool enemy_OnRight = (enemy.currentposition == (piece.currentposition + new Vector2(-1, board_Dir * 1)));
                bool enemy_InFront = (enemy.currentposition == (piece.currentposition + new Vector2(0, board_Dir * 1)));
                bool enemy_InFront2 = (enemy.currentposition == (piece.currentposition + new Vector2(0, board_Dir * 2)));

                bool enemy_onSideLeft = (enemy.currentposition == piece.currentposition + new Vector2(1, 0));
                bool enemy_onSideRight = (enemy.currentposition == piece.currentposition + new Vector2(-1, 0));

                if (enemy_OnLeft || enemy_OnRight)
                {
                    moves.Add(enemy.currentposition);
                }

                if (enemy.piece_type == Chess_Piece_Type.Pawn && (enemy_onSideRight || enemy_onSideLeft) && enemy.EnPassent_Possible)
                {
                    EnPassent_PawnOnSide = true;
                    EnPassent_Tile = enemy.currentposition + new Vector2(0, board_Dir * 1);
                }

                if (enemy_InFront)
                    EnemyInFront = true;

                if (enemy_InFront2)
                    EnemyInFront2 = true;
            }

            // if there's no enemies front, add regular movement
            if (!EnemyInFront && !EnemyInFront2 && piece.firstMove)
            {
                moves.Add(piece.currentposition + new Vector2(0, board_Dir * 1));
                moves.Add(piece.currentposition + new Vector2(0, board_Dir * 2));
            }
            else if (!EnemyInFront)
            {
                moves.Add(piece.currentposition + new Vector2(0, board_Dir * 1));
            }

            if (EnPassent_PawnOnSide)
                moves.Add(EnPassent_Tile);


        }
        else if (piece.piece_type == Chess_Piece_Type.Knight)
        {
            moves.Add(piece.currentposition + new Vector2(1, 2));
            moves.Add(piece.currentposition + new Vector2(-1, 2));
            moves.Add(piece.currentposition + new Vector2(2, 1));
            moves.Add(piece.currentposition + new Vector2(-2, 1));
            moves.Add(piece.currentposition + new Vector2(1, -2));
            moves.Add(piece.currentposition + new Vector2(-1, -2));
            moves.Add(piece.currentposition + new Vector2(2, -1));
            moves.Add(piece.currentposition + new Vector2(-2, -1));
        }
        else if (piece.piece_type == Chess_Piece_Type.Rock)
        {
            for (int i = 1; i <= 8; i++)
            {
                moves.Add(piece.currentposition + new Vector2(0, i));
                moves.Add(piece.currentposition + new Vector2(0, -i));
                moves.Add(piece.currentposition + new Vector2(i, 0));
                moves.Add(piece.currentposition + new Vector2(-i, 0));
            }
        }
        else if (piece.piece_type == Chess_Piece_Type.Bishop)
        {
            for (int i = 1; i <= 8; i++)
            {
                moves.Add(piece.currentposition + new Vector2(i, i));
                moves.Add(piece.currentposition + new Vector2(i, -i));
                moves.Add(piece.currentposition + new Vector2(-i, i));
                moves.Add(piece.currentposition + new Vector2(-i, -i));
            }
        }
        else if (piece.piece_type == Chess_Piece_Type.Queen)
        {
            for (int i = 1; i <= 8; i++)
            {
                moves.Add(piece.currentposition + new Vector2(0, i));
                moves.Add(piece.currentposition + new Vector2(0, -i));
                moves.Add(piece.currentposition + new Vector2(i, 0));
                moves.Add(piece.currentposition + new Vector2(-i, 0));

                moves.Add(piece.currentposition + new Vector2(i, i));
                moves.Add(piece.currentposition + new Vector2(i, -i));
                moves.Add(piece.currentposition + new Vector2(-i, i));
                moves.Add(piece.currentposition + new Vector2(-i, -i));
            }
        }




        //////removing tiles out of board bounds
        List<Vector2> modified_Moves = new List<Vector2>();
        foreach (Vector2 move in moves)
            if (!(move.y > 8 || move.y < 1 || move.x > 8 || move.x < 1))
                modified_Moves.Add(move);


        /////removing tiles where other friendly exists
        moves.Clear(); //instead of creating a new list
        foreach (Vector2 move in modified_Moves)
            moves.Add(move);

        List<Chess_piece> allpieces = new List<Chess_piece>();
        foreach (Chess_piece p in White_Pieces)
            allpieces.Add(p);
        foreach (Chess_piece p in Black_Pieces)
            allpieces.Add(p);

        foreach (Vector2 move in modified_Moves)
        {
            foreach (Chess_piece p in allpieces)
            {
                if (p.currentposition == move)
                {
                    if (piece.piece_type == Chess_Piece_Type.Pawn)
                    {
                        float direction = p.currentposition.y - piece.currentposition.y;

                        if ((direction == (board_Dir * 1)) && piece.firstMove)
                        {
                            moves.Remove(piece.currentposition + new Vector2(0, board_Dir * 2));
                        }
                    }
                    else if (piece.piece_type == Chess_Piece_Type.Rock)
                    {
                        Vector2 direction = p.currentposition - piece.currentposition;

                        if (direction.y >= 1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.y > p.currentposition.y && m.x == p.currentposition.x)
                                    moves.Remove(m);
                            }
                        }
                        else if (direction.y <= -1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.y < p.currentposition.y && m.x == p.currentposition.x)
                                    moves.Remove(m);
                            }
                        }
                        else if (direction.x >= 1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.x > p.currentposition.x && m.y == p.currentposition.y)
                                    moves.Remove(m);
                            }
                        }
                        else if (direction.x <= -1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.x < p.currentposition.x && m.y == p.currentposition.y)
                                    moves.Remove(m);
                            }
                        }
                    }
                    else if (piece.piece_type == Chess_Piece_Type.Bishop)
                    {
                        Vector2 direction = p.currentposition - piece.currentposition;

                        if (direction.y >= 1 && direction.x >= 1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.y > p.currentposition.y && m.x > p.currentposition.x)
                                    moves.Remove(m);
                            }
                        }
                        else if (direction.y >= 1 && direction.x <= -1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.y > p.currentposition.y && m.x < p.currentposition.x)
                                    moves.Remove(m);
                            }
                        }
                        else if (direction.y <= -1 && direction.x >= 1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.y < p.currentposition.y && m.x > p.currentposition.x)
                                    moves.Remove(m);
                            }
                        }
                        else if (direction.y <= -1 && direction.x <= -1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.y < p.currentposition.y && m.x < p.currentposition.x)
                                    moves.Remove(m);
                            }
                        }
                    }
                    else if (piece.piece_type == Chess_Piece_Type.Queen)
                    {
                        Vector2 direction = p.currentposition - piece.currentposition;

                        //replicate rook
                        if (direction.y >= 1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.y > p.currentposition.y && m.x == p.currentposition.x)
                                    moves.Remove(m);
                            }
                        }
                        else if (direction.y <= -1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.y < p.currentposition.y && m.x == p.currentposition.x)
                                    moves.Remove(m);
                            }
                        }
                        else if (direction.x >= 1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.x > p.currentposition.x && m.y == p.currentposition.y)
                                    moves.Remove(m);
                            }
                        }
                        else if (direction.x <= -1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.x < p.currentposition.x && m.y == p.currentposition.y)
                                    moves.Remove(m);
                            }
                        }


                        //replicate bishop
                        if (direction.y >= 1 && direction.x >= 1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.y > p.currentposition.y && m.x > p.currentposition.x)
                                    moves.Remove(m);
                            }
                        }
                        else if (direction.y >= 1 && direction.x <= -1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.y > p.currentposition.y && m.x < p.currentposition.x)
                                    moves.Remove(m);
                            }
                        }
                        else if (direction.y <= -1 && direction.x >= 1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.y < p.currentposition.y && m.x > p.currentposition.x)
                                    moves.Remove(m);
                            }
                        }
                        else if (direction.y <= -1 && direction.x <= -1)
                        {
                            foreach (Vector2 m in modified_Moves)
                            {
                                if (m.y < p.currentposition.y && m.x < p.currentposition.x)
                                    moves.Remove(m);
                            }
                        }


                    }

                    //remove tile where other pieces exist when they are friendly.
                    bool mypiece = ((master) && (p.color == Piece_Color.White)) || ((!master) && (p.color == Piece_Color.Black));

                    if (mypiece)
                        moves.Remove(move);
                }
            }
        }




        //remove moves which leads to the king being checked.
        modified_Moves.Clear(); //instead of creating a new list
        foreach (Vector2 move in moves)
            modified_Moves.Add(move);

        if (check_KingChecks)
        {


            Vector2 my_Original_Position = piece.currentposition;

            foreach (Vector2 move in moves)
            {
                if ((piece.piece_type == Chess_Piece_Type.Bishop) && (piece.color == Piece_Color.Black) && !master && check_KingChecks && (move == new Vector2(5,5)))
                    Debug.Log("Break here");

                piece.currentposition = move;

                //find if enemy exists in this move
                Chess_piece Enemy = new Chess_piece();
                Vector2 enemy_original_position = new Vector2();
                List<Chess_piece> enemies = master ? Black_Pieces : White_Pieces;
                foreach (Chess_piece enemy in enemies)
                {
                    if (enemy.currentposition == move)
                    {
                        Enemy = enemy;
                        enemy_original_position = enemy.currentposition;
                    }
                }

                //if enemy exists remove it temporarly
                if (Enemy)
                {
                    Enemy.currentposition = new Vector2(91, 91);
                }

                //if king is still checked, remove the move
                if (IsAKingChecked(master))
                {
                    modified_Moves.Remove(move);
                }

                //enemy back to it's posisiton
                Enemy.currentposition = enemy_original_position;
            }

            piece.currentposition = my_Original_Position;
        }

        return modified_Moves;
    }
    int GetPieceValue(Chess_piece piece)
    {
        int value = 0;

        if (piece.piece_type == Chess_Piece_Type.Queen)
            value = 9;
        else if (piece.piece_type == Chess_Piece_Type.Rock)
            value = 5;
        else if (piece.piece_type == Chess_Piece_Type.Bishop)
            value = 3;
        else if (piece.piece_type == Chess_Piece_Type.Knight)
            value = 3;
        else if (piece.piece_type == Chess_Piece_Type.Pawn)
            value = 1;

        return value;
    }
    string Min_Sec(float seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(seconds);

        return t.Minutes + ":" + t.Seconds;
    }
    string ComputeMoveFEN(Chess_piece piece, string enemy, string NewPosition)
    {
        string MoveLog = "";
        if (piece.piece_type != Chess_Piece_Type.Pawn)
        {
            MoveLog += Convert_Object_ToPieceName(piece)[1];
        }
        if (enemy != "")
        {
            if (piece.piece_type == Chess_Piece_Type.Pawn)
            {
                MoveLog += Convert_Num_Text(piece.currentposition)[0];
            }

            MoveLog += "x";
        }
        MoveLog += NewPosition;

        return MoveLog;
    }
}

[System.Serializable]
public class Tile_Row
{
    [SerializeField] string name;
    [SerializeField] public List<GameObject> tile_row = new List<GameObject>();
}
public class Move_Score
{
    public Chess_piece piece;
    public Vector2 move;
    public float gain;
    public float loss;
    public string log;
    public string Log2;
    
    public Move_Score()
    {

    }
}
