using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JointVR;

public class DaggerAudio : MonoBehaviour
{
    [SerializeField, Tooltip("The audio source used for all dagger sounds")]
    private AudioSource audioS;

    [SerializeField, Tooltip("The audio clip for stabbing an enemy")]
    private AudioClip guardStab;

    [SerializeField, Tooltip("The audio clip for stabbing a wall")]
    private AudioClip wallStab;

    [SerializeField, Tooltip("Default stab sound")]
    private AudioClip defaultStab;

    [SerializeField, Tooltip("The stabmanager on the dagger, used to subscribe to stab events.")]
    private StabManager manager;


    // Subscribe to stab events
    public void OnEnable()
    {
        manager.OnStabEnter += PlayStabAudio;
        manager.OnStabExit += StabExitEvent;
    }

    // Unsubscribe to stab events
    public void OnDisable()
    {
        manager.OnStabEnter -= PlayStabAudio;
        manager.OnStabExit -= StabExitEvent;
    }

    // Determine which stab audio to use, then play
    public void PlayStabAudio(GameObject target)
    {
        if (target.CompareTag("Enemy"))
            audioS.PlayOneShot(guardStab, 1f);
        else if (target.CompareTag("Wall"))
            audioS.PlayOneShot(wallStab, 1f);
        else
            audioS.PlayOneShot(defaultStab, 1f);
    }

    public void StabExitEvent(GameObject target)
    {
    }
}
