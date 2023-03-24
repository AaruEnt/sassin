using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    public AudioSource audio;
    public AudioSource runAudio;

    public void Footstep (int index)
    {
        audio.Play();
    }

    public void RunStep()
    {
        Debug.Log("RunCall");
        runAudio.Play();
    }
}
