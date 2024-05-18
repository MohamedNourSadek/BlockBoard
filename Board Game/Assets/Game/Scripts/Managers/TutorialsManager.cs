using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialsManager : Manager
{
    public bool IsTutorialAvailable(GameType game)
    {
        string keyName = SaveManager.GetTutorialSaveKey(game);
        bool tutorialPlayed = SaveManager.GetInt(keyName) == 1;

        return !tutorialPlayed;
    }
    public void SetTutorialState(GameType game, bool state)
    {
        string keyName = SaveManager.GetTutorialSaveKey(game);

        if (state)
            SaveManager.SetInt(keyName, 1);
        else
            SaveManager.SetInt(keyName, 0);
    }
   
}

