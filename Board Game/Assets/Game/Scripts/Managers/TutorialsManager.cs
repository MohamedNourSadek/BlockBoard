using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialsManager : Manager
{
    public bool IsTutorialAvailable(GameType game)
    {
        string keyName = SaveManager.GetTutorialSaveKey(game);

        int tutorialPlayed = SaveManager.GetInt(keyName);

        if (tutorialPlayed == 0)
            return true;
        else
            return false;
    }
    public void SetTutorialState(GameType game, bool state)
    {
        string keyName = SaveManager.GetTutorialSaveKey(game);

        if (state)
            SaveManager.SetInt(keyName, 0);
        else
            SaveManager.SetInt(keyName, 1);
    }
   
}

