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
        BetAmountSlider.value = Manager.GameManager.PokerSettings.PokerBetAmount;
    }

    private void SetBetAmount(float value)
    {
        Manager.GameManager.PokerSettings.PokerBetAmount = (int)(value);    
        BetAmountText.text = value.ToString();
    }
    public void OnBetAmountChanged(float value)
    {
        SetBetAmount(value);
    }

}
