using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {
    public class PlacePointAnimations : InteractionAnimations {
        [Header("Place Point")]
        public PlacePoint placePoint;

        protected override void OnEnable() {
            base.OnEnable();
            if(placePoint == null)
                placePoint = GetComponent<PlacePoint>();
            if(placePoint == null)
                placePoint = GetComponentInParent<PlacePoint>();

            placePoint.OnHighlight.AddListener(StartHighlight);
            placePoint.OnStopHighlight.AddListener(StopHighlight);
            placePoint.OnPlace.AddListener(OnPlace);
            placePoint.OnRemove.AddListener(OnRemove);
        }

        protected override void OnDisable() {
            base.OnDisable();
            placePoint.OnHighlight.RemoveListener(StartHighlight);
            placePoint.OnStopHighlight.RemoveListener(StopHighlight);
            placePoint.OnPlace.RemoveListener(OnPlace);
            placePoint.OnRemove.RemoveListener(OnRemove);
        }

        protected override void LateUpdate() {
            if(!placePoint.enabled)
                return;

            base.LateUpdate();
        }

        void StartHighlight(PlacePoint placePoint, Grabbable grabbable) {
            Highlight();
        }

        void StopHighlight(PlacePoint placePoint, Grabbable grabbable) {
            Unhighlight();
        }

        void OnPlace(PlacePoint placePoint, Grabbable grabbable) {
            Activate();
        }

        void OnRemove(PlacePoint placePoint, Grabbable grabbable) {
            Deactivate();
        }
    }
}
