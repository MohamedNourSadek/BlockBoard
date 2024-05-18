using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class SettingsManager : Manager
{
    public AudioMixer SfxMixer;
    public AudioMixer MusicMixer;
    public bool CameraSnap;

    public UnityAction<bool> OnCameraSnapChanged;


    public void Start()
    {
        ChangeCameraSnap(GetSavedCameraSnap());
        ChangeSfxVolume(GetSavedSfxVolume());
        ChangeMusicVolume(GetSavedMusicVolume());
    }

    public void ChangeSfxVolume(float volume)
    {
        SfxMixer.SetFloat("Volume", volume);
        SaveManager.SetFloat(SaveManager.SfxVolumeKey, volume);
    }
    public void ChangeMusicVolume(float volume)
    {
        MusicMixer.SetFloat("Volume", volume);
        SaveManager.SetFloat(SaveManager.MusicVolumeKey, volume);
    }   
    public void ChangeCameraSnap(bool state)
    {
        int stateInt = state ? 1 : 0;
        SaveManager.SetInt(SaveManager.CameraSnappingKey, stateInt);
        CameraSnap = state;
        OnCameraSnapChanged?.Invoke(state);
    }


    public float GetSavedSfxVolume()
    {
        return SaveManager.GetFloat(SaveManager.SfxVolumeKey, 0.5f);
    }
    public float GetSavedMusicVolume()
    {
        return SaveManager.GetFloat(SaveManager.MusicVolumeKey, 0.5f);
    }
    public bool GetSavedCameraSnap()
    {
        return SaveManager.GetInt(SaveManager.CameraSnappingKey) == 0 ? false : true;
    }
}
