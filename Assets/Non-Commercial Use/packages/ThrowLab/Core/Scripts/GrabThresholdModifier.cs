using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.ThrowLab
{
    public abstract class GrabThresholdModifier : MonoBehaviour
    {
        public abstract float GripValue();
        public abstract void SetGrabThreshold(float grip);
        public abstract void SetReleaseThreshold(float grip);
    }
}
