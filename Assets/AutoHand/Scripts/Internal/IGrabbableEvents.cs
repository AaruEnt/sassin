using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {
    public interface IGrabbableEvents {


        void OnHighlight(Hand hand);

        void OnUnhighlight(Hand hand);

        void OnGrab(Hand hand);

        void OnRelease(Hand hand);

        bool CanGrab(Hand hand);

        Grabbable GetGrabbable();
    }
}