using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Autohand;
using JointVR;

public class Climber : MonoBehaviour
{
    public StabManager s;
    public AutoHandPlayer player;
    public Climbable c;

    private bool climbStarted = false;

    // Start is called before the first frame update

    public void RemoveClimb(Hand hand, Grabbable grab) {
        if (climbStarted)
            player.EndClimb(hand, grab);
        climbStarted = false;
        c.enabled = false;
    }

    public void AttemptStartClimb(Hand hand, Grabbable grab) {
        if (s.CheckKinematicStab()) {
            c.enabled = true;
            player.StartClimb(hand, grab);
            climbStarted = true;
        }
    }
}
