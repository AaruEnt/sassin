using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

public class CustomAdditionalGrabEvents : GrabbableExtraEvents
{
    public UnityHandGrabEvent RealOnJointBreak;
    public float minBreakDistance = 1.5f; // temporary pending finding the actual event for when distance breaks the joint

    Grabbable grab;

    void OnEnable()
    {
        grab = GetComponent<Grabbable>();
        grab.OnReleaseEvent += ReallyOnJointBreak;
    }

    void OnDisable()
    {
        grab = grab ?? GetComponent<Grabbable>();
        grab.OnReleaseEvent -= ReallyOnJointBreak;

    }

    public void ReallyOnJointBreak(Hand hand, Grabbable grab)
    {
        if ((hand.transform.position - grab.transform.position).magnitude >= minBreakDistance)
            RealOnJointBreak.Invoke(hand, grab);
    }
}