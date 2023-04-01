using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    [SerializeField, Tooltip("The audio used to play walk footsteps")]
    private AudioSource audioS;

    [SerializeField, Tooltip("The audio source used to play running footsteps")]
    private AudioSource runAudio;

    public void Footstep (int index)
    {
        audioS.Play();
    }

    public void RunStep()
    {
        runAudio.Play();
    }
}
