using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsPanel : Panel
{
    public TMP_Text GameVersion;
    public Slider SfxVolumeSlider;
    public Slider MusicVolumeSlider;
    public Toggle CameraSnapToggle;
    public Button BackButton;


    private SettingsManager SettingsManager;

    public override void Start()
    {
        SettingsManager = Manager.GetManager<SettingsManager>();  

        GameVersion.text = Manager.GameManager.GameVersion;

        SfxVolumeSlider.onValueChanged.AddListener(SettingsManager.ChangeSfxVolume);
        MusicVolumeSlider.onValueChanged.AddListener(SettingsManager.ChangeMusicVolume);
        CameraSnapToggle.onValueChanged.AddListener(SettingsManager.ChangeCameraSnap);

        BackButton.onClick.AddListener(OnBackPressed);

        RefreshUI();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        if (SettingsManager == null)
            SettingsManager = Manager.GetManager<SettingsManager>();
        
        SfxVolumeSlider.value = SettingsManager.SfxVolume;
        MusicVolumeSlider.value = SettingsManager.MusicVolume;
        CameraSnapToggle.isOn = SettingsManager.CameraSnap;
    }
    public void OnBackPressed()
    {
        Hide();

        if (Panel.GetPanel<MainMenuPanel>())
            Panel.GetPanel<MainMenuPanel>().Show();
        else
            Panel.GetPanel<PausePanel>().OnBackFromSettingsPressed();
    }
}
