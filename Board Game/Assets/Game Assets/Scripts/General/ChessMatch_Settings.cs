using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChessMatch_Settings : MonoBehaviour
{
    [SerializeField] Slider Time_slider;
    [SerializeField] Text time_text;
    [SerializeField] Slider bonus_slider;
    [SerializeField] Text bonus_text;
    [SerializeField] Slider Difficulty;
    [SerializeField] Text Difficulty_text;

    public static string time_String = "chess_time";
    public static string bonus_String = "chess_bonus";
    public static string diff_string = "difficulty_bonus";

    private void Awake()
    {
        Time_slider.value = Manager.GameManager.ChessSettings.ChessTime;
        time_text.text = Manager.GameManager.ChessSettings.ChessTime.ToString();
        Time_slider.onValueChanged.AddListener(onTime_Change);

        bonus_slider.value = Manager.GameManager.ChessSettings.ChessBonusTime;
        bonus_text.text = Manager.GameManager.ChessSettings.ChessBonusTime.ToString();
        bonus_slider.onValueChanged.AddListener(onBonus_Change);

        Difficulty.value = Manager.GameManager.ChessSettings.ChessDifficulty;
        Difficulty_text.text = Manager.GameManager.ChessSettings.ChessDifficulty.ToString();
        Difficulty.onValueChanged.AddListener(on_Diff_change);
    }
     
    //Helping Functions
    public void onTime_Change(float value)
    {
        SetTime(value);
        time_text.text = value.ToString();
        Manager.GameManager.ChessSettings.ChessTime = (int)value;
    }
    
    void SetTime(float Value)
    {
        PlayerPrefs.SetFloat(time_String, Value);
    }


    public void onBonus_Change(float value)
    {
        SetBonus(value);
        bonus_text.text = value.ToString();
        Manager.GameManager.ChessSettings.ChessBonusTime = (int)value;
    }

    void SetBonus(float Value)
    {
        PlayerPrefs.SetFloat(bonus_String, Value);
    }

    public void on_Diff_change(float value)
    {
        Set_Diff(value);
        Difficulty_text.text = value.ToString();
        Manager.GameManager.ChessSettings.ChessDifficulty = (int)value;
    }

    void Set_Diff(float Value)
    {
        PlayerPrefs.SetFloat(diff_string, Value);
    }
}
