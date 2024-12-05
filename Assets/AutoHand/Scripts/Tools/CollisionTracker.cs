using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {
    public delegate void CollisionEvent(GameObject from);

    public class CollisionTracker : MonoBehaviour {

        public bool disableCollisionTracking = false;
        public bool disableTriggersTracking = false;

        public event CollisionEvent OnCollisionFirstEnter;
        public event CollisionEvent OnCollisionLastExit;
        public event CollisionEvent OnTriggerFirstEnter;

        public event CollisionEvent OnTriggerLastExit; // todo fix typo?

        public int collisionCount { get { return collisionObjects.Count; } }
        public int triggerCount { get { return triggerObjects.Count; } }

        const int MAX_COLLISIONS_TRACKED = 256;

        public List<GameObject> triggerObjects { get; protected set; } = new List<GameObject>(MAX_COLLISIONS_TRACKED);

        public List<GameObject> collisionObjects { get; protected set; } = new List<GameObject>(MAX_COLLISIONS_TRACKED);

        public List<GameObject> nextTriggerObjects { get; protected set; } = new List<GameObject>(MAX_COLLISIONS_TRACKED);

        public List<GameObject> nextCollisionObjects { get; protected set; } = new List<GameObject>(MAX_COLLISIONS_TRACKED);

        protected List<Collision> collisions { get; set; } = new List<Collision>(MAX_COLLISIONS_TRACKED);

        Coroutine lateFixedUpdate;

        public void CleanUp() {
            triggerObjects.Clear();
            nextTriggerObjects.Clear();
            collisionObjects.Clear();
            nextCollisionObjects.Clear();
            collisions.Clear();
        }

        protected virtual void OnEnable() {
            lateFixedUpdate = StartCoroutine(LateFixedUpdate());
        }

        protected virtual void OnDisable() {
            for(int i = 0; i < collisionObjects.Count; i++) {
                if(collisionObjects[i] && OnCollisionLastExit != null)
                    OnCollisionLastExit.Invoke(collisionObjects[i]);
            }
            for(int i = 0; i < triggerObjects.Count; i++) {
                if(triggerObjects[i] && OnTriggerLastExit != null)
                        OnTriggerLastExit.Invoke(triggerObjects[i]);
            }
            CleanUp();

            StopCoroutine(lateFixedUpdate);
        }

        WaitForFixedUpdate waitForFixed = new WaitForFixedUpdate();
        IEnumerator LateFixedUpdate() {
            // using late fixed update so the events get fired in the same cycle as the collision updates happened
            while(true) {
                yield return waitForFixed;

                CheckTrackedObjects();
            }
        }

        private void CheckTrackedObjects() {
            if(!disableCollisionTracking) {
                for(int i = 0; i < collisionObjects.Count; i++) {
                    if(!collisionObjects[i].activeInHierarchy ||
                        !nextCollisionObjects.Contains(collisionObjects[i])) {
                        if(OnCollisionLastExit != null)
                            OnCollisionLastExit.Invoke(collisionObjects[i]);
                    }
                }

                for(int i = 0; i < nextCollisionObjects.Count; i++) {
                    if(nextCollisionObjects[i] == null ||
                    !nextCollisionObjects[i].activeInHierarchy) {
                        nextCollisionObjects.RemoveAt(i);
                    }
                    else if(!collisionObjects.Contains(nextCollisionObjects[i])) {
                        if(OnCollisionFirstEnter != null)
                            OnCollisionFirstEnter.Invoke(nextCollisionObjects[i]);
                    }
                }

                collisionObjects.Clear();
                collisionObjects.AddRange(nextCollisionObjects);
                nextCollisionObjects.Clear();
                collisions.Clear();
            }

            if(!disableTriggersTracking) {
                for(int i = 0; i < triggerObjects.Count; i++) {
                    if(!triggerObjects[i].activeInHierarchy ||
                        !nextTriggerObjects.Contains(triggerObjects[i])) {
                        if(OnTriggerLastExit != null)
                            OnTriggerLastExit.Invoke(triggerObjects[i]);
                    }
                }

                for(int i = 0; i < nextTriggerObjects.Count; i++) {
                    if(nextTriggerObjects[i] == null ||
                    !nextTriggerObjects[i].activeInHierarchy) {
                        nextTriggerObjects.RemoveAt(i);
                    }
                    else if(!triggerObjects.Contains(nextTriggerObjects[i])) {
                        if(OnTriggerFirstEnter != null)
                            OnTriggerFirstEnter.Invoke(nextTriggerObjects[i]);
                    }
                }

                triggerObjects.Clear();
                triggerObjects.AddRange(nextTriggerObjects);
                nextTriggerObjects.Clear();
            }
        }

        protected virtual void OnCollisionStay(Collision collision) {
            if(!disableCollisionTracking) {
                collisions.Add(collision);

                if(!nextCollisionObjects.Contains(collision.collider.gameObject)) {
                    nextCollisionObjects.Add(collision.collider.gameObject);
                }
            }
        }

        protected virtual void OnTriggerStay(Collider other) {
            if(!disableTriggersTracking) {
                if(!nextTriggerObjects.Contains(other.gameObject)) {
                    nextTriggerObjects.Add(other.gameObject);
                }
            }
        }

//#if UNITY_EDITOR
//        private void OnDrawGizmos() {
//            foreach(var collision in collisions) {
//                foreach(var contactPoint in collision.contacts) {
//                    Gizmos.DrawSphere(contactPoint.point, 0.0025f);
//                }
//            }
//        }
//#endif
    }
}