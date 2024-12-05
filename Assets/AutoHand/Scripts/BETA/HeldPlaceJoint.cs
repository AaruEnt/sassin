using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {
    public class HeldPlaceJoint : PlacePoint {

        [AutoHeader("Held Place Joint")]
        public bool ignoreMe1;
        public Grabbable connectedGrabbable;
        public Vector3 pivotOffset;
        public float heldMassScale = 1f;
        [Space]
        public float xMaxLimit = 0f;
        public float xMinLimit = 0f;
        public float xSpring = 0f;
        [Space]
        public float yMaxLimit = 0f;
        public float yMinLimit = 0f;
        public float ySpring = 0f;
        [Space]
        public float zMaxLimit = 0f;
        public float zMinLimit = 0f;
        public float zSpring = 0f;

        [Space]
        [Range(0, 1), Tooltip("The percentage from the min/max distance needed to trigger the event, good for creating a buffer for the event to trigger slightly before the max range to help prevent missed event")]
        public float eventOffset = 0.05f;
        public UnityHandGrabEvent OnMinDistanceEvent;
        public UnityHandGrabEvent OnMaxDistanceEvent;

        GrabbableHeldJoint heldJoint = null;


        protected  void Start() {
            //base.Start();
            //heldPlaceOnly = true;
            disableRigidbodyOnPlace = false;
            parentOnPlace = true;
            forceHandRelease = false;
            makePlacedKinematic = false;
            forcePlace = true;
            
        }

        public override bool CanPlace(Grabbable placeObj, bool checkRoot = true) {
            if(placeObj.body == connectedGrabbable.body)
                return false;

            return base.CanPlace(placeObj);
        }


        public override void Place(Grabbable placeObj) {

            Debug.Log("Place");
            Dictionary<Hand, Transform> grabPoint = new Dictionary<Hand, Transform>();
            var hands = new Hand[(placeObj.GetHeldBy().Count)];
            placeObj.GetHeldBy().CopyTo(hands, 0);

            placeObj.transform.position = placedOffset.position;
            placeObj.transform.rotation = placedOffset.rotation;

            foreach(var hand in hands) {
                grabPoint.Add(hand, hand.handGrabPoint);
            }

            base.Place(placeObj);

            foreach(var hand in hands) {
                //hand.BreakGrabConnection();
                //hand.CreateGrabConnection(placeObj, grabPoint[hand].position, grabPoint[hand].rotation, placeObj.transform.position, placeObj.transform.rotation, true);
                hand.heldJoint.connectedBody = connectedGrabbable.body;
                hand.heldJoint.connectedAnchor = connectedGrabbable.body.transform.InverseTransformPoint(hand.handGrabPoint.position);
            }

            placeObj.DeactivateRigidbody();
            placeObj.transform.parent = connectedGrabbable.body.transform;
            placeObj.body = connectedGrabbable.body;

            if(heldJoint == null) {
                heldJoint = placeObj.gameObject.AddComponent<GrabbableHeldJoint>();
                heldJoint.connectedGrabbable = connectedGrabbable;
                heldJoint.xMaxLimit = xMaxLimit;
                heldJoint.xMinLimit = xMinLimit;
                heldJoint.xSpring = xSpring;

                heldJoint.yMaxLimit = yMaxLimit;
                heldJoint.yMinLimit = yMinLimit;
                heldJoint.ySpring = ySpring;

                heldJoint.zMaxLimit = zMaxLimit;
                heldJoint.zMinLimit = zMinLimit;
                heldJoint.zSpring = zSpring;

                heldJoint.eventOffset = eventOffset;
                heldJoint.OnMaxDistanceEvent = OnMaxDistanceEvent;
                heldJoint.OnMinDistanceEvent = OnMinDistanceEvent;
                heldJoint.Start();
                if(hands.Length > 0)
                    heldJoint.OnGrabbed(hands[0], placeObj);

                heldJoint.UpdateJoint();
            }
        }

        public override void Remove(Grabbable placeObj) {
            Debug.Log("Remove");
            if(heldJoint != null) {
                Destroy(heldJoint);
                heldJoint = null;
            }

            base.Remove(placeObj);

        }
    }
}

