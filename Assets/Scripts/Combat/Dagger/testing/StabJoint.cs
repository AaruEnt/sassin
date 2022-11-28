using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


namespace JointVR
{
    [System.Serializable]
    public class StabJoint
    {
        public Collider stabbedCollider;
        public Collider unstabbedCollider;

        public Rigidbody jointRb;
        public ConfigurableJoint joint;

        public float stabTime;

        public Vector3 originalStabPosition;
        public Vector3 stabDirection;

        public float previousStabDepth;

        internal bool kinematicStab = false;

        [SerializeField, ReadOnly]
        [AllowNesting]
        private float _stabDepth;

        public float stabDepth { 
            
            get {
                _stabDepth = Vector3.Dot(
                
                -stabDirection.normalized,
                
                originalStabPosition - 
            
            jointRb.transform.InverseTransformPoint(stabbedCollider.transform.position));
            return _stabDepth;
            }
        }
    }
}