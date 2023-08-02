using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


// TODO: Add rB list. <list>.Contains(collider.attachedrigidbody) prevent stab hands and anything else

namespace JointVR {
    public class Stabber : MonoBehaviour
    {
        public Transform root;
        private StabManager manager;
        public Transform stabDirectionTransform;
        public StabDirection stabDirection;
        public Rigidbody rb;

        public List<Collider> colliders = new List<Collider>();
        public List<StabJoint> stabJoints = new List<StabJoint>();
        public List<Stabber> disableStabJointGroupsOnStab = new List<Stabber>();
        public List<Collider> ignoreCollisionOnStab = new List<Collider>();

        [Range(0, 1)]
        public float angleThreshold = 0.66f;
        public float velocityThreshold = 3f;
        [SerializeField] protected AnimationCurve dampOverTime;
        [SerializeField] protected float stabbedMassScale = 1;
        [SerializeField] protected float stabbedConnectedMassScale = 1;
        public float stabAmount = 5f;
        public float stabLength;
        public float restistance;
        public float damper;
        public float unstabTimeThreshold = 0.2f;
        public float unstabDepthThreshold = 0.05f;
        internal float unstabTime;

        public bool setStabbedColliderAsChild;
        internal bool kinematicStab
        {
            get
            {
                foreach (StabJoint stabJoint in stabJoints)
                    if (stabJoint.kinematicStab)
                        return true;

                return false;
            }
        }

        [ShowNativeProperty]
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


        // Start is called before the first frame update
        void Awake() {
            for (int i = 0; i < stabAmount; i++)
                stabJoints.Add(new StabJoint());

            manager = GetComponentInParent<StabManager>();

            if (!root) root = manager.transform;

            if (stabDirectionTransform == null)
                stabDirectionTransform = root;
            
            Debug.DrawRay(stabDirectionTransform.position, GetStabDirection(), Color.green, 5f);
        }

        // Update is called once per frame
        void Update() {
            Resistance();
            AttemptUnstab();
        }

        public Vector3 GetStabDirection() {
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

        public void Resistance() {
            foreach (StabJoint stabJoint in stabJoints)
            {
                if (stabJoint.stabbedCollider && stabJoint.previousStabDepth != 0)
                {
                    float stabDepth = stabJoint.stabDepth;

                    float friction = (stabJoint.previousStabDepth - stabDepth) / stabLength * 2;
                    stabJoint.previousStabDepth = stabDepth;

                    stabJoint.jointRb.AddForce(friction * restistance * GetStabDirection());
                }
            }
        }

        void AttemptUnstab() {
            foreach (StabJoint joint in stabJoints)
            {
                if (joint.stabbedCollider)
                {
                    float stabDepth = joint.stabDepth;

                    //Debug.Log("Stab depth " + stabDepth);

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

        public void Unstab(StabJoint stabJoint) {
            if (root.parent == stabJoint.stabbedCollider?.transform)
            {
                manager.maintainParent = null;
                root.parent = null;
            }
            foreach (Stabber disableGroup in disableStabJointGroupsOnStab)
            {
                foreach (Collider collider in disableGroup.colliders)
                    collider.enabled = true;
            }

            if (stabJoint.stabbedCollider?.transform.parent)
                if (stabJoint.stabbedCollider.transform.parent == stabJoint.joint.transform)
                    stabJoint.stabbedCollider.transform.parent = null;

            if (stabJoint.kinematicStab) {
                stabJoint.kinematicStab = false;
            }

            Destroy(stabJoint.joint);

            if (stabJoint.stabbedCollider)
            {
                IgnoreCollision(stabJoint.stabbedCollider);
                stabJoint.unstabbedCollider = stabJoint.stabbedCollider;
            }

            stabJoint.stabbedCollider = null;

            unstabTime = Time.time;
        }

        public void Stab(StabJoint stabJoint, Collider stabbedCollider)
        {
            stabJoint.originalStabPosition = root.transform.InverseTransformPoint(stabbedCollider.transform.position);

            stabJoint.stabTime = Time.time;
            stabJoint.stabbedCollider = stabbedCollider;

            Vector3 newAnchor = Vector3.zero;
            float stabLength = -this.stabLength * (stabDirection.inverse ? -1 : 1);

            newAnchor.x = stabDirection.xMotion == ConfigurableJointMotion.Limited ? stabLength : 0;
            newAnchor.y = stabDirection.yMotion == ConfigurableJointMotion.Limited ? stabLength : 0;
            newAnchor.z = stabDirection.zMotion == ConfigurableJointMotion.Limited ? stabLength : 0;

            if (stabbedCollider)
                IgnoreCollision(stabbedCollider, true);

            stabJoint.stabDirection = root.InverseTransformDirection(transform.TransformDirection(newAnchor));


            foreach (Stabber disableGroup in disableStabJointGroupsOnStab)
            {
               if(!disableGroup.isStabbing)
                   foreach (Collider collider in disableGroup.colliders)
                       collider.enabled = false;
            }

            if (setStabbedColliderAsChild)
                stabJoint.stabbedCollider.transform.parent = stabJoint.joint.transform;

            stabJoint.joint = root.gameObject.AddComponent<ConfigurableJoint>();
            stabJoint.joint.connectedBody = stabbedCollider.attachedRigidbody;
            stabJoint.joint.autoConfigureConnectedAnchor = true; // changed

            if (stabDirectionTransform)
            {
                stabJoint.joint.axis = root.InverseTransformDirection(stabDirectionTransform.right);
                stabJoint.joint.secondaryAxis = root.InverseTransformDirection(stabDirectionTransform.up);
                //newAnchor = root.InverseTransformDirection(transform.TransformDirection(newAnchor));
            }

            //stabJoint.joint.anchor = newAnchor;

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

            if (stabbedCollider.attachedRigidbody.isKinematic)
            {
                stabJoint.kinematicStab = true;
            }

            StartCoroutine(SetDampOverTime(stabJoint));
        }


        // TODO
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

        void IgnoreCollision(Collider collider, bool ignore)
        {
            foreach (Collider jointGroupCollider in ignoreCollisionOnStab)
                Physics.IgnoreCollision(jointGroupCollider, collider, ignore);
        }

        protected virtual JointDrive SetJointDrive(JointDrive jointDrive, float spring, float damper, float maximumForce)
        {
            JointDrive newDrive = new JointDrive();
            newDrive.positionSpring = spring;
            newDrive.positionDamper = damper;
            newDrive.maximumForce = maximumForce;
            return newDrive;
        }

        IEnumerator SetDampOverTime(StabJoint stabJoint)
        {
            WaitForFixedUpdate wait = new WaitForFixedUpdate();

            float newDamp = 0;

            while (newDamp < damper && stabJoint.joint != null)
            {
                newDamp = Mathf.Clamp(dampOverTime.Evaluate(Time.time - stabJoint.stabTime), 0, 1);

                newDamp = newDamp * damper;

                if (stabDirection.xMotion == ConfigurableJointMotion.Limited)
                    stabJoint.joint.xDrive = SetJointDrive(stabJoint.joint.xDrive, 0, newDamp, Mathf.Infinity);

                if (stabDirection.yMotion == ConfigurableJointMotion.Limited)
                    stabJoint.joint.yDrive = SetJointDrive(stabJoint.joint.yDrive, 0, newDamp, Mathf.Infinity);

                if (stabDirection.zMotion == ConfigurableJointMotion.Limited)
                    stabJoint.joint.zDrive = SetJointDrive(stabJoint.joint.zDrive, 0, newDamp, Mathf.Infinity);
                
                yield return wait;
            }
        }

        internal void ForceUnstab()
        {
            foreach (StabJoint j in stabJoints)
            {
                if (j.stabbedCollider && j.joint)
                {
                    Stats? s = j.stabbedCollider?.transform.root.GetComponent<Stats>();
                    if (s)
                        s.ManuallyRemoveCollision(root.gameObject);
                    Unstab(j);
                }
            }
        }
    }

    [System.Serializable]
    public class StabDirection
    {
        public ConfigurableJointMotion xMotion;
        public ConfigurableJointMotion yMotion;
        public ConfigurableJointMotion zMotion;

        public bool inverse;
    }
}
