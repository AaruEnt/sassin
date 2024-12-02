using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.XR.Hands;

public class OpenXRAutoHandMapper : MonoBehaviour
{
    public Transform wrist;
    public Transform index;
    public Transform thumb;
    public Transform middle;
    public Transform ring;
    public Transform pinky;


    bool handTrackingSkeletonInitialized = false;
    Dictionary<XRHandJointID, Quaternion> _skeletonMap;
    public Dictionary<XRHandJointID, Quaternion> openHandReference {
        get {
            if(!handTrackingSkeletonInitialized) {
                _skeletonMap = new Dictionary<XRHandJointID, Quaternion>();
                var wrist = this.wrist;

                var indexProximal = index;
                var indexIntermediate = index.GetChild(0);
                var indexDistal = indexIntermediate.GetChild(0);
                var indexTip = indexDistal.GetChild(0);

                var middleProximal = middle;
                var middleIntermediate = middle.GetChild(0);
                var middleDistal = middleIntermediate.GetChild(0);
                var middleTip = middleDistal.GetChild(0);

                var ringProximal = ring;
                var ringIntermediate = ring.GetChild(0);
                var ringDistal = ringIntermediate.GetChild(0);
                var ringTip = ringDistal.GetChild(0);

                var littleProximal = pinky;
                var littleIntermediate = pinky.GetChild(0);
                var littleDistal = ringIntermediate.GetChild(0);
                var littleTip = ringDistal.GetChild(0);

                var thumbMetacarpal = thumb;
                var thumbProximal = thumb.GetChild(0);
                var thumbDistal = thumbProximal.GetChild(0);
                var thumbTip = thumbDistal.GetChild(0);

                _skeletonMap.Add(XRHandJointID.Wrist, wrist.rotation);
                _skeletonMap.Add(XRHandJointID.IndexProximal, indexProximal.rotation);
                _skeletonMap.Add(XRHandJointID.IndexIntermediate, indexIntermediate.rotation);
                _skeletonMap.Add(XRHandJointID.IndexDistal, indexDistal.rotation);
                _skeletonMap.Add(XRHandJointID.IndexTip, indexTip.rotation);

                _skeletonMap.Add(XRHandJointID.MiddleProximal, middleProximal.rotation);
                _skeletonMap.Add(XRHandJointID.MiddleIntermediate, middleIntermediate.rotation);
                _skeletonMap.Add(XRHandJointID.MiddleDistal, middleDistal.rotation);
                _skeletonMap.Add(XRHandJointID.MiddleTip, middleTip.rotation);

                _skeletonMap.Add(XRHandJointID.RingProximal, ringProximal.rotation);
                _skeletonMap.Add(XRHandJointID.RingIntermediate, ringIntermediate.rotation);
                _skeletonMap.Add(XRHandJointID.RingDistal, ringDistal.rotation);
                _skeletonMap.Add(XRHandJointID.RingTip, ringTip.rotation);

                _skeletonMap.Add(XRHandJointID.LittleProximal, littleProximal.rotation);
                _skeletonMap.Add(XRHandJointID.LittleIntermediate, littleIntermediate.rotation);
                _skeletonMap.Add(XRHandJointID.LittleDistal, littleDistal.rotation);
                _skeletonMap.Add(XRHandJointID.LittleTip, littleTip.rotation);

                _skeletonMap.Add(XRHandJointID.ThumbMetacarpal, thumbMetacarpal.rotation);
                _skeletonMap.Add(XRHandJointID.ThumbProximal, thumbProximal.rotation);
                _skeletonMap.Add(XRHandJointID.ThumbDistal, thumbDistal.rotation);
                _skeletonMap.Add(XRHandJointID.ThumbTip, thumbTip.rotation);
            }

            return _skeletonMap;
        }
    }
}
