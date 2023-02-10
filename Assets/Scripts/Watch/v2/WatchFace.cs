using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchFace : MonoBehaviour
{
    public WristButtonv2 btn;
    public Animator anim;
    public Rigidbody handMatch;
    // Start is called before the first frame update
    void Start()
    {
        
    }

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
