using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.Audio;



public class Messages_Manager : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [SerializeField] GameObject content_obj;
    [SerializeField] GameObject Message_Prefab;
    [SerializeField] InputField input;
    [SerializeField] Text missed_messages_text;
    [SerializeField] public Color myColor;
    [SerializeField] public Color OtherColor;
    [SerializeField] AudioClip message_clip;
    public List<my_Message> messages = new List<my_Message>();
    [SerializeField] Vector2 initialPosition;
    [SerializeField] AudioMixer Fx_Mixer;


    PhotonView view;

    public bool State = false;
    public int Missed_Messages = 0;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    //Ui Keys
    public void SwitchMenu()
    {
        State = !State;
        menu.SetActive(State);

        if (State)
        {
            Missed_Messages = 0;
            Update_Messages_MissedUI();
        }
    }

    public void send_message()
    {
        if (input.text.Length > 0)
        {
            int me = PhotonNetwork.LocalPlayer.ActorNumber;
            SendMessage(me, input.text);
            view.RPC("SendMessage", RpcTarget.OthersBuffered, me, input.text);
            input.text = "";
        }
    }

    public void Logg_Message(string message)
    {
        SendMessage(PhotonNetwork.LocalPlayer.ActorNumber, message);
        view.RPC("SendMessage", RpcTarget.OthersBuffered, PhotonNetwork.LocalPlayer.ActorNumber, message);
    }

    [PunRPC] public void SendMessage(int player_actorNum, string message)
    {
        string PlayerName = "P";
        Color PlayerColor = OtherColor;

        int i = 1;

        foreach(var player in Manager.GameManager.Players)
        {
            if(player.Value == player_actorNum)
                PlayerName = "P" + i;

            if (PhotonNetwork.LocalPlayer.ActorNumber == player_actorNum)
                PlayerColor = myColor;

            i++;
        }

        Add_message(PlayerName, message, PlayerColor);

        if(State == false)
        {
            Missed_Messages += 1;

            float Volume;
            Fx_Mixer.GetFloat("Volume", out Volume);
            float MappedVolume = AdditionalMath.Remap(Volume, new Vector2(-30f, 0f), new Vector2(0f, 1f));

            AudioSource.PlayClipAtPoint(message_clip, Camera.main.transform.position, MappedVolume);
            Update_Messages_MissedUI();
        }
    }
    public void Add_message(string PlayerName, string Message, Color color)
    {
        my_Message g = Instantiate(Message_Prefab,content_obj.transform).GetComponent<my_Message>();
        g.PrintMessage(PlayerName, Message, color);
        messages.Add(g);
        ReOrganize_Messages();
    }

    void Update_Messages_MissedUI()
    {
        if (missed_messages_text)
        {
            missed_messages_text.text = Missed_Messages.ToString();

            if (Missed_Messages > 0)
                missed_messages_text.color = Color.red;
            else
                missed_messages_text.color = Color.white;
        }
    }

    void ReOrganize_Messages()
    {
        for(int i = messages.Count - 1; i >= 0; i--)
        {
            messages[i].transform.localPosition = new Vector3(initialPosition.x, -initialPosition.y + ((i - (messages.Count - 1)) * 100f) , 0f);
        }
    }
}
