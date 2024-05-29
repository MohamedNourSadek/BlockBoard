using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanel : Panel
{
    public TMP_Text HeaderText;
    public TMP_Text InfoText;
    public Button NextButton;
    public Button PreviousButton;
    public Button CloseButton;


    private List<string> tutorialInfo;
    private int currentRule = 0;


    public override void Awake()
    {
        base.Awake();

        NextButton.onClick.AddListener(OnNextPressed);
        PreviousButton.onClick.AddListener(OnPreviousPressed);
        CloseButton.onClick.AddListener(OnClosePressed);

    }
    public void ShowTutorial(List<string> tutorialInfo)
    {
        Manager.GameManager.PauseGame();
        SetTutorialInfo(tutorialInfo);
        Show();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        if (currentRule < 0)
            currentRule = 0;
        else if (currentRule > tutorialInfo.Count - 1)
            currentRule = tutorialInfo.Count - 1;

        if (currentRule == 0)
        {
            PreviousButton.interactable = false;
        }
        else if (currentRule == tutorialInfo.Count - 1)
        {
            NextButton.interactable = false;
        }
        else
        {
            PreviousButton.interactable = true;
            NextButton.interactable = true;
        }

        InfoText.text = tutorialInfo[currentRule];
        HeaderText.text = "Rule " + (currentRule + 1);
    }
    

    private void SetTutorialInfo(List<string> tutorialInfo)
    {
        this.tutorialInfo = tutorialInfo;
    }
    private void OnNextPressed()
    {
        currentRule++;
        RefreshUI();
    }
    private void OnPreviousPressed()
    {
        currentRule--;
        RefreshUI();
    }
    private void OnClosePressed()
    {
        Hide();
        Manager.GameManager.ResumeGame();
        Panel.GetPanel<GameNormalPanel>().Show();
    }
}
