using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Autohand;
using JointVR;

public class Climber : MonoBehaviour
{
    public bool hand;
    [ShowIf("hand")]
    public HandPublicEvents h;
    [HideIf("hand")]
    public StabManager s;
    // Start is called before the first frame update
    void OnEnable()
    {
        if (hand) {
            h.hand.OnGrabbed += CheckValidClimb;
        }
    }

    void OnDisable() {
        if (hand) {
            h.hand.OnGrabbed -= CheckValidClimb;
        }
    }

    public void CheckValidClimb(Hand hand, Grabbable grab) {
        if (grab.gameObject.GetComponent<ClimbInteractible>())
            AttemptStartClimb();
    }

    internal void AttemptStartClimb() {
        ClimbManager.climbing.Add(this);
    }
}
