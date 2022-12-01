using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

public class ClimbInteractible : Grabbable
{
    new void Start() {
        base.Start();
    }

    void OnEnable() {
        OnGrabEvent += CheckValidClimb;
        OnReleaseEvent += RemoveClimb;
        OnJointBreakEvent += RemoveClimb;
    }

    new void OnDisable() {
        base.OnDisable();
        OnGrabEvent -= CheckValidClimb;
        OnReleaseEvent -= RemoveClimb;
        OnJointBreakEvent -= RemoveClimb;
    }

    internal void CheckValidClimb(Hand hand, Grabbable grab) {
        if (grab.gameObject.GetComponent<ClimbInteractible>())
            AttemptStartClimb(hand);
    }

    internal void RemoveClimb(Hand hand, Grabbable grab) {
        if (ClimbManager.climbing.Contains(hand))
            ClimbManager.climbing.Remove(hand);
    }

    public void AttemptStartClimb(Hand hand) {
        ClimbManager.climbing.Add(hand);
    }
}
