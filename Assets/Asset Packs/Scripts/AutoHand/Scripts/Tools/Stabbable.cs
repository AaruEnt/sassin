using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand{
    [HelpURL("https://app.gitbook.com/s/5zKO0EvOjzUDeT2aiFk3/auto-hand/extras/stabbing")]
    public class Stabbable : MonoBehaviour{
        public Rigidbody body;
        public Grabbable grabbable;

        [Tooltip("The index that must match the stabbers index to allow stabbing")]
        public int stabIndex = 0;
        public int maxStabbers = 1;
        public float positionDamper = 1000;
        public float rotationDamper = 1000;
        public bool parentOnStab = true;

        [Header("Events")]
        public UnityEvent StartStab;
        public UnityEvent EndStab;
        
        //Progammer Events <3
        public StabEvent StartStabEvent;
        public StabEvent EndStabEvent;

        public List<Stabber> currentStabbers { get; private set; }
        public int currentStabs { get; private set; }

        Transform prereleaseParent;

        private void OnEnable() {
            currentStabbers = new List<Stabber>();
            if(!body && GetComponent<Rigidbody>())
                body = GetComponent<Rigidbody>();

            if(grabbable == null) {
                body.gameObject.HasGrabbable(out grabbable);
            }
        }

        public virtual void OnStab(Stabber stabber) {
            currentStabs++;
            currentStabbers.Add(stabber);

            if(parentOnStab && grabbable != null && stabber.grabbable != null) {
                grabbable.AddJointedBody(stabber.grabbable.body);
                for(int i = 0; i < stabber.stabbed.Count; i++) {
                    if(stabber.stabbed[i] != this) {
                        var stabbable = stabber.stabbed[i];
                        if(stabbable != this && stabbable.grabbable != null && stabbable.parentOnStab && stabbable.grabbable.parentOnGrab) {
                            if(grabbable.parentOnGrab)
                                grabbable.AddJointedBody(stabbable.grabbable.body);
                            stabbable.grabbable.AddJointedBody(grabbable.body);
                        }
                    }
                }
            }



            StartStab?.Invoke();
            StartStabEvent?.Invoke(stabber, this);
        }

        public virtual void OnEndStab(Stabber stabber) {
            currentStabs--;
            currentStabbers.Remove(stabber);
            if(parentOnStab && grabbable && stabber.grabbable) {
                grabbable.RemoveJointedBody(stabber.grabbable.body);

                for(int i = 0; i < stabber.stabbed.Count; i++) {
                    if(stabber.stabbed[i] != this) {
                        var stabbable = stabber.stabbed[i];
                        if(stabbable != this && stabbable.grabbable != null && stabbable.parentOnStab && stabbable.grabbable.parentOnGrab) {
                            grabbable.RemoveJointedBody(stabbable.grabbable.body);
                            stabbable.grabbable.RemoveJointedBody(grabbable.body);
                        }
                    }
                }
            }

            EndStab?.Invoke();
            EndStabEvent?.Invoke(stabber, this);
        }

        public virtual bool CanStab(Stabber stabber) {
            return currentStabs < maxStabbers && stabber.stabIndex == stabIndex;
        }

        public int StabbedCount() {
            return currentStabbers.Count;
        }

    }
}
