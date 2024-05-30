using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DominoPanel : Panel
{
    public TMP_Text YourScoreText;
    public TMP_Text OpponentScoreText;
    public Slider TimerSlider;
    public Slider BorrowingTimer;

    public void SetPlayersScore(int myScore, int opponentScore)
    {
        YourScoreText.text = myScore.ToString();
        OpponentScoreText.text = opponentScore.ToString();
    }


    public void SetTimer(float timeRatioLeft)
    {
        TimerSlider.value = timeRatioLeft;
    }

    public void ShowBorrowTimer(bool show)
    {
        BorrowingTimer.gameObject.SetActive(show);
    }

    public void SetBorrowTimer(float timeRatioLeft)
    {
        BorrowingTimer.value = timeRatioLeft;
    }

}
