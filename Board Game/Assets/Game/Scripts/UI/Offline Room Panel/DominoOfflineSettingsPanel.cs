using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class DominoOfflineSettingsPanel : Panel
{
    public Slider WinScoreSlider;
    public TMP_Text WinScoreText;


    public override void Awake()
    {
        base.Awake();

        WinScoreSlider.onValueChanged.AddListener(OnWinScoreChanged);
        WinScoreSlider.value = GetWinScore();
    }

    private float GetWinScore()
    {
        float winScore = SaveManager.GetFloat(SaveManager.DominoDefaultScoreKey);

        if (winScore == 0f)
            return 100f;
        else
            return winScore;
    }
    private void SetWinScore(float value)
    {
        SaveManager.SetFloat(SaveManager.DominoDefaultScoreKey, value);
        Cross_Scene_Data.DominoWinScore = value;
        WinScoreText.text = value.ToString();
    }
    public void OnWinScoreChanged(float value)
    {
        SetWinScore(value);
    }

}
