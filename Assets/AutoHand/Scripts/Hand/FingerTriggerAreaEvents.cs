using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FingerTriggerAreaEvents : HandTriggerAreaEvents
{
    [Header("Finger Trigger Events")]
    public FingerEnum[] allowedFingers;
    [Space]
    public UnityEvent<Finger, FingerTriggerAreaEvents> FingerEnterEvent;
    public UnityEvent<Finger, FingerTriggerAreaEvents> FingerExitEvent;

    protected Collider[] triggerAreaColliders;
    protected GameObject[] triggerAreaObjects;
    protected int[] startingLayers;

    Collider[] colliderNonAlloc = new Collider[32];
    bool validState = false;
    bool lastValidState = false;
    Finger currentFinger;

    protected virtual void Awake() {
        triggerAreaColliders = GetComponentsInChildren<Collider>();
        startingLayers = new int[triggerAreaColliders.Length];
        triggerAreaObjects = new GameObject[triggerAreaColliders.Length];
        for(int i = 0; i < triggerAreaColliders.Length; i++) {
            startingLayers[i] = triggerAreaColliders[i].gameObject.layer;
            triggerAreaObjects[i] = triggerAreaColliders[i].gameObject;
        }
    }


    protected virtual void FixedUpdate() {
        CheckFingerOverlapEvents();
    }

    protected virtual void CheckFingerOverlapEvents() {
        validState = false;

        if(hands.Count > 0) {
            var layer = LayerMask.NameToLayer(Hand.grabbingLayerName);
            var layerMask = LayerMask.GetMask(Hand.grabbingLayerName);
            for(int i = 0; i < triggerAreaColliders.Length; i++)
                triggerAreaObjects[i].layer = layer;

            foreach(var hand in hands) {
                foreach(var finger in hand.fingers) {
                    for(int i = 0; i < allowedFingers.Length; i++) {
                        if(finger.fingerType == allowedFingers[i]) {
                            int overlapCount = Physics.OverlapSphereNonAlloc(finger.tip.position, finger.tipRadius, colliderNonAlloc, layerMask, QueryTriggerInteraction.Collide);
                            if(overlapCount > 0) {
                                validState = true;
                                currentFinger = finger;
                                if(validState != lastValidState)
                                    OnFingerEnter(finger);
                                break;
                            }
                        }
                    }
                }
            }

            for(int i = 0; i < startingLayers.Length; i++)
                triggerAreaObjects[i].layer = startingLayers[i];
        }

        if(validState != lastValidState) {
            if(validState == false)
                OnFingerExit(currentFinger);
        }

        if(validState == false)
            currentFinger = null;
        lastValidState = validState;
    }

    protected virtual void OnFingerEnter(Finger finger) {
        FingerEnterEvent?.Invoke(finger, this);
    }

    protected virtual void OnFingerExit(Finger finger) {
        FingerExitEvent?.Invoke(finger, this);
    }
}
