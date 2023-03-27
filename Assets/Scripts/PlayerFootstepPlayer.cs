using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

public class PlayerFootstepPlayer : MonoBehaviour
{
    public AudioSource audio;
    [Tooltip("The amont of time, in seconds, between each footstep when constantly moving at 0 momentum")]
    public float baseFootstepTime = 0.5f;

    internal float nextFootstepTime = 0.5f;
    internal AutoHandPlayer player;
    internal Rigidbody rb;
    internal MomentumController momentum;

    internal float currFootstepTime = 0f;

    private float baseMaxMovespeed;
    private float trueMaxMovespeed = 8f;
    private float minFootstepTimeRunning = 0.32f;
    private float minFootstepTimeWallrunning = 0.25f;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<AutoHandPlayer>();
        rb = GetComponent<Rigidbody>();
        momentum = GetComponent<MomentumController>();

        nextFootstepTime = baseFootstepTime;
        baseMaxMovespeed = player.maxMoveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO:
        // scale nextFootstepTime based on player velocity, with a bonus if wallrunning
        // max velocity when not jumping or sliding is 8f
        // max velocity with no momentum is 3f
        // soft minimum should be 0.32f, hard minimum is probably 0.25f
        float speed = rb.velocity.magnitude;
        float mod = speed - baseMaxMovespeed;
        if (mod > 0)
        {
            float tmp = (mod / (trueMaxMovespeed - baseMaxMovespeed));
            tmp = tmp < 0 ? 0 : tmp > 1 ? 1 : tmp;
            if (momentum.isWallRunning)
                nextFootstepTime = baseFootstepTime - (tmp * (baseFootstepTime - minFootstepTimeWallrunning));
            else
                nextFootstepTime = baseFootstepTime - (tmp * (baseFootstepTime - minFootstepTimeRunning));
        }
        else
        {
            nextFootstepTime = baseFootstepTime;
        }


        if (speed > 0f && (player.IsGrounded() || momentum.isWallRunning) && !player.crouching)
            currFootstepTime += Time.deltaTime;
        if (currFootstepTime >= nextFootstepTime)
        {
            currFootstepTime = 0f;
            audio.Play();
        }
    }
}
