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
        Cross_Scene_Data.AI_MaxScore = GetMaxScore();

        maxScore_text.text = GetMaxScore().ToString();
        MaxScore.onValueChanged.AddListener(OnMaxScore_Change);
    }

    //Helping Functions
    public void OnMaxScore_Change(float value)
    {
        SetMaxScore(value);
        maxScore_text.text = value.ToString();
        Cross_Scene_Data.AI_MaxScore = value;
    }
    public static float GetMaxScore()
    {
        float MaxScore = PlayerPrefs.GetFloat(MaxScore_string);

        if (MaxScore == 0f)
            return 100f;
        else
            return MaxScore;
    }
    void SetMaxScore(float Value)
    {
        PlayerPrefs.SetFloat(MaxScore_string, Value);
    }
}
