using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRoomItem : Panel
{
    public Image PlayerHighlight;
    public TMP_Text PlayerName;

    public void SetPlayerInfo(Photon.Realtime.Player playerInfo)
    {
        PlayerName.text = playerInfo.NickName;
        bool isReady = (((string)playerInfo.CustomProperties[PhotonManager.PlayerReadyKey]) == "true");
        
        PlayerHighlight.gameObject.SetActive(isReady);
    }

}
