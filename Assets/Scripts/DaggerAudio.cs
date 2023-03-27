using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JointVR;

public class DaggerAudio : MonoBehaviour
{
    public AudioSource audio;

    public AudioClip guardStab;
    public AudioClip wallStab;
    public AudioClip defaultStab;

    public StabManager manager;

    private GameObject stabbed;

    public void OnEnable()
    {
        manager.OnStabEnter += PlayStabAudio;
        manager.OnStabExit += StabExitEvent;
    }

    public void OnDisable()
    {
        manager.OnStabEnter -= PlayStabAudio;
        manager.OnStabExit -= StabExitEvent;
    }

    public void PlayStabAudio(GameObject target)
    {
        if (target.CompareTag("Enemy"))
            audio.PlayOneShot(guardStab, 1f);
        else if (target.CompareTag("Wall"))
            audio.PlayOneShot(wallStab, 1f);
        else
            audio.PlayOneShot(defaultStab, 1f);
    }

    public void StabExitEvent(GameObject target)
    {
    }
}
