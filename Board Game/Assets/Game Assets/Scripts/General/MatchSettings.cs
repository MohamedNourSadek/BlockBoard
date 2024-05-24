using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchSettings : MonoBehaviour
{
    [SerializeField] Slider MaxScore;
    [SerializeField] Text maxScore_text;

    public static string MaxScore_string = "MaxScore";

    private void Awake()
    {
        MaxScore.value = GetMaxScore();
        Manager.GameManager.DominoSettings.DominoWinScore = GetMaxScore();

        maxScore_text.text = GetMaxScore().ToString();
        MaxScore.onValueChanged.AddListener(OnMaxScore_Change);
    }

    //Helping Functions
    public void OnMaxScore_Change(float value)
    {
        SetMaxScore(value);
        maxScore_text.text = value.ToString();
        Manager.GameManager.DominoSettings.DominoWinScore = (int)value;
    }
    public static int GetMaxScore()
    {
        float MaxScore = PlayerPrefs.GetFloat(MaxScore_string);

        if (MaxScore == 0f)
            return 100;
        else
            return (int)MaxScore;
    }
    void SetMaxScore(float Value)
    {
        PlayerPrefs.SetFloat(MaxScore_string, Value);
    }
}
