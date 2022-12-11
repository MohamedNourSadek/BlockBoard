using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class Button_Sound : MonoBehaviour
{
    [SerializeField] List<AudioClip> clips = new List<AudioClip>();
    [SerializeField] AudioMixer Fx_Mixer;
    [SerializeField] float volume = 1f;
   
    public void Play_OnPress()
    {
        AudioClip audio = clips[Random.Range(0, clips.Count)];

        float Volume;
        Fx_Mixer.GetFloat("Volume", out Volume);
        float MappedVolume = AdditionalMath.Remap(Volume, new Vector2(-30f, 0f), new Vector2(0f, volume));
        AudioSource.PlayClipAtPoint(audio, Camera.main.transform.position, MappedVolume);
    }
}
