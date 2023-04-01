using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine.Serialization;


namespace JointVR
{
    [RequireComponent(typeof(Rigidbody))]
    public class StabManager : MonoBehaviour
    {
        [SerializeField] public List<Stabber> stabbers = new List<Stabber>();
        [SerializeField] public List<Rigidbody> ignoreStab = new List<Rigidbody>();
        [Tag] public string ignoreStabTag;
        private Rigidbody rb;

        internal Transform maintainParent;

        public StabEvent OnStabEnter;
        public StabEvent OnStabExit;

        
        // Start is called before the first frame update
        void Start()
        {
            Stabber[] tempStabs = GetComponentsInChildren<Stabber>();

            foreach (Stabber stab in tempStabs)
                stabbers.Add(stab);

            stabbers = stabbers.Distinct().ToList();

            rb = GetComponent<Rigidbody>();

            rb.centerOfMass = Vector3.zero;
            rb.maxAngularVelocity = 50;

            foreach(Stabber s in stabbers)
                foreach (StabJoint joint in s.stabJoints)
                    joint.jointRb = rb;

            
        }

        void Update()
        {
            if (maintainParent)
                transform.parent = maintainParent;
        }

        bool AttemptStab(Stabber stabber, Collider hitCollider, Vector3 relativeVelocity)
        {
            if (!hitCollider.attachedRigidbody) return false;
            //if (hitCollider.attachedRigidbody.isKinematic) return false;
            if (hitCollider.isTrigger) return false;

            if (ColliderAlreadyStabbedByThis(hitCollider)) return false;

            if (SuccessfulAngleOfApproach(stabber.GetStabDirection(), relativeVelocity, stabber.angleThreshold) == false) return false;

            if (relativeVelocity.magnitude < stabber.velocityThreshold) return false;

            return true;
        }

        bool ColliderAlreadyStabbedByThis(Collider collider) {
            foreach (Stabber stabber in stabbers)
            {
                foreach (StabJoint joint in stabber.stabJoints)
                {
                    if (joint.stabbedCollider == collider)
                        return true;
                }
            }

            return false;
        }

        bool SuccessfulAngleOfApproach(Vector3 stabDirection, Vector3 relativeVelocity, float angleThreshold) {
            float tmp = Vector3.Dot(-relativeVelocity.normalized, stabDirection);
            return tmp > angleThreshold;
        }

        StabJoint NextUnstabbedJoint(Stabber s) {
            foreach (StabJoint joint in s.stabJoints)
            {
                if (joint.stabbedCollider == null)
                    return joint;
            }

            return null;
        }

        Vector3 RelativeVelocity(Rigidbody stabJoint, Rigidbody stabbedBody) {
            return stabJoint.velocity - stabbedBody.velocity;
        }

        private void OnCollisionEnter(Collision collision) {
            foreach(Stabber s in stabbers)
            {
                if (Time.time - s.unstabTime < 0.25f)
                    return;
            }

            List<Stabber> validGroups = new List<Stabber>();
            float closestDirection = -1;
            Stabber stab = null;
            StabJoint stabJoint = null;

            //toDo; Debug contact points

            foreach (Stabber group in stabbers)
                if (AttemptStab(group, collision.collider, collision.relativeVelocity))
                    if (IsValidTarget(collision.collider))
                        foreach (ContactPoint contact in collision.contacts)
                            foreach (Collider collider in group.colliders)
                                if (contact.thisCollider == collider)
                                {
                                    validGroups.Add(group);
                                }

            List<Collider> colliders = new List<Collider>();

            foreach (Stabber group in validGroups)
            {
                float angleOfApproach = -Vector3.Dot(collision.relativeVelocity, group.GetStabDirection());

                if (angleOfApproach > closestDirection)
                {
                    StabJoint joint = NextUnstabbedJoint(group);

                    if (joint != null)
                    {
                        closestDirection = angleOfApproach;
                        stab = group;
                        stabJoint = joint;
                    }
                }
            }

            if (stab != null && stabJoint != null)
            {
                if (collision.collider.attachedRigidbody.isKinematic)
                {
                    maintainParent = collision.body.transform;
                }
                stab.Stab(stabJoint, collision.collider);
                OnStabEnter.Invoke(collision.gameObject);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            // ewwww stinky On^4 in port, TODO try and change this
            foreach (Stabber stab in stabbers)
            {
                foreach (ContactPoint contact in collision.contacts)
                    foreach (Collider collider in stab.colliders)
                        if (contact.thisCollider == collider)
                            foreach (StabJoint joint in stab.stabJoints)
                            {
                                if (joint.unstabbedCollider == collision.collider)
                                {
                                    stab.IgnoreCollision(contact.thisCollider);
                                    joint.unstabbedCollider = null;
                                    OnStabExit.Invoke(collision.gameObject);
                                }                       
                            }
            }
        }

        private bool IsValidTarget(Collider hitCollider) {
            if (ignoreStabTag != "" && (hitCollider.gameObject.tag == ignoreStabTag || hitCollider.attachedRigidbody.gameObject.tag == ignoreStabTag))
                return false;
            if (ignoreStab.Contains(hitCollider.attachedRigidbody))
                return false;
            return true;
        }

        public bool CheckKinematicStab() {
            foreach (Stabber s in stabbers)
                if (s.kinematicStab)
                    return true;
            return false;
        }

        public void FreezeRigidbody() {
            if (CheckKinematicStab())
                rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        public void UnFreezeRigidbody() {
            rb.constraints = RigidbodyConstraints.None;
        }

        public void UnstabAll()
        {
            foreach (Stabber s in stabbers)
            {
                s.ForceUnstab();
            }
            maintainParent = null;
            transform.parent = null;
        }
    }
}

public delegate void StabEvent(GameObject other);
