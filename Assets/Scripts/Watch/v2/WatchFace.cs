using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchFace : MonoBehaviour
{
    [SerializeField, Tooltip("The button component on the watch")]
    private WristButtonv2 btn;

    [SerializeField, Tooltip("The animator on the watch face")]
    private Animator anim;

    [SerializeField, Tooltip("Used to match to the correct hand")]
    private Rigidbody handMatch;
    

    // Update is called once per frame
    void Update()
    {
        if (!btn.GetIsPressed() && anim.GetBool("ToggleOn"))
            anim.SetBool("ToggleOn", false);
    }

    void OnCollisionEnter(Collision col)
    {
        if (btn.GetIsPressed() == false || col.body != handMatch)
            return;
        anim.SetBool("ToggleOn", true);
    }
}
