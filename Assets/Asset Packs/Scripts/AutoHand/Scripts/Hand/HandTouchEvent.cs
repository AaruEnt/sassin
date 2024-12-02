using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand{
    [HelpURL("https://app.gitbook.com/s/5zKO0EvOjzUDeT2aiFk3/auto-hand/extras/hand-touch-trigger")]
    public class HandTouchEvent : MonoBehaviour{
        [Header("For Solid Collision")]
        [Tooltip("Whether or not first hand to enter should take ownership and be the only one to call events")]
        public bool oneHanded = true;
        public HandType handType = HandType.both;

        [Header("Events")]
        public UnityHandEvent HandStartTouch;
        public UnityHandEvent HandStopTouch;
        
        public HandEvent HandStartTouchEvent;
        public HandEvent HandStopTouchEvent;

        protected List<Hand> hands = new List<Hand>();

        protected virtual void OnDisable() {
            for (int i = hands.Count - 1; i >= 0; i++)
                Untouch(hands[i]);
            hands.Clear();
        }

        public virtual void Touch(Hand hand) {
            if (enabled == false || handType == HandType.none || (hand.left && handType == HandType.right) || (!hand.left && handType == HandType.left))
                return;

            if(!hands.Contains(hand)) {
                if(oneHanded && hands.Count == 0) {
                    HandStartTouch?.Invoke(hand);
                    HandStartTouchEvent?.Invoke(hand);
                }
                else {
                    HandStartTouch?.Invoke(hand);
                    HandStartTouchEvent?.Invoke(hand);
                }

                hands.Add(hand);
            }
        }
        
        public virtual void Untouch(Hand hand) {
            if (enabled == false || handType == HandType.none || (hand.left && handType == HandType.right) || (!hand.left && handType == HandType.left))
                return;

            if(hands.Contains(hand)) {
                if(oneHanded && hands[0] == hand){
                    HandStopTouch?.Invoke(hand);
                    HandStopTouchEvent?.Invoke(hand);
                }
                else if(!oneHanded){
                    HandStopTouch?.Invoke(hand);
                    HandStopTouchEvent?.Invoke(hand);
                }

                hands.Remove(hand);
            }
        }
    }
}
