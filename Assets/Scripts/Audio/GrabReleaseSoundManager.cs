using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

public class GrabReleaseSoundManager : MonoBehaviour
{
    public AudioSource grabSoundPlayer;
    public AudioSource releaseSoundPlayer;
    public AudioClip defaultGrabSound;
    public AudioClip defaultReleaseSound;

    public void PlayGrabSound(Hand hand, Grabbable grabbable)
    {
        var c = grabbable.transform.GetComponent<CustomGrabReleaseSound>();
        if (c && c.customGrabSound != null)
            grabSoundPlayer.clip = c.customGrabSound;
        else
            grabSoundPlayer.clip = defaultGrabSound;
        grabSoundPlayer.Play();
    }

    public void PlayReleaseSound(Hand hand, Grabbable grabbable)
    {
        var c = grabbable.transform.GetComponent<CustomGrabReleaseSound>();
        if (c && c.customReleaseSound != null)
            releaseSoundPlayer.clip = c.customReleaseSound;
        else
            releaseSoundPlayer.clip = defaultReleaseSound;
        releaseSoundPlayer.Play();
    }
}
