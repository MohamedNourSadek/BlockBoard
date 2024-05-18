using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PokerOfflineSettingsPanel : Panel
{
    public Slider BetAmountSlider;
    public TMP_Text BetAmountText;


    public override void Awake()
    {
        base.Awake();

        BetAmountSlider.onValueChanged.AddListener(OnBetAmountChanged);
        BetAmountSlider.value = GetBetScore();
    }

    private float GetBetScore()
    {
        float betAmount = SaveManager.GetFloat(SaveManager.DominoDefaultScoreKey);

        if (betAmount == 0f)
            return 100f;
        else
            return betAmount;
    }
    private void SetBetAmount(float value)
    {
        SaveManager.SetFloat(SaveManager.DominoDefaultScoreKey, value);
        Cross_Scene_Data.BetAmount = value;
        BetAmountText.text = value.ToString();
    }
    public void OnBetAmountChanged(float value)
    {
        SetBetAmount(value);
    }

}
