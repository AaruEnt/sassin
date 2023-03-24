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

    public void PlayStabAudio(GameObject target)
    {
        if (stabbed != null)
            return;
        if (target.CompareTag("Enemy"))
            audio.PlayOneShot(guardStab, 1f);
        else
            audio.PlayOneShot(defaultStab, 1f);

        stabbed = target;
    }

    public void StabExitEvent(GameObject target)
    {
        if (stabbed == target)
            stabbed = null;
    }
}
