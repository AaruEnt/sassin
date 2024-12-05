using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Autohand.Demo{
public class XRHeadTracking : MonoBehaviour{
#if UNITY_2020_3_OR_NEWER
        public TrackingOriginModeFlags mode = TrackingOriginModeFlags.TrackingReference;

    void Start(){
        List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        for(int i = 0;  i < subsystems.Count; i++){
            subsystems[i].TrySetTrackingOriginMode(mode);
        }
    }
#endif
    }
}
