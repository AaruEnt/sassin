using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Valve.VR.InteractionSystem;

public class AnimatorHelper : MonoBehaviour
{
    public Animator anim;
    public AudioSource audio;
    public bool setRandomDelayOnStart = false;
    public string defaultParam = "";
    private Vector3 _startPos;
    private Quaternion _startRot;
    public Vector3 pos;
    public Quaternion rot;

    private void Start()
    {
        if (setRandomDelayOnStart)
        {
            SetRandomValOnStart(defaultParam);
        }
        _startPos = transform.position;
        _startRot = transform.rotation;
    }

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

    public void SetRandomValOnStart(string param)
    {
        StartCoroutine(RandomDelay(param));
    }

    private IEnumerator RandomDelay(string param)
    {
        yield return new WaitForSeconds((float)Randomizer.RandomRange(3, 0));
        anim.SetBool(param, true);
    }

    public void MoveToPos()
    {
        anim.enabled = false;
        transform.position = pos;
        transform.rotation = rot;
        anim.enabled = true;
    }

    public void ResetPos()
    {
        anim.enabled = false;
        transform.position = _startPos;
        transform.rotation = _startRot;
        anim.enabled = true;
    }
}
