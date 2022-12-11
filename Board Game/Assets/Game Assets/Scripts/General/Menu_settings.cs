using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.Audio;


public class Menu_settings : MonoBehaviour
{
    [SerializeField] Text gameversion;
    [SerializeField] Slider Fx_volumeSlider;
    [SerializeField] Slider Music_volumeSlider;
    [SerializeField] public Toggle Camer_Snap_Toggle;
    [SerializeField] GameObject help_Messages_Object;
    [SerializeField] AudioMixer Fx_Mixer;
    [SerializeField] AudioMixer Music_Mixer;
     
    public static string Fx_volumeCode = "Fx_volume";
    public static string Music_volumeCode = "Music_volume";
    public static string Camera_Snaaping = "Camer_Snaaping";

    public bool initialized = false;

    public void Start()
    {
        Fx_volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        Music_volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        Camer_Snap_Toggle.onValueChanged.AddListener(OnCamSnap_Changed);

        Fx_volumeSlider.value = Get_Fx_Volume();
        Music_volumeSlider.value = Get_Music_Volume();

        gameversion.text = Cross_Scene_Data.Game_Version;

        int CamSnap = Get_Camera_Snaaping();
        Camer_Snap_Toggle.isOn = (CamSnap == 0) ? false : true;
        Cross_Scene_Data.Camera_Snap = Camer_Snap_Toggle.isOn;

        initialized = true;
    }
    private void OnEnable()
    {
        Fx_volumeSlider.value = Get_Fx_Volume();
        Music_volumeSlider.value = Get_Music_Volume();
        Camer_Snap_Toggle.isOn = Cross_Scene_Data.Camera_Snap;
    }
    public void OnVolumeChanged(float volume)
    {
        Fx_Mixer.SetFloat("Volume", Fx_volumeSlider.value);
        Music_Mixer.SetFloat("Volume", Music_volumeSlider.value);

        PlayerPrefs.SetFloat(Fx_volumeCode, Fx_volumeSlider.value);
        PlayerPrefs.SetFloat(Music_volumeCode, Music_volumeSlider.value);
    }
    public void OnCamSnap_Changed(bool state)
    {
        Cross_Scene_Data.Camera_Snap = Camer_Snap_Toggle.isOn;
        int State = Camer_Snap_Toggle.isOn ? 1 : 0;
        PlayerPrefs.SetInt(Camera_Snaaping, State);

        Cross_Scene_Data.Camera_Snap_Event.Invoke(Camer_Snap_Toggle.isOn);
    }
    public static float Get_Fx_Volume()
    {
        return PlayerPrefs.GetFloat(Fx_volumeCode);
    }
    public static float Get_Music_Volume()
    {
        return PlayerPrefs.GetFloat(Music_volumeCode);
    }
    public static int Get_Camera_Snaaping()
    {
        return PlayerPrefs.GetInt(Camera_Snaaping);
    }
}
