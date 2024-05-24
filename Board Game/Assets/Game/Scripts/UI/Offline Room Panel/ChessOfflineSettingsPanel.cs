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

        DifficultySlider.value = Manager.GameManager.ChessSettings.ChessDifficulty;
        TimeBonusSlider.value = Manager.GameManager.ChessSettings.ChessBonusTime;
        TimeSlider.value = Manager.GameManager.ChessSettings.ChessTime;
    }

    private void SetDifficulty(float value)
    {
        Manager.GameManager.ChessSettings.ChessDifficulty = (int)value;
        DifficultyText.text = value.ToString();
    }
    private void SetTime(float value)
    {
        Manager.GameManager.ChessSettings.ChessTime = (int)value;
        TimeText.text = value.ToString();
    }
    private void SetTimeBonus(float value)
    {
        Manager.GameManager.ChessSettings.ChessBonusTime = (int)value;
        TimeBonusText.text = value.ToString();
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
