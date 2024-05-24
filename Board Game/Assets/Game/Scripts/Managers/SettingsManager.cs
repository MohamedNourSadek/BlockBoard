using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class SettingsManager : Manager
{
    public float SfxVolume
    {
        set
        {
            SaveManager.SetFloat(SaveManager.SfxVolumeKey, value);
        }
        get
        {
            return SaveManager.GetFloat(SaveManager.SfxVolumeKey, 0.5f);
        }
    }
    public float MusicVolume
    {
        set
        {
            SaveManager.SetFloat(SaveManager.MusicVolumeKey, value);
        }
        get
        {
            return SaveManager.GetFloat(SaveManager.MusicVolumeKey, 0.5f);
        }
    }
    public bool CameraSnap
    {
        set
        {
            int stateInt = value ? 1 : 0;
            SaveManager.SetInt(SaveManager.CameraSnappingKey, stateInt);
            OnCameraSnapChanged?.Invoke(value);
        }
        get
        {
            return SaveManager.GetInt(SaveManager.CameraSnappingKey) == 0 ? false : true;
        }
    }

    public AudioMixer SfxMixer;
    public AudioMixer MusicMixer;

    public UnityAction<bool> OnCameraSnapChanged;

    public override void Awake()
    {
        if (SettingsManager == null)
            SettingsManager = this;
        else 
            Destroy(gameObject);

        base.Awake();
    }

    public void Start()
    {
        ChangeCameraSnap(CameraSnap);
        ChangeSfxVolume(SfxVolume);
        ChangeMusicVolume(MusicVolume);
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


    
}
