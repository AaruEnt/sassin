using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaggerAnimator : MonoBehaviour
{
    public Animator anim;

    public void ToggleOn()
    {
        var tmp = GetComponent<ConfigurableJoint>();
        if (!tmp)
            anim.SetBool("isSpinning", true);
    }

    public void ToggleOff()
    {
        anim.SetBool("isSpinning", false);
    }

    void OnCollisionEnter(Collision col)
    {
        ToggleOff();
    }
}
