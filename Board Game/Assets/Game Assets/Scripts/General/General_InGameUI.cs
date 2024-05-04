using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class General_InGameUI : MonoBehaviour
{
    [SerializeField] GameObject Menu;
    [SerializeField] GameObject Menu_Layer1;
    [SerializeField] GameObject Settings;
    [SerializeField] public GameObject Game_UI;
    [SerializeField] GameObject Messages_Icon;
    [SerializeField] GameObject Tutorial_menu;
    [SerializeField] List<string> Domino_Rules = new List<string>();
    [SerializeField] List<string> Chess_Rules = new List<string>();
    [SerializeField] List<string> Poker_Rules = new List<string>();
    [SerializeField] Text ruleHeader;
    [SerializeField] Text ruleText;
    [SerializeField] Button LeftSlider;
    [SerializeField] Button RightSlider;
    [SerializeField] public GameObject EndGame_Menu;
    [SerializeField] public GameObject EndRound_Menu;
    [SerializeField] public Text EndGame_Message;
    [SerializeField] public Text EndRound_Message;
    [SerializeField] public string  SubmissionText = "Other Player left \n \n";
    [SerializeField] public string  WinText = "You've Won!";
    [SerializeField] public string  LoseText = "You've Lost!";
    [SerializeField] public Button Camera_Snap;
    [SerializeField] public Sprite Camera_Snap_On;
    [SerializeField] public Sprite Camera_Snap_Off;
    [SerializeField] public Menu_settings settings;

    //internal 
    PhotonView view;
    List<string> Rules = new List<string>();


    private void Awake()
    {
        if (Cross_Scene_Data.AI)
            Messages_Icon.SetActive(false);
        else
            view = GetComponent<PhotonView>();

        if (Manager.GameManager.CurrentGame == GameType.Domino)
            Rules = Domino_Rules;
        else if (Manager.GameManager.CurrentGame == GameType.Chess)
            Rules = Chess_Rules;
        else if (Manager.GameManager.CurrentGame == GameType.Poker)
            Rules = Poker_Rules;

        if(Menu_settings.Get_Camera_Snaaping() == 0)
            Camera_Snap.image.sprite = Camera_Snap_Off;
        else
            Camera_Snap.image.sprite = Camera_Snap_On;

        Cross_Scene_Data.Camera_Snap_Event.AddListener(OnCamera_Snap_Event);
    }


    public void Toggle_Camera_Snapp()
    {
        if (!settings.initialized)
            settings.Start();

        if (Cross_Scene_Data.Camera_Snap)
            settings.Camer_Snap_Toggle.isOn = false;
        else
            settings.Camer_Snap_Toggle.isOn = true;
    }


    public void ShowMenu(bool state)
    {
        Menu.SetActive(state);
        Menu_Layer1.SetActive(state);
        Game_UI.SetActive(!state);

        if (Cross_Scene_Data.AI)
        {
            if (state)
                Time.timeScale = 0f;
            else
                Time.timeScale = 1f;
        }
    }
    public void ShowSettings()
    {
        Settings.SetActive(true);
        Menu_Layer1.SetActive(false);
    }
    public void BackTo_Menu()
    {
        Settings.SetActive(false);
        Menu_Layer1.SetActive(true);
        Menu.SetActive(true);
    }

    public void leaveGame()
    {
        Cross_Scene_Data.Current_Guest_score = 0;
        Cross_Scene_Data.Current_Master_score = 0;
        Cross_Scene_Data.UseNewMaxScore = false;
        Cross_Scene_Data.In_Domino_Game = false;
        Cross_Scene_Data.In_Chess_Game = false;
        Cross_Scene_Data.In_Poker_Game = false;

        if (Cross_Scene_Data.AI)
        {
            SceneManager.LoadScene("Lobby_Scene");
        }
        else
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel("Lobby_Scene");
        }
    }

    public void Show_tutorial(bool state)
    {
        Tutorial_menu.SetActive(state);
        Game_UI.SetActive(!state);
        Slide_rules(0);

        if (Cross_Scene_Data.AI)
        {
            if (state)
                Time.timeScale = 0f;
            else
                Time.timeScale = 1f;
        }
    }

    int currentRule = 0;
    public void Slide_rules(int i)
    {
        if ((currentRule > 0) || (currentRule < Rules.Count - 1))
        {
            currentRule += i;
            ruleText.text = Rules[currentRule];
            ruleHeader.text = "Rule " + (currentRule + 1);
        }

        FixSelected();
    }
    void FixSelected()
    {
        if (currentRule < 0)
            currentRule = 0;
        else if (currentRule > Rules.Count - 1)
            currentRule = Rules.Count - 1;

        if (currentRule == 0)
            LeftSlider.interactable = false;
        else if (currentRule == Rules.Count - 1)
            RightSlider.interactable = false;
        else
        {
            LeftSlider.interactable = true;
            RightSlider.interactable = true;
        }
    }

    public void Rematch()
    {
        if (Cross_Scene_Data.AI)
            Rematch_Sync();

        else
            view.RPC("Rematch_Sync", RpcTarget.AllBuffered);
    }
    [PunRPC] void Rematch_Sync()
    {
        if (!Cross_Scene_Data.AI)
        {
            PhotonNetwork.LoadLevel("Game");
        }
        else
        {
            SceneManager.LoadScene("Game");
        }

        Cross_Scene_Data.In_Chess_Game = false;
        Cross_Scene_Data.In_Domino_Game = false;
        Cross_Scene_Data.In_Poker_Game = false;
    }



    void OnCamera_Snap_Event(bool state)
    {
        if (state)
            Camera_Snap.image.sprite = Camera_Snap_On;
        else
            Camera_Snap.image.sprite = Camera_Snap_Off;
    }
}
