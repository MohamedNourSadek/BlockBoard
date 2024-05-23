using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyPanel : Panel
{
    public Button JoinButton;
    public Button LeaveButton;
    public TMP_Text RoomErrorText;
    public TMP_InputField RoomInput;


    public override void Awake()
    {
        base.Awake();
        JoinButton.onClick.AddListener(OnJoinPressed);
        LeaveButton.onClick.AddListener(OnLeavePressed);
        RoomInput.onValueChanged.AddListener(OnRoomInputChanged);
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        OnRoomInputChanged(RoomInput.text);
    }

    private void OnLeavePressed()
    {
        Hide();
        Panel.GetPanel<OnlineModesPanel>().Show();
    }

    private void OnJoinPressed()
    {
        throw new NotImplementedException();
    }
    
    private void OnRoomInputChanged(string roomName)
    {
        var roomNameCorrection = InputCorrectionManager.GetCorrectionMessage(roomName, InputType.RoomName);

        RoomErrorText.text = roomNameCorrection.CorrectionMessage;
        JoinButton.interactable = roomNameCorrection.IsInputCorrect;
    }
}
