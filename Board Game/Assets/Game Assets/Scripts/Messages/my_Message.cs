using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class my_Message : MonoBehaviour
{
    [SerializeField] Text playerName;
    [SerializeField] Text message;
    
    public void PrintMessage(string PlayerName, string Message, Color color)
    {
        playerName.text = PlayerName;
        playerName.color = color;
        message.text = Message;
    }
}
