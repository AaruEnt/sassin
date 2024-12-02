using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {
    [RequireComponent(typeof(Grabbable))]
    public class GrabbableExtraEvents : MonoBehaviour {
        public UnityHandGrabEvent OnFirstGrab;
        public UnityHandGrabEvent OnLastRelease;
        public UnityHandGrabEvent OnTwoHandedGrab;
        public UnityHandGrabEvent OnTwoHandedRelease;

        [Space]
        public UnityPlacePointEvent OnPlacePointAdd;
        public UnityPlacePointEvent OnPlacePointRemove;
        public UnityPlacePointEvent OnPlacePointHighlight;
        public UnityPlacePointEvent OnPlacePointUnhighlight;


        Grabbable grab;

        void OnEnable() {
            grab = GetComponent<Grabbable>();
            grab.OnGrabEvent += Grab;
            grab.OnReleaseEvent += Release;
            grab.OnPlacePointAddEvent += PlacePointAdd;
            grab.OnPlacePointRemoveEvent += PlacePointRemove;
            grab.OnPlacePointHighlightEvent += PlacePointHighlight;
            grab.OnPlacePointUnhighlightEvent += PlacePointUnhighlight;
        }

        void OnDisable() {
            grab = grab ?? GetComponent<Grabbable>();
            grab.OnGrabEvent -= Grab;
            grab.OnReleaseEvent -= Release;
            grab.OnPlacePointAddEvent -= PlacePointAdd;
            grab.OnPlacePointRemoveEvent -= PlacePointRemove;
            grab.OnPlacePointHighlightEvent -= PlacePointHighlight;
            grab.OnPlacePointUnhighlightEvent -= PlacePointUnhighlight;


        }

        public void PlacePointAdd(PlacePoint point, Grabbable grab) {
            OnPlacePointAdd?.Invoke(point, grab);
        }

        public void PlacePointRemove(PlacePoint point, Grabbable grab) {
            OnPlacePointRemove?.Invoke(point, grab);
        }

        public void PlacePointHighlight(PlacePoint point, Grabbable grab) {
            OnPlacePointHighlight?.Invoke(point, grab);
        }

        public void PlacePointUnhighlight(PlacePoint point, Grabbable grab) {
            OnPlacePointUnhighlight?.Invoke(point, grab);
        }

        public void Grab(Hand hand, Grabbable grab) {
            if(grab.HeldCount() == 1) {
                OnFirstGrab?.Invoke(hand, grab);
            }
            if(grab.HeldCount() == 2) {
                OnTwoHandedGrab?.Invoke(hand, grab);
            }
        }

        public void Release(Hand hand, Grabbable grab) {
            if(grab.HeldCount() == 0) {
                OnLastRelease?.Invoke(hand, grab);
            }
            if(grab.HeldCount() == 1) {
                OnTwoHandedRelease?.Invoke(hand, grab);
            }
        }
    }

}