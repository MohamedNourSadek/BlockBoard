using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Winner { Master, Guest, Draw, NoWinner }
public enum MyWinState { Won, Draw, Lost }

public class DominoController : MonoBehaviour
{
    public static DominoController Instance;

    [Header("Parameters")]
    [SerializeField] int CardsPerPlayer = 7;
    
    
    private List<DominoTile> tilesOutside = new List<DominoTile>();
    private PhotonView view;
    public MyWinState myWinState = MyWinState.Lost;


    #region Constructs
    public DominoController()
    {
        Instance = this;
    }
    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }
    #endregion

    #region Public Functions
    public void StartGame()
    {
        TurnDominoSystemsOn();

        if (GameManager.IsMasterClient)
            StartDominoGame();
    }
    public List<DominoTile> GetTilesOutSide()
    {
        return tilesOutside;
    }
    public void EndRound(Winner roundWinner, bool rematch, bool bySubmission)
    {
        DominoPlayer.Instance.GameIsOn = false;

        if (roundWinner == Winner.Master)
        {
            Manager.GameManager.CurrentScore[0] = DominoPlayer.Instance.CalculateScore(DominoPlayer.Instance.GuestCards);
            Manager.GameManager.MasterWonLastGame = true;
        }
        else if (roundWinner == Winner.Guest)
        {
            Manager.GameManager.CurrentScore[1] = DominoPlayer.Instance.CalculateScore(DominoPlayer.Instance.MasterCards);
            Manager.GameManager.MasterWonLastGame = false;
        }

        Winner matchWinner = Winner.Draw;

        if (Manager.GameManager.CurrentScore[0] >= Manager.GameManager.DominoSettings.DominoWinScore)
            matchWinner = Winner.Master;
        else if (Manager.GameManager.CurrentScore[1] >= Manager.GameManager.DominoSettings.DominoWinScore)
            matchWinner = Winner.Guest;
        else
            matchWinner = Winner.NoWinner;

        DominoPlayer.Instance.MyScore.text = GameManager.IsMasterClient ? Manager.GameManager.CurrentScore[0].ToString() : Manager.GameManager.CurrentScore[1].ToString();
        DominoPlayer.Instance.OtherScore.text = GameManager.IsMasterClient ? Manager.GameManager.CurrentScore[1].ToString() : Manager.GameManager.CurrentScore[0].ToString();

        string Message = "";

        if (bySubmission)
        {
            Message = General_InGameUI.Instance.SubmissionText;
        }


        if ((matchWinner == Winner.Master && GameManager.IsMasterClient) || (matchWinner == Winner.Guest && !GameManager.IsMasterClient))
        {
            Message += General_InGameUI.Instance.WinText;
            myWinState = MyWinState.Won;
        }
        else if ((matchWinner == Winner.Guest && GameManager.IsMasterClient) || (matchWinner == Winner.Master && !GameManager.IsMasterClient))
        {
            Message += General_InGameUI.Instance.LoseText;
            myWinState = MyWinState.Lost;
        }
        else if (matchWinner == Winner.NoWinner)
        {
            if ((roundWinner == Winner.Master && GameManager.IsMasterClient) || (roundWinner == Winner.Guest && !GameManager.IsMasterClient))
            {
                Message = "You've won this round!";

            }
            else if ((roundWinner == Winner.Guest && GameManager.IsMasterClient) || (roundWinner == Winner.Master && !GameManager.IsMasterClient))
            {
                Message = "You've Lost this round!";
            }
            else if (roundWinner == Winner.Draw)
            {
                Message = "Draw!";
            }

            Panel.GetPanel<PopUpPanel>().ShowPopUp(Message);

            DominoPlayer.Instance.GameIsOn = false;
            StartCoroutine(NextRound());
            return;
        }

        if (!(Manager.GameManager.GameMode == GameMode.Offline) && (Manager.GameManager.CurrentGame == GameType.Domino))
        {
            //if(!(myWinState == MyWinState.Lost))
                //Handle_Data(Data_Result);
        }

        General_InGameUI.Instance.EndGame_Message.text = Message;
        General_InGameUI.Instance.EndGame_Menu.SetActive(true);

        General_InGameUI.Instance.Game_UI.SetActive(false);
        Manager.GameManager.CurrentScore[0] = 0;
        Manager.GameManager.CurrentScore[1] = 0;
        Manager.GameManager.MasterWonLastGame = true;
    }
    #endregion


    #region Private Functions   
    private void StartDominoGame()
    {
        tilesOutside = GameManager.CopyList(DominoGeometery.Instance.DominoCards);

        string randomCardsSeqence = GenerateRandomSequence(tilesOutside.Count);

        if (Manager.GameManager.GameMode == GameMode.Offline)
            SendSequenceToClients(randomCardsSeqence);
        else
            view.RPC("SendSequenceToClients", RpcTarget.All, randomCardsSeqence);
    }
    private void TurnDominoSystemsOn()
    {
        DominoGeometery.Instance.gameObject.SetActive(true);
        Panel.GetPanel<DominoPanel>().Show();
        gameObject.SetActive(true);
    }
    private void DistributeCards()
    {
        for (int i = 0; i < CardsPerPlayer; i++)
        {
            DominoPlayer.Instance.MasterCards.Add(tilesOutside[0]);
            tilesOutside.Remove(tilesOutside[0]);
        }

        for (int i = 0; i < CardsPerPlayer; i++)
        {
            DominoPlayer.Instance.GuestCards.Add(tilesOutside[0]);
            tilesOutside.Remove(tilesOutside[0]);
        }

        DominoGeometery.Instance.OrganizeCardsOutside(tilesOutside);
        DominoPlayer.Instance.ReOrganizeCardsInHands();
    }
    private string GenerateRandomSequence(int count)
    {
        List<int> randomSequence = new List<int>();

        string randomSequenceString = "";

        for (int i = 0; i < count; i++)
        {
            bool exists = true;
            int num = 0;

            while (exists)
            {
                num = UnityEngine.Random.Range(0, count);
                exists = randomSequence.Exists(e => e.Equals(num));
            }

            randomSequence.Add(num);
        }

        foreach (int i in randomSequence)
            randomSequenceString += i.ToString() + ",";

        return randomSequenceString;
    }
    private IEnumerator NextRound()
    {
        yield return new WaitForSecondsRealtime(2f);

        if (GameManager.IsMasterClient)
            General_InGameUI.Instance.Rematch();
    }
    #endregion


    #region RPCs
    [PunRPC]
    private void SendSequenceToClients(string randomCardsSeqence)
    {
        var sequenceList = randomCardsSeqence.Split(',');

        List<DominoTile> newCardsOrganization = new List<DominoTile>();

        for (int i = 0; i < sequenceList.Length - 1; i++)
            newCardsOrganization.Add(tilesOutside[System.Convert.ToInt32(sequenceList[i])]);

        tilesOutside = newCardsOrganization;

        DistributeCards();
    }
    #endregion

}
