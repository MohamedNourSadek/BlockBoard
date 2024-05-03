using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : Manager
{
    public List<AudioClip> Clips = new List<AudioClip>();
    public AudioMixer FxMixer;

    [Header("Sounds Volume")]
    [Range(0,1f)] public float ButtonClickVolume = 1f;

    public void PlayButtonClick()
    {
        AudioClip audio = Clips[Random.Range(0, Clips.Count)];

        float fxVolume;
        FxMixer.GetFloat("Volume", out fxVolume);
        float mappedVolume = AdditionalMath.Remap(fxVolume, new Vector2(-30f, 0f), new Vector2(0f, ButtonClickVolume));


        AudioSource.PlayClipAtPoint(audio, Camera.main.transform.position, mappedVolume);
    }
}
