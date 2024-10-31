using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HMVoiceLines : MonoBehaviour
{
    public AudioSource voice;
    public List<AudioClip> onExitLines = new List<AudioClip>();

    public void PlayExitLine()
    {
        if (voice.isPlaying)
        {
            voice.Stop();
        }
        voice.clip = Randomizer.PickRandomObject(onExitLines);
        voice.Play();
    }
}
