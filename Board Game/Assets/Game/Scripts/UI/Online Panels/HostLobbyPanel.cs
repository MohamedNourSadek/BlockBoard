using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostLobbyPanel : Panel
{
    public TMP_InputField RoomNameInput;
    public TMP_Text RoomErrorText;
    public Button CreateRoomButton;
    public Button BackButton;

    public Dictionary<GameType, Panel> GameSettings = new Dictionary<GameType, Panel>();


    public override void Awake()
    {
        base.Awake();

        RoomNameInput.onValueChanged.AddListener(OnRoomNameChanged);
        CreateRoomButton.onClick.AddListener(OnCreateRoomPressed);
        BackButton.onClick.AddListener(OnBackPressed);
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

        foreach (GameType game in GameSettings.Keys)
        {
            if (game == GameType.None)
                continue;

            if (game == Manager.GameManager.CurrentGame)
                GameSettings[game].Show();
            else
                GameSettings[game].Hide();
        }

        OnRoomNameChanged(RoomNameInput.text);
    }

    private void OnRoomNameChanged(string roomName)
    {
        var roomCorrect = InputCorrectionManager.GetCorrectionMessage(roomName, InputType.RoomName);

        RoomErrorText.text = roomCorrect.CorrectionMessage;
        CreateRoomButton.interactable = roomCorrect.IsInputCorrect;
    }

    private void OnCreateRoomPressed()
    {
        Hide();
        Panel.GetPanel<WaitingPanel>().Show(null, false);

        PhotonManager.Instance.OnRoomJoinCallback += OnRoomJoinCallback;
        PhotonManager.Instance.CreateRoom(RoomNameInput.text);
    }

    private void OnRoomJoinCallback(bool state, object result)
    {
        PhotonManager.Instance.OnRoomJoinCallback -= OnRoomJoinCallback;
        Panel.GetPanel<WaitingPanel>().Hide();

        if (state)
        {
            Debug.Log("Show Room");
        }
        else
        {
            Panel.GetPanel<MessagePanel>().Show("Error", ((string)result) , ButtonTypes.Ok, ButtonTypes.None, () =>
            {
                Show();
            });
        }
    }


    private void OnBackPressed()
    {
        Hide();
        Panel.GetPanel<OnlineModesPanel>().Show();
    }
}
