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

    // Start is called before the first frame update

    public void RemoveClimb(Hand hand, Grabbable grab) {
        player.EndClimb(hand, grab);
        c.enabled = false;
    }

    public void AttemptStartClimb(Hand hand, Grabbable grab) {
        if (s.CheckKinematicStab()) {
            c.enabled = true;
            player.StartClimb(hand, grab);
        }
    }
}
