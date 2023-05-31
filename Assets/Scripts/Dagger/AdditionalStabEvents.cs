using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace JointVR
{
    public class AdditionalStabEvents : MonoBehaviour
    {
        public PlusStabEvent OnStabbedEvent;
        public PlusStabEvent OnUnStabbedEvent;

        internal void OnStabEnter(StabManager m, GameObject target)
        {
            OnStabbedEvent.Invoke(m, target);
        }

        internal void OnStabExit(StabManager m, GameObject target)
        {
            OnUnStabbedEvent.Invoke(m, target);
        }
    }

    [System.Serializable]
    public class PlusStabEvent : UnityEvent<StabManager, GameObject> { }
}
