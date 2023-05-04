using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManagement : MonoBehaviour
{
    public AudioMixer mixer;
    
    
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
