using Sirenix.OdinInspector.Editor.GettingStarted;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChessOfflineSettingsPanel : Panel
{
    public Slider DifficultySlider;
    public TMP_Text DifficultyText;
    public Slider TimeSlider;
    public TMP_Text TimeText;
    public Slider TimeBonusSlider;
    public TMP_Text TimeBonusText;


    public override void Awake()
    {
        base.Awake();

        TimeSlider.onValueChanged.AddListener(OnTimeChange);
        TimeBonusSlider.onValueChanged.AddListener(OnTimeBonusChange);
        DifficultySlider.onValueChanged.AddListener(OnDifficultyChange);

        DifficultySlider.value = GetDifficulty();
        TimeBonusSlider.value = GetTimeBonus();
        TimeSlider.value = GetTime();
    }


    private void SetDifficulty(float value)
    {
        SaveManager.SetFloat(SaveManager.ChessDefaultDifficultyKey, value);
        Cross_Scene_Data.chess_difficulty = value;
        DifficultyText.text = value.ToString();
    }
    private void SetTime(float value)
    {
        SaveManager.SetFloat(SaveManager.ChessDefaultTimeKey, value);
        Cross_Scene_Data.chess_Time = value;
        TimeText.text = value.ToString();
    }
    private void SetTimeBonus(float value)
    {
        SaveManager.SetFloat(SaveManager.ChessDefaultBonusKey, value);
        Cross_Scene_Data.chess_bonus = value;
        TimeBonusText.text = value.ToString();
    }
    private float GetDifficulty()
    {
        float diff = SaveManager.GetFloat(SaveManager.ChessDefaultDifficultyKey);

        if (diff == 0)
            return 100f;
        else
            return diff;
    }
    private float GetTime()
    {
        float time = SaveManager.GetFloat(SaveManager.ChessDefaultTimeKey);

        if (time == 0f)
            return 10f;
        else
            return time;
    }
    private float GetTimeBonus()
    {
        float bonus = SaveManager.GetFloat(SaveManager.ChessDefaultBonusKey);
        return bonus;
    }


    private void OnTimeChange(float value)
    {
        SetTime(value);
    }
    private void OnTimeBonusChange(float value)
    {
        SetTimeBonus(value);
    }
    private void OnDifficultyChange(float value)
    {
        SetDifficulty(value);
    }
}
