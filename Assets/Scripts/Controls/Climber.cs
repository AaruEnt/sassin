using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Autohand;
using JointVR;

public class Climber : MonoBehaviour
{
    [SerializeField, Tooltip("The stab manager on the dagger, used to check if stabbing a kinematic object")]
    private StabManager s;

    [SerializeField, Tooltip("The player component, used to start and end climbing")]
    private AutoHandPlayer player;

    [SerializeField, Tooltip("The climbable on the dagger")]
    private Climbable c;

    private bool climbStarted = false;


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
