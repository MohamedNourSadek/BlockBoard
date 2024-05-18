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
        Time_slider.value = GetTime();
        Cross_Scene_Data.chess_Time = GetTime();
        time_text.text = GetTime().ToString();
        Time_slider.onValueChanged.AddListener(onTime_Change);

        bonus_slider.value = GetBonus();
        Cross_Scene_Data.chess_bonus = GetBonus();
        bonus_text.text = GetBonus().ToString();
        bonus_slider.onValueChanged.AddListener(onBonus_Change);

        Difficulty.value = GetDiff();
        Cross_Scene_Data.chess_difficulty = GetDiff();
        Difficulty_text.text = GetDiff().ToString();
        Difficulty.onValueChanged.AddListener(on_Diff_change);
    }
     
    //Helping Functions
    public void onTime_Change(float value)
    {
        SetTime(value);
        time_text.text = value.ToString();
        Cross_Scene_Data.chess_Time = value;
    }
    public float GetTime()
    {
        float time = PlayerPrefs.GetFloat(time_String);

        if (time == 0f)
            return 10f;
        else
            return time;
    }
    void SetTime(float Value)
    {
        PlayerPrefs.SetFloat(time_String, Value);
    }


    public void onBonus_Change(float value)
    {
        SetBonus(value);
        bonus_text.text = value.ToString();
        Cross_Scene_Data.chess_bonus = value;
    }
    public float GetBonus()
    {
        float bonus = PlayerPrefs.GetFloat(bonus_String);
            return bonus;
    }
    void SetBonus(float Value)
    {
        PlayerPrefs.SetFloat(bonus_String, Value);
    }

    public void on_Diff_change(float value)
    {
        Set_Diff(value);
        Difficulty_text.text = value.ToString();
        Cross_Scene_Data.chess_difficulty = value;
    }
    public float GetDiff()
    {
        float diff = PlayerPrefs.GetFloat(diff_string);
        if (diff == 0)
            return 100f;
        else
            return diff;
    }
    void Set_Diff(float Value)
    {
        PlayerPrefs.SetFloat(diff_string, Value);
    }
}
