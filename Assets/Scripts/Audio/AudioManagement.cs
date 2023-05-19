using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManagement : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider BGMSlider;
    public Slider SFXSlider;
    public Slider AmbianceSlider;
    public Slider VoiceSlider;

    public void Start()
    {
        if (PlayerPrefs.HasKey("BGM"))
        {
            float bgm = PlayerPrefs.GetFloat("BGM");
            mixer.SetFloat("BGMVolume", bgm);
            BGMSlider.value = bgm;
        }
        if (PlayerPrefs.HasKey("SFX"))
        {
            float sfx = PlayerPrefs.GetFloat("SFX");
            mixer.SetFloat("SFXVolume", sfx);
            BGMSlider.value = sfx;
        }

        this.gameObject.SetActive(false);
    }
    public void OnDisable()
    {
        PlayerPrefs.SetFloat("BGM", BGMSlider.value);
    }


    public void SetBGMVolume(Slider slider)
    {
        mixer.SetFloat("BGMVolume", slider.value);
    }
    public void SetSFXVolume(Slider slider)
    {
        mixer.SetFloat("SFXMain", slider.value);
    }
    public void SetAmbianceVolume(Slider slider)
    {
        mixer.SetFloat("AmbianceMain", slider.value);
    }
    public void SetVoiceVolume(Slider slider)
    {
        mixer.SetFloat("VoiceVolume", slider.value);
    }
}
