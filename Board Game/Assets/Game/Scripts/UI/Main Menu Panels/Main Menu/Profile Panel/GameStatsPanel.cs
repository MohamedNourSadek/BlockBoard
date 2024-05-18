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
    private GameType gameType;

    public override void Awake()
    {
        base.Awake();

        Back.onClick.AddListener(OnBackPressed);
    }

    public void Show(GameType gameType)
    {
        this.gameType = gameType;
        base.Show();
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

        PlayerProfile profile = Manager.GetManager<ProfileManager>().GetPlayerProfile();
        
        Title.text = gameType.ToString();
        SkillLevel.text = profile.Skill[gameType].ToString();
        GamesPlayed.text = profile.GamesPlayed[gameType].ToString();
        Stats.text = "(" + profile.GamesWon[gameType].ToString() + ", " + profile.GamesDraw[gameType] + ", " + profile.GamesLost[gameType] + ")";
        AvgOpponentSkill.text = profile.AvgOpponentSkill[gameType].ToString();
    }

    public void OnBackPressed()
    {
        Hide();
        Panel.GetPanel<ProfilePanel>().Show<UserInfoPanel>();
    }
}
