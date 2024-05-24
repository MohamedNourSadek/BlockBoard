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
        WinScoreSlider.value = Manager.GameManager.DominoSettings.DominoWinScore;
    }

    private void SetWinScore(float value)
    {
        Manager.GameManager.DominoSettings.DominoWinScore = (int)(value);
        WinScoreText.text = value.ToString();
    }
    public void OnWinScoreChanged(float value)
    {
        SetWinScore(value);
    }

}
