using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStatsPanel : Panel
{
    public TMP_Text Title;
    public TMP_Text SkillLevel;
    public TMP_Text GamesPlayed;
    public TMP_Text Stats;
    public TMP_Text AvgOpponentSkill;

    public Button Back;

    public override void Awake()
    {
        base.Awake();

        Back.onClick.AddListener(OnBackPressed);
    }

    public void Show(GameType gameType)
    {
        Title.text = gameType.ToString();
    }

    public void OnBackPressed()
    {
        Hide();
        Panel.GetPanel<ProfilePanel>().Show<UserInfoPanel>();
    }
}
