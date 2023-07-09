using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaggerAnimator : MonoBehaviour
{
    [SerializeField, Tooltip("The animator for the dagger")]
    private Animator anim;



    // Toggles on the spinning animation
    public void ToggleOn()
    {
        var tmp = GetComponent<ConfigurableJoint>();
        if (!tmp)
            anim.SetBool("isSpinning", true);
    }

    // Toggles off the spin animation
    public void ToggleOff()
    {
        anim.SetBool("isSpinning", false);
    }

    // Calls ToggleOff on hitting anything
    void OnCollisionEnter(Collision col)
    {
        ToggleOff();
    }
}
