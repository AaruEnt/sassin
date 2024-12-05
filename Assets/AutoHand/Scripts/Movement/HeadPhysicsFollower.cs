using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand{
    [RequireComponent(typeof(Rigidbody))]
    public class HeadPhysicsFollower : MonoBehaviour{

        [Header("References")]
        public Camera headCamera;
        public Transform trackingContainer;
        public Transform followBody;

        [Header("Follow Settings")]
        public float followStrength = 50f;
        [Tooltip("The maximum allowed distance from the body for the headCamera to still move")]
        public float maxBodyDistance = 1f;

        internal SphereCollider headCollider;
        Vector3 startHeadPos;
        bool started;
        float lastUpdateTime;

        Transform _moveTo = null;
        Transform moveTo {
            get {
                if(!gameObject.activeInHierarchy)
                    return null;
                if(_moveTo == null) {
                    _moveTo = new GameObject().transform;
                    _moveTo.transform.rotation = transform.rotation;
                    _moveTo.rotation = transform.rotation;
                    _moveTo.name = "HEAD FOLLOW POINT";
                    _moveTo.parent = AutoHandExtensions.transformParent;
                }

                return _moveTo;
            }
        }
        internal Rigidbody body;
        CollisionTracker collisionTracker = null;


        internal void Init() {
            if(collisionTracker == null) {
                collisionTracker = gameObject.AddComponent<CollisionTracker>();
                collisionTracker.disableTriggersTracking = true;
            }
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            gameObject.layer = LayerMask.NameToLayer(AutoHandPlayer.HandPlayerLayer);
            
            transform.position = headCamera.transform.position;
            transform.rotation = headCamera.transform.rotation;
            headCollider = GetComponent<SphereCollider>();
            //headCollider.isTrigger = true;
            startHeadPos = headCamera.transform.position;
        }

        protected void FixedUpdate() {
            moveTo.position = headCamera.transform.position;

            if(startHeadPos.y != headCamera.transform.position.y && !started) {
                started = true;
                body.position = headCamera.transform.position;
            }

            if(!started)
                return;
            
            MoveTo();
        }

        public bool Started() {
            return started;
        }
        
        internal virtual void MoveTo() {
            Vector3 currentPosition = transform.position;
            Vector3 currentHeadPosition = headCamera.transform.position;
            moveTo.position = Vector3.MoveTowards(currentPosition, currentHeadPosition, maxBodyDistance);
            body.velocity = (moveTo.position - currentPosition) * followStrength;
            lastUpdateTime = Time.realtimeSinceStartup;

            var deltaTime = (Time.realtimeSinceStartup - lastUpdateTime);
            transform.position = Vector3.MoveTowards(transform.position, moveTo.position, body.velocity.magnitude * deltaTime);
            body.velocity = Vector3.MoveTowards(body.velocity, Vector3.zero, body.velocity.magnitude * deltaTime);
            body.position = transform.position;
        }

        protected virtual void Update() {
            if(moveTo != null && !body.isKinematic) 
                MoveTo();

            lastUpdateTime = Time.realtimeSinceStartup;
        }


        public int CollisionCount() {
            return collisionTracker.collisionCount;
        }

    }
}
