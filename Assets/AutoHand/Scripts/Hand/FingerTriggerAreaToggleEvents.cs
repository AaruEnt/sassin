using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand
{
    [RequireComponent(typeof(FingerTriggerAreaEvents))]
    public class FingerTriggerAreaToggleEvents : MonoBehaviour
    {
        public UnityEvent<Finger, FingerTriggerAreaEvents> ToggleOn;
        public UnityEvent<Finger, FingerTriggerAreaEvents> ToggleOff;

        bool toggleState = false;

        void OnEnable() {
            GetComponent<FingerTriggerAreaEvents>().FingerEnterEvent.AddListener(Toggle);
        }

        void OnDisable() {
            GetComponent<FingerTriggerAreaEvents>().FingerEnterEvent.RemoveListener(Toggle);
        }

        void Toggle(Finger finger, FingerTriggerAreaEvents triggerArea) {
            toggleState = !toggleState;
            if(toggleState)
                ToggleOn.Invoke(finger, triggerArea);
            else
                ToggleOff.Invoke(finger, triggerArea);

        }
    }
}
