using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialsManager : Manager
{
    public bool IsTutorialAvailable(GameType game)
    {
        string keyName = GetTutorialKey(game);

        int tutorialPlayed = PlayerPrefs.GetInt(keyName);

        if (tutorialPlayed == 0)
            return true;
        else
            return false;
    }
    public void SetTutorialState(GameType game, bool state)
    {
        string keyName = Manager.GetManager<TutorialsManager>().GetTutorialKey(game);

        if (state)
            PlayerPrefs.SetInt(keyName, 0);
        else
            PlayerPrefs.SetInt(keyName, 1);
    }
    private string GetTutorialKey(GameType game)
    {
        return game.ToString() + DataManager.TutorialSaveKey;
    }   
}

