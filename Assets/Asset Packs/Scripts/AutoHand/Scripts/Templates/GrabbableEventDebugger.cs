using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Autohand.Demo {
    [RequireComponent(typeof(Grabbable))]public class GrabbableEventDebugger : MonoBehaviour
    {
        Grabbable grab;

        private void OnEnable(){
            grab = GetComponent<Grabbable>();
            grab.OnBeforeGrabEvent += OnBeforeGrabbed;
            grab.OnGrabEvent += OnGrabbed;
            grab.OnReleaseEvent += OnReleased;
            grab.OnJointBreakEvent += OnJointBreakEvent;
            grab.OnSqueezeEvent += OnSqueeze;
            grab.OnUnsqueezeEvent += OnUnsqueeze;
            grab.OnHighlightEvent += OnHighlightStart;
            grab.OnUnhighlightEvent += OnHighlightStop;
        }

        private void OnDisable(){
            grab.OnBeforeGrabEvent -= OnBeforeGrabbed;
            grab.OnGrabEvent -= OnGrabbed;
            grab.OnReleaseEvent -= OnReleased;
            grab.OnJointBreakEvent -= OnJointBreakEvent;
            grab.OnSqueezeEvent -= OnSqueeze;
            grab.OnUnsqueezeEvent -= OnUnsqueeze;
            grab.OnHighlightEvent -= OnHighlightStart;
            grab.OnUnhighlightEvent -= OnHighlightStop;
        }

        void OnBeforeGrabbed(Hand hand, Grabbable grabbable) {
            Debug.Log(grabbable.name + " BEFORE GRABBED");
        }

        void OnGrabbed(Hand hand, Grabbable grabbable) {
            Debug.Log(grabbable.name + " GRABBED");
        }

        void OnReleased(Hand hand, Grabbable grabbable) {
            Debug.Log(grabbable.name + " RELEASED");
        }

        void OnHighlightStart(Hand hand, Grabbable grabbable) {
            Debug.Log(grabbable.name + " HIGHLIGHTED");
        }

        void OnHighlightStop(Hand hand, Grabbable grabbable) {
            Debug.Log(grabbable.name + " UNHIGHLIGHTED");
        }

        void OnJointBreakEvent(Hand hand, Grabbable grabbable) {
            Debug.Log(grabbable.name + " JOINT BROKE");
        }

        void OnSqueeze(Hand hand, Grabbable grabbable) {
            Debug.Log(grabbable.name + " SQUEEZED");
        }

        void OnUnsqueeze(Hand hand, Grabbable grabbable) {
            Debug.Log(grabbable.name + " UNSQUEEZED");
        }
    }


[RequireComponent(typeof(Grabbable))]
public class GrabbableRigidbodyGrabEvents : MonoBehaviour {
    public bool setGravityWhenGrabbed = true;
    public bool setKinematicWhenGrabbed = false;
    public bool setGravityWhenReleased = false;
    public bool setKinematicWhenReleased = true;
    Grabbable grab;

    private void OnEnable() {
        grab = GetComponent<Grabbable>();
        grab.OnGrabEvent += OnGrabbed;
        grab.OnReleaseEvent += OnReleased;
    }

    private void OnDisable() {
        grab.OnGrabEvent -= OnGrabbed;
        grab.OnReleaseEvent -= OnReleased;
    }

    void OnGrabbed(Hand hand, Grabbable grabbable) {
        grabbable.body.useGravity = setGravityWhenGrabbed;
        grabbable.body.isKinematic = setKinematicWhenGrabbed;
    }

    void OnReleased(Hand hand, Grabbable grabbable) {
        grabbable.body.useGravity = setGravityWhenReleased;
        grabbable.body.isKinematic = setKinematicWhenReleased;
    }
}
}