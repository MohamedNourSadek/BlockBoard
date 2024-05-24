using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Poker_MatchSettings : MonoBehaviour
{
    [SerializeField] Slider Bet_slider;
    [SerializeField] Text Bet_Text;

    public static string bet_string = "bet_amount";


    void Awake()
    {       
        Bet_slider.value = GetBet();
        Bet_Text.text = GetBet().ToString();
        Bet_slider.onValueChanged.AddListener(On_BetChange);
    }

    //Helping Functions
    public void On_BetChange(float value)
    {
        SetBet(value);
        Bet_Text.text = value.ToString();
    }
    public static float GetBet()
    {
        float bet_amount = PlayerPrefs.GetFloat(bet_string);

        if (bet_amount == 0f)
            return 100f;
        else
            return bet_amount;
    }
    void SetBet(float Value)
    {
        PlayerPrefs.SetFloat(bet_string, Value);
    }
}
