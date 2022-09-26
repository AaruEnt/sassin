using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JointVR
{
    public class StabJointGroup : MonoBehaviour
    {
        public Transform root;

        public List<Collider> colliders = new List<Collider>();
        public List<StabJoint> stabJoints = new List<StabJoint>();

        public List<StabJointGroup> disableStabJointGroupsOnStab = new List<StabJointGroup>();
        public List<Collider> ignoreCollisionOnStab = new List<Collider>();

        public float angleThreshold;
        public float velocityThreshold;
        public float stabLength;
        public float restistance;
        public float damper;
        [Range(1,10)] public int stabAmount = 1; //How many objects this StabJointGroup can stab at one time

        public Transform stabDirectionTransform;
        public StabDirection stabDirection;

        [System.Serializable]
        public struct StabDirection
        {
            public ConfigurableJointMotion xMotion;
            public ConfigurableJointMotion yMotion;
            public ConfigurableJointMotion zMotion;

            public bool inverse;
        }

        public bool setStabbedColliderAsChild;
        public float unstabTimeThreshold = 0.2f;
        public float unstabDepthThreshold = 0.05f;

        [SerializeField] protected AnimationCurve dampOverTime;

        [SerializeField] protected float stabbedMassScale = 1;
        [SerializeField] protected float stabbedConnectedMassScale = 1;

        public bool isStabbing
        {
            get
            {
                foreach (StabJoint stabJoint in stabJoints)
                    if (stabJoint.stabbedCollider)
                        return true;

                return false;
            }
        }
 
        void Awake()
        {
            for (int i = 0; i < stabAmount; i++)
                stabJoints.Add(new StabJoint());


            if (!root) root = GetComponentInParent<StabMediator>().transform;

            if (stabDirectionTransform == null)
                stabDirectionTransform = root;

            Debug.DrawRay(stabDirectionTransform.position, GetStabDirection(), Color.green, 5f);
        }

        private void Update()
        {
            Resistance();
            AttemptUnstab();
        }

        public void Stab(StabJoint stabJoint, Collider stabbedCollider)
        {
            stabJoint.originalStabPosition = transform.InverseTransformPointUnscaled(root.transform, stabbedCollider.transform.position);

            stabJoint.stabTime = Time.time;
            stabJoint.stabbedCollider = stabbedCollider;

            Vector3 newAnchor = Vector3.zero;
            float stabLength = -this.stabLength * (stabDirection.inverse ? -1 : 1);

            newAnchor.x = stabDirection.xMotion == ConfigurableJointMotion.Limited ? stabLength : 0;
            newAnchor.y = stabDirection.yMotion == ConfigurableJointMotion.Limited ? stabLength : 0;
            newAnchor.z = stabDirection.zMotion == ConfigurableJointMotion.Limited ? stabLength : 0;

            IgnoreCollision(stabbedCollider, true);

            stabJoint.stabDirection = root.InverseTransformDirection(transform.TransformDirection(newAnchor));

            foreach (StabJointGroup disableGroup in disableStabJointGroupsOnStab)
            {
               if(!disableGroup.isStabbing)
                   foreach (Collider collider in disableGroup.colliders)
                       collider.enabled = false;
            }

            if (setStabbedColliderAsChild)
                stabJoint.stabbedCollider.transform.parent = stabJoint.joint.transform;

            stabJoint.joint = root.gameObject.AddComponent<ConfigurableJoint>();
            stabJoint.joint.connectedBody = stabbedCollider.attachedRigidbody;
            stabJoint.joint.autoConfigureConnectedAnchor = false;

            if (stabDirectionTransform)
            {
                stabJoint.joint.axis = root.InverseTransformDirection(stabDirectionTransform.right);
                stabJoint.joint.secondaryAxis = root.InverseTransformDirection(stabDirectionTransform.up);
                newAnchor = root.InverseTransformDirection(transform.TransformDirection(newAnchor));
            }

            stabJoint.joint.anchor = newAnchor;

            SoftJointLimit newLimit = new SoftJointLimit();
            newLimit.limit = Mathf.Abs(stabLength);

            stabJoint.joint.linearLimit = newLimit;

            stabJoint.joint.xMotion = stabDirection.xMotion;
            stabJoint.joint.yMotion = stabDirection.yMotion;
            stabJoint.joint.zMotion = stabDirection.zMotion;

            stabJoint.joint.angularXMotion = ConfigurableJointMotion.Locked;
            stabJoint.joint.angularYMotion = ConfigurableJointMotion.Locked;
            stabJoint.joint.angularZMotion = ConfigurableJointMotion.Locked;

            stabJoint.joint.massScale = stabbedMassScale;
            stabJoint.joint.connectedMassScale = stabbedConnectedMassScale;

            StartCoroutine(SetDampOverTime(stabJoint));
        }

        IEnumerator SetDampOverTime(StabJoint stabJoint)
        {
            WaitForFixedUpdate wait = new WaitForFixedUpdate();

            float newDamp = 0;

            while (newDamp < damper && stabJoint.joint != null)
            {
                newDamp = Mathf.Clamp(dampOverTime.Evaluate(Time.time - stabJoint.stabTime), 0, damper);

                if (stabDirection.xMotion == ConfigurableJointMotion.Limited)
                    stabJoint.joint.xDrive = SetJointDrive(stabJoint.joint.xDrive, 0, newDamp, Mathf.Infinity);

                if (stabDirection.yMotion == ConfigurableJointMotion.Limited)
                    stabJoint.joint.yDrive = SetJointDrive(stabJoint.joint.yDrive, 0, newDamp, Mathf.Infinity);

                if (stabDirection.zMotion == ConfigurableJointMotion.Limited)
                    stabJoint.joint.zDrive = SetJointDrive(stabJoint.joint.zDrive, 0, newDamp, Mathf.Infinity);
                
                yield return wait;
            }
        }

        public void Resistance()
        {
            foreach (StabJoint stabJoint in stabJoints)
            {
                if (stabJoint.stabbedCollider)
                {
                    float stabDepth = stabJoint.stabDepth;

                    float friction = (stabJoint.previousStabDepth - stabDepth) / stabLength * 2;
                    stabJoint.previousStabDepth = stabDepth;

                    stabJoint.jointRb.AddForce(friction * restistance * GetStabDirection());
                }
            }
        }

        protected virtual JointDrive SetJointDrive(JointDrive jointDrive, float spring, float damper, float maximumForce)
        {
            JointDrive newDrive = new JointDrive();
            newDrive.positionSpring = spring;
            newDrive.positionDamper = damper;
            newDrive.maximumForce = maximumForce;
            return newDrive;
        }

        void AttemptUnstab()
        {
            foreach (StabJoint joint in stabJoints)
            {
                if (joint.stabbedCollider)
                {
                    float stabDepth = joint.stabDepth;

                    //Debug.Log("Stab depth" + stabDepth);

                    if (Time.time - joint.stabTime >= unstabTimeThreshold)
                    {
                        if (stabDepth <= stabLength * unstabDepthThreshold)
                        {
                            Unstab(joint);
                        }
                    }
                }
            }
        }

        public float unstabTime;

        public void Unstab(StabJoint stabJoint)
        {
            foreach (StabJointGroup disableGroup in disableStabJointGroupsOnStab)
            {
                foreach (Collider collider in disableGroup.colliders)
                    collider.enabled = true;
            }

            if (stabJoint.stabbedCollider.transform.parent)
                if (stabJoint.stabbedCollider.transform.parent == stabJoint.joint.transform)
                    stabJoint.stabbedCollider.transform.parent = null;

            Destroy(stabJoint.joint);

            IgnoreCollision(stabJoint.stabbedCollider);

            stabJoint.unstabbedCollider = stabJoint.stabbedCollider;

            stabJoint.stabbedCollider = null;

            unstabTime = Time.time;
        }

        public void IgnoreCollision(Collider collider)
        {
            StartCoroutine(IgnoreCollisionCO(collider));
        }

        IEnumerator IgnoreCollisionCO(Collider collider)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            IgnoreCollision(collider, false);
        }

        public Vector3 GetStabDirection()
        {
            Vector3 stabDirectionVector = Vector3.zero;

            Transform stabTransform = stabDirectionTransform ? stabDirectionTransform : root;

            if (stabDirection.xMotion == ConfigurableJointMotion.Limited)
                stabDirectionVector = stabTransform.right;

            if(stabDirection.yMotion == ConfigurableJointMotion.Limited)
                stabDirectionVector = stabTransform.up;

            if (stabDirection.zMotion == ConfigurableJointMotion.Limited)
                stabDirectionVector = stabTransform.forward;

            return stabDirectionVector.normalized * (stabDirection.inverse ? -1 : 1);
        }

        void IgnoreCollision(Collider collider, bool ignore)
        {
            foreach (Collider jointGroupCollider in ignoreCollisionOnStab)
                Physics.IgnoreCollision(jointGroupCollider, collider, ignore);
        }
    }
}