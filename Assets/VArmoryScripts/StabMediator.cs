using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JointVR
{
    public class StabMediator : MonoBehaviour
    {
        [SerializeField] public List<StabJointGroup> stabJointGroups = new List<StabJointGroup>();

        protected void Start()
        {
            StabJointGroup[] tempGroups = GetComponentsInChildren<StabJointGroup>();

            foreach (StabJointGroup group in tempGroups)
                stabJointGroups.Add(group);

            Rigidbody rb = GetComponent<Rigidbody>();

            rb.centerOfMass = Vector3.zero;
            rb.maxAngularVelocity = 50;

            foreach(StabJointGroup stabJointGroup in stabJointGroups)
                foreach (StabJoint joint in stabJointGroup.stabJoints)
                    joint.jointRb = transform.GetComponent<Rigidbody>();
        }

        bool AttemptStab(StabJointGroup jointGroup, Collider hitCollider, Vector3 relativeVelocity)
        {
            if (!hitCollider.attachedRigidbody) return false;
            if (hitCollider.attachedRigidbody.isKinematic) return false;
            if (hitCollider.isTrigger) return false;

            if (ColliderAlreadyStabbedByThis(hitCollider)) return false;

            if (SuccessfulAngleOfApproach(jointGroup.GetStabDirection(), relativeVelocity, jointGroup.angleThreshold) == false) return false;

            if (relativeVelocity.magnitude < jointGroup.velocityThreshold) return false;

            return true;
        }

        bool ColliderAlreadyStabbedByThis(Collider collider)
        {
            foreach (StabJointGroup group in stabJointGroups)
            {
                foreach (StabJoint joint in group.stabJoints)
                {
                    if (joint.stabbedCollider == collider)
                        return true;
                }
            }

            return false;
        }

        StabJoint NextUnstabbedJoint(StabJointGroup jointGroup)
        {
            foreach (StabJoint joint in jointGroup.stabJoints)
            {
                if (joint.stabbedCollider == null)
                    return joint;
            }

            return null;
        }

        Vector3 RelativeVelocity(Rigidbody stabJoint, Rigidbody stabbedBody)
        {
            return stabJoint.velocity - stabbedBody.velocity;
        }

        bool SuccessfulAngleOfApproach(Vector3 stabDirection, Vector3 relativeVelocity, float angleThreshold)
        {
            return Vector3.Dot(-relativeVelocity.normalized, stabDirection) > angleThreshold;
        }

        private void OnCollisionEnter(Collision collision)
        {
            foreach(StabJointGroup group in stabJointGroups)
            {
                if (Time.time - group.unstabTime < 0.25f)
                    return;
            }

            List<StabJointGroup> validGroups = new List<StabJointGroup>();
            float closestDirection = -1;
            StabJointGroup stabGroup = null;
            StabJoint stabJoint = null;

            //toDo; Debug contact points

            foreach (StabJointGroup group in stabJointGroups)
                if (AttemptStab(group, collision.collider, collision.relativeVelocity))
                    foreach (ContactPoint contact in collision.contacts)
                        foreach (Collider collider in group.colliders)
                            if (contact.thisCollider == collider)
                            {
                                validGroups.Add(group);
                            }

            List<Collider> colliders = new List<Collider>();

            foreach (StabJointGroup group in validGroups)
            {
                float angleOfApproach = -Vector3.Dot(collision.relativeVelocity, group.GetStabDirection());

                if (angleOfApproach > closestDirection)
                {
                    StabJoint joint = NextUnstabbedJoint(group);

                    if (joint != null)
                    {
                        closestDirection = angleOfApproach;
                        stabGroup = group;
                        stabJoint = joint;
                    }
                }
            }

            if (stabGroup != null && stabJoint != null)
            {
                stabGroup.Stab(stabJoint, collision.collider);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            foreach (StabJointGroup group in stabJointGroups)
            {
                foreach (ContactPoint contact in collision.contacts)
                    foreach (Collider collider in group.colliders)
                        if (contact.thisCollider == collider)
                            foreach (StabJoint joint in group.stabJoints)
                            {
                                if (joint.unstabbedCollider == collision.collider)
                                {
                                    group.IgnoreCollision(contact.thisCollider);
                                    joint.unstabbedCollider = null;
                                }                       
                            }
            }
        }
    }
}