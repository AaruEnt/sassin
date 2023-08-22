using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

public class PlayerFootstepPlayer : MonoBehaviour
{
    // Serialized Vars

    [SerializeField, Tooltip("The audio source playing footstep audio")]
    private AudioSource audioS;

    [SerializeField, Tooltip("The amont of time, in seconds, between each footstep when constantly moving at 0 momentum")]
    private float baseFootstepTime = 0.5f;

    [SerializeField, Tooltip("")]
    private Stats playerStats;

    // Internal Vars

    // Amount of time, in seconds, between each footstep at current momentum
    internal float nextFootstepTime = 0.5f;

    // The player component, used to check if player is grounded or crouching
    internal AutoHandPlayer player;

    // The player rigidbody, used for grabbing velocity to calculate footstep time
    internal Rigidbody rb;

    // The player's momentum manager, used to determine if player is wall running
    internal MomentumController momentum;

    // The time since the last footstep sound played
    internal float currFootstepTime = 0f;


    // Private Vars

    // The max movespeed at 0 momentum, grabbed from player, used to calculate scale from 0 momentum to max momentum
    private float baseMaxMovespeed;

    // See above, but manually calculated (bad) instead of grabbed from anywhere. Actual max is something like 7.998f while sprinting, but 8 is close enough.
    private float trueMaxMovespeed = 8f;

    // Soft min time, was manually set to what felt good.
    private float minFootstepTimeRunning = 0.32f;

    // Hard min time, see above.
    private float minFootstepTimeWallrunning = 0.25f;

    // Grab all private/internal variables
    void Start()
    {
        player = GetComponent<AutoHandPlayer>();
        rb = GetComponent<Rigidbody>();
        momentum = GetComponent<MomentumController>();

        nextFootstepTime = baseFootstepTime;
        baseMaxMovespeed = player.maxMoveSpeed;
    }

    // Update nextfootsteptime and play audio
    void Update()
    {
        float speed = rb.velocity.magnitude;
        float mod = speed - baseMaxMovespeed;

        // If momentum is above the minimum
        if (mod > 0)
        {
            //create a scale from 0f-1f based on where momentum is from 3-8
            float tmp = (mod / (trueMaxMovespeed - baseMaxMovespeed));
            // clamp scale to 0 and 1, sanity check
            tmp = tmp < 0 ? 0 : tmp > 1 ? 1 : tmp;
            // Set footstep time based using the relevant minimum time
            if (momentum.isWallRunning || momentum.isSprinting)
                nextFootstepTime = baseFootstepTime - (tmp * (baseFootstepTime - minFootstepTimeWallrunning));
            else
                nextFootstepTime = baseFootstepTime - (tmp * (baseFootstepTime - minFootstepTimeRunning));
        }
        else
        {
            // Reset footsteptime to base when momentum lost
            nextFootstepTime = baseFootstepTime;
        }

        // If the player is moving, and wallrunning or on the ground, and not crouching, increment time until next footstep is played
        if (speed > 0f && (player.IsGrounded() || momentum.isWallRunning) && !player.crouching && playerStats.health > 0f)
            currFootstepTime += Time.deltaTime;

        // If the time has met or exceeded the time the next footstep should be played, reset the time and play a footstep
        if (currFootstepTime >= nextFootstepTime)
        {
            currFootstepTime = 0f;
            audioS.Play();
        }
    }
}
