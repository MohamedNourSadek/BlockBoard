using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameGeneralPanel : MonoBehaviour
{
    [SerializeField] GameObject Menu;
    [SerializeField] GameObject Menu_Layer1;
    [SerializeField] GameObject Settings;
    [SerializeField] public GameObject Game_UI;
    [SerializeField] GameObject Messages_Icon;
    [SerializeField] GameObject Tutorial_menu;
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
    public static GameGeneralPanel Instance;

    private void Start()
    {
        Instance = this;

        if ((Manager.GameManager.GameMode == GameMode.Offline))
            Messages_Icon.SetActive(false);
        else
            view = GetComponent<PhotonView>();

        if(Manager.GetManager<SettingsManager>().CameraSnap)
            Camera_Snap.image.sprite = Camera_Snap_Off; 
        else
            Camera_Snap.image.sprite = Camera_Snap_On;

        Manager.SettingsManager.OnCameraSnapChanged += OnCamera_Snap_Event;
    }


    public void Toggle_Camera_Snapp()
    {
        var settingsManager = Manager.GetManager<SettingsManager>();
        settingsManager.ChangeCameraSnap(!settingsManager.CameraSnap);
    }


    public void ShowMenu(bool state)
    {
        Menu.SetActive(state);
        Menu_Layer1.SetActive(state);
        Game_UI.SetActive(!state);

        if ((Manager.GameManager.GameMode == GameMode.Offline))
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
        Manager.GameManager.CurrentScore[0] = 0;
        Manager.GameManager.CurrentScore[1] = 0;

        Manager.GameManager.CurrentGame = GameType.None;
        
        if ((Manager.GameManager.GameMode == GameMode.Offline))
        {
            SceneManager.LoadScene("Lobby_Scene");
        }
        else
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel("Lobby_Scene");
        }
    }


    public void Rematch()
    {
        if ((Manager.GameManager.GameMode == GameMode.Offline))
            Rematch_Sync();

        else
            view.RPC("Rematch_Sync", RpcTarget.AllBuffered);
    }
    [PunRPC] void Rematch_Sync()
    {
        if (!(Manager.GameManager.GameMode == GameMode.Offline))
        {
            PhotonNetwork.LoadLevel("Game");
        }
        else
        {
            SceneManager.LoadScene("Game");
        }

        Manager.GameManager.CurrentGame = GameType.None;
    }



    void OnCamera_Snap_Event(bool state)
    {
        if (state)
            Camera_Snap.image.sprite = Camera_Snap_On;
        else
            Camera_Snap.image.sprite = Camera_Snap_Off;
    }
}
