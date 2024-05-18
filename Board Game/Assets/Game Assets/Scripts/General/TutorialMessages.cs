using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class TutorialMessages : MonoBehaviour
{

    [SerializeField] List<GameObject> HelpMessages = new List<GameObject>();
    [SerializeField] AudioClip messages_Sound;
    [SerializeField] GameObject GameUI;
    [SerializeField] AudioMixer Fx_Mixer;

    public static string Domino_tut = "Domino_tutorial";
    public static string Chess_tut = "Chess_tutorial";
    public static string Poker_tut = "Poker_tutorial";

    int currentMessage = 0;
     
    private void Start()
    {
        var tutorialManagers = Manager.GetManager<TutorialsManager>();

        if (tutorialManagers.IsTutorialAvailable(Manager.GameManager.CurrentGame))
        {
            tutorialManagers.SetTutorialState(Manager.GameManager.CurrentGame, false);

            HelpMessages[currentMessage].SetActive(true);

            if ((Manager.GameManager.GameMode == GameMode.Offline))
                Time.timeScale = 0;

            GameUI.SetActive(false);
        }
    }

    public void Show_Next_Message()
    {
        foreach (GameObject g in HelpMessages)
            g.SetActive(false);

        currentMessage++;

        HelpMessages[currentMessage].SetActive(true);

        if (HelpMessages.Count - 1 == currentMessage)
        {
            Time.timeScale = 1f;
            GameUI.SetActive(true);
        }

        float Volume;
        Fx_Mixer.GetFloat("Volume", out Volume);
        float MappedVolume = AdditionalMath.Remap(Volume, new Vector2(-30f, 0f), new Vector2(0f, 1f));

        AudioSource.PlayClipAtPoint(messages_Sound, Camera.main.transform.position, MappedVolume);
    }
    public void CloseTutorial()
    {
        currentMessage = HelpMessages.Count - 2;
        Time.timeScale = 1f;
        GameUI.SetActive(true);
        Show_Next_Message();
    }



}

