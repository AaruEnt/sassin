using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Valve.VR.InteractionSystem;

public class AnimatorHelper : MonoBehaviour
{
    public Animator anim;
    public AudioSource audio;

    public void PlayAnim(string animParam)
    {
        StartCoroutine(ToggleAnim(animParam));
    }

    private IEnumerator ToggleAnim(string animParam)
    {

        anim.SetBool(animParam, true);
        yield return null;
        anim.SetBool(animParam, false);
    }

    public void PlaySound()
    {
        audio.Play();
    }
}
