using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

        public float stabDepth { 
            
            get { return Vector3.Dot(
                
                -stabDirection.normalized,
                
                originalStabPosition - 
            
            TwitchExtension.InverseTransformPointUnscaled(jointRb.transform, 
            
            stabbedCollider.transform.position)); } }
    }
}
