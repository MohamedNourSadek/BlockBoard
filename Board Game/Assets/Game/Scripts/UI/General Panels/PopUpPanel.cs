using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopUpPanel : Panel
{
    public TextMeshProUGUI Message;

    public void ShowPopUp(string message)
    {
        Show();
        Message.text = message;
    }
}
