using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand{
    public delegate void HandAreaEvent(Hand hand, HandTriggerAreaEvents area);
    public delegate void HandEvent(Hand hand);

    [HelpURL("https://app.gitbook.com/o/v43F1UfKchmlV5VQCpro/s/5zKO0EvOjzUDeT2aiFk3/auto-hand/extras/hand-touch-trigger")]
    public class HandTriggerAreaEvents : MonoBehaviour{
        [Header("Trigger Events Settings")]
        [Tooltip("Whether or not first hand to enter should take ownership and be the only one to call events")]
        public bool oneHanded = true;
        public HandType handType = HandType.both;
        [Tooltip("Whether or not to call the release event if exiting while grab event activated")]
        public bool exitTriggerRelease = true;
        [Tooltip("Whether or not to call the release event if exiting while grab event activated")]
        public bool exitTriggerUnsqueeze = true;

        [Header("Events")]
        public UnityHandEvent HandEnter;
        public UnityHandEvent HandExit;
        public UnityHandEvent HandGrab;
        public UnityHandEvent HandRelease;
        public UnityHandEvent HandSqueeze;
        public UnityHandEvent HandUnsqueeze;

        //For Programmers <3
        public HandAreaEvent HandEnterEvent;
        public HandAreaEvent HandExitEvent;
        public HandAreaEvent HandGrabEvent;
        public HandAreaEvent HandReleaseEvent;
        public HandAreaEvent HandSqueezeEvent;
        public HandAreaEvent HandUnsqueezeEvent;

        protected List<Hand> hands = new List<Hand>();
        protected bool grabbing;
        protected bool squeezing;

        protected virtual void OnDisable() {
            for (int i = hands.Count - 1; i >= 0; i--){
                hands[i].RemoveHandTriggerArea(this);
            }
        }

        protected virtual void Update(){
            foreach (var hand in hands){
                if (!hand.enabled) {
                    Exit(hand);
                    Release(hand);
                }
            }
        }


        public virtual void Enter(Hand hand) {
            if (enabled == false || handType == HandType.none || (hand.left && handType == HandType.right) || (!hand.left && handType == HandType.left))
                return;

            if(!hands.Contains(hand)) {
                hands.Add(hand);
                if(oneHanded && hands.Count == 1) {
                    HandEnter?.Invoke(hand);
                    HandEnterEvent?.Invoke(hand, this);
                }
                else {
                    HandEnter?.Invoke(hand);
                    HandEnterEvent?.Invoke(hand, this);
                }
            }
        }

        public virtual void Exit(Hand hand) {
            if (enabled == false || handType == HandType.none || (hand.left && handType == HandType.right) || (!hand.left && handType == HandType.left))
                return;

            if(hands.Contains(hand)) {
                if(oneHanded && hands[0] == hand){
                    HandExit?.Invoke(hand);
                    HandExitEvent?.Invoke(hand, this);

                    if(grabbing && exitTriggerRelease){
                        HandRelease?.Invoke(hand);
                        HandReleaseEvent?.Invoke(hand, this);
                        grabbing = false;
                    }
                    if(squeezing && exitTriggerUnsqueeze){
                        HandUnsqueeze?.Invoke(hand);
                        HandUnsqueezeEvent?.Invoke(hand, this);
                        squeezing = false;
                    }

                    //If there is another hand, it enters
                    if(hands.Count > 1) {
                        HandEnter?.Invoke(hands[1]);
                        HandEnterEvent?.Invoke(hands[1], this);
                    }

                }
                else if(!oneHanded) {
                    HandExit?.Invoke(hand);
                    HandExitEvent?.Invoke(hand, this);

                    if(grabbing && exitTriggerRelease){
                        HandRelease?.Invoke(hand);
                        HandReleaseEvent?.Invoke(hand, this);
                        grabbing = false;
                    }
                    if(squeezing && exitTriggerUnsqueeze){
                        HandUnsqueeze?.Invoke(hand);
                        HandUnsqueezeEvent?.Invoke(hand, this);
                        squeezing = false;
                    }

                }

                hands.Remove(hand);
            }
        }


        public virtual void Grab(Hand hand) {
            if (enabled == false || handType == HandType.none || (hand.left && handType == HandType.right) || (!hand.left && handType == HandType.left))
                return;

            if(grabbing)
                return;

            if(oneHanded && hands[0] == hand){
                HandGrab?.Invoke(hand);
                HandGrabEvent?.Invoke(hand, this);
                grabbing = true;
            }
            else if(!oneHanded){
                HandGrab?.Invoke(hand);
                HandGrabEvent?.Invoke(hand, this);
                grabbing = true;
            }
        }

        public virtual void Release(Hand hand) {
            if (enabled == false || handType == HandType.none || (hand.left && handType == HandType.right) || (!hand.left && handType == HandType.left))
                return;

            if(!grabbing)
                return;

            if(oneHanded && hands[0] == hand){
                HandRelease?.Invoke(hand);
                HandReleaseEvent?.Invoke(hand, this);
                grabbing = false;
            }
            else if(!oneHanded){
                HandRelease?.Invoke(hand);
                HandReleaseEvent?.Invoke(hand, this);
                grabbing = false;
            }
        }


        public virtual void Squeeze(Hand hand) {
            if (enabled == false || handType == HandType.none || (hand.left && handType == HandType.right) || (!hand.left && handType == HandType.left))
                return;

            if(squeezing)
                return;

            if(oneHanded && hands[0] == hand){
                HandSqueeze?.Invoke(hand);
                HandSqueezeEvent?.Invoke(hand, this);
                squeezing = true;
            }
            else if(!oneHanded){
                squeezing = true;
                HandSqueeze?.Invoke(hand);
                HandSqueezeEvent?.Invoke(hand, this);
            }
        }

        public virtual void Unsqueeze(Hand hand) {
            if (enabled == false || handType == HandType.none || (hand.left && handType == HandType.right) || (!hand.left && handType == HandType.left))
                return;

            if(!squeezing)
                return;

            if(oneHanded && hands[0] == hand){
                HandUnsqueeze?.Invoke(hand);
                HandUnsqueezeEvent?.Invoke(hand, this);
                squeezing = false;
            }
            else if(!oneHanded){
                squeezing = false;
                HandUnsqueeze?.Invoke(hand);
                HandUnsqueezeEvent?.Invoke(hand, this);
            }
        }
}
}
