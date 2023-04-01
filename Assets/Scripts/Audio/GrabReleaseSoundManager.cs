using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

// Allows for an override of grab/release sounds
public class GrabReleaseSoundManager : MonoBehaviour
{
    [SerializeField, Tooltip("The audio source that plays grab sounds")]
    private AudioSource grabSoundPlayer;

    [SerializeField, Tooltip("The audiosource playing release sounds")]
    private AudioSource releaseSoundPlayer;


    [SerializeField, Tooltip("Default sound for grabbing when no override applied")]
    private AudioClip defaultGrabSound;

    [SerializeField, Tooltip("Default sound for release when no override applied")]
    private AudioClip defaultReleaseSound;

    // Grab the overrider, and use it to choose audio clip to play
    public void PlayGrabSound(Hand hand, Grabbable grabbable)
    {
        var c = grabbable.transform.GetComponent<CustomGrabReleaseSound>();
        if (c && c.customGrabSound != null)
            grabSoundPlayer.clip = c.customGrabSound;
        else
            grabSoundPlayer.clip = defaultGrabSound;
        grabSoundPlayer.Play();
    }

    // Grab the overrider and use it to choose audio clip to play
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
