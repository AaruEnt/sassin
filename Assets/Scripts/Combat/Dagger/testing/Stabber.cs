using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


namespace JointVR {
    public class Stabber : MonoBehaviour
    {
        public Transform root;
        public Transform stabDirectionTransform;
        public StabDirection stabDirection;

        public List<Collider> colliders = new List<Collider>();
        public List<StabJoint> stabJoints = new List<StabJoint>();
        public List<Stabber> disableStabJointGroupsOnStab = new List<Stabber>();
        public List<Collider> ignoreCollisionOnStab = new List<Collider>();

        public float angleThreshold = 30f;
        public float velocityThreshold = 3f;
        [SerializeField] protected float stabbedMassScale = 1;
        [SerializeField] protected float stabbedConnectedMassScale = 1;
        public float stabAmount = 5f;
        public float stabLength;
        public float restistance;
        public float unstabTimeThreshold = 0.2f;
        public float unstabDepthThreshold = 0.05f;
        internal float unstabTime;


        [ShowNativeProperty]
        public bool isStabbing
        {
            get;set;
            // get
            // {
            //     foreach (StabJoint stabJoint in stabJoints)
            //         if (stabJoint.stabbedCollider)
            //             return true;

            //     return false;
            // }
        }


        // Start is called before the first frame update
        void Awake() {
            for (int i = 0; i < stabAmount; i++)
                stabJoints.Add(new StabJoint());

            if (!root) root = GetComponentInParent<StabManager>().transform;

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
                if (stabJoint.stabbedCollider)
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

        public void Unstab(StabJoint stabJoint) {
            foreach (Stabber disableGroup in disableStabJointGroupsOnStab)
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

        // TODO
        public void Stab(StabJoint stabJoint, Collider stabbedCollider)
        {
            return;
        }


        // TODO
        public void IgnoreCollision(Collider collider)
        {
            return;
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
