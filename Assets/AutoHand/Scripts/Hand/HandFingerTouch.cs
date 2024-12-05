using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {
    public class HandFingerTouch
        : MonoBehaviour {
        [Tooltip("Reference to the Hand component")]
        public Hand hand;

        [Tooltip("Reference to the Index Finger component")]
        public Finger index;
        [Tooltip("Reference to the Middle Finger component")]
        public Finger middle;
        [Tooltip("Reference to the Ring Finger component")]
        public Finger ring;
        [Tooltip("Reference to the Pinky Finger component")]
        public Finger pinky;
        [Tooltip("Reference to the Thumb component")]
        public Finger thumb;

        [Tooltip("Layer mask for determining what objects the fingers can interact with")]
        public LayerMask touchMask;

        [Tooltip("Speed at which finger offset adjusts - These default settings are tuned for the default hands physics settings and results might vary based on hands set follow speed and rigidbody settings\"")]
        public float offsetSpeed = 0.65f;

        [Tooltip("Maximum offset for the index finger")]
        public float maxIndexOffset = 0.5f;
        [Tooltip("Maximum offset for the middle finger")]
        public float maxMiddleOffset = 0.5f;
        [Tooltip("Maximum offset for the ring finger")]
        public float maxRingOffset = 0.5f;
        [Tooltip("Maximum offset for the pinky finger")]
        public float maxPinkyOffset = 0.5f;
        [Tooltip("Maximum offset for the thumb")]
        public float maxThumbOffset = 0.5f;

        [Tooltip("Minimum pressure required for interaction - These default settings are tuned for the default hands physics settings and results might vary based on hands set follow speed and rigidbody settings")]
        public float minPressure = 300;
        [Tooltip("Maximum pressure for interaction - These default settings are tuned for the default hands physics settings and results might vary based on hands set follow speed and rigidbody settings")]
        public float maxPressure = 1200;

        [Tooltip("Curve to represent the relationship between pressure applied and finger bend")]
        public AnimationCurve pressureBendCurve = AnimationCurve.Linear(0, 0, 1, 1);


        // Internal fields for handling finger touch and pressure
        private float fingerRadiusMultiplier = 4f;
        private float[] maxOffsets;
        private float[] currentPressure;
        private bool[] isTouching;
        private Vector3 largeSphereCheckerPoint;
        private float largeSphereCheckerRadius;
        private Collider[] collidersNonAlloc = new Collider[100];
        private Vector3[] fingerTips = new Vector3[5];
        private Finger[] fingers = new Finger[5];
        private Vector3 smoothAngularVelocity;



        /// <summary>
        /// Returns a value between 0 and 1 representing the current pressure applied to the finger
        /// </summary>
        public float GetCurrentFingerPressure(FingerEnum finger) {
            return currentPressure[(int)finger];
        }




        void Awake() {
            fingerTips[0] = (hand.transform.InverseTransformPoint(index.tip.position));
            fingerTips[1] =(hand.transform.InverseTransformPoint(middle.tip.position));
            fingerTips[2] =(hand.transform.InverseTransformPoint(ring.tip.position));
            fingerTips[3] =(hand.transform.InverseTransformPoint(pinky.tip.position));
            fingerTips[4] =(hand.transform.InverseTransformPoint(thumb.tip.position));

            fingers[0] = (index);
            fingers[1] =(middle);
            fingers[2] =(ring);
            fingers[3] =(pinky);
            fingers[4] =(thumb);

            maxOffsets = new float[] { maxIndexOffset, maxMiddleOffset, maxRingOffset, maxPinkyOffset, maxThumbOffset };
            currentPressure = new float[] { 0, 0, 0, 0, 0 };
            isTouching = new bool[] { false, false, false, false, false };

            CreateEncapsulatingSphere(fingerTips, out largeSphereCheckerPoint, out largeSphereCheckerRadius);
            largeSphereCheckerRadius += index.tipRadius;
            largeSphereCheckerRadius += middle.tipRadius;
        }


        void OnEnable() {
            StartCoroutine(SlowUpdateCoroutine());
        }


        void OnDisable() {
            StopAllCoroutines();
        }


        void LateUpdate() {
            MoveTowardsBendTarget();
        }


        IEnumerator SlowUpdateCoroutine() {
            while (true) {
                CheckFingerTouch();
                yield return new WaitForFixedUpdate();
            }
        }


        void MoveTowardsBendTarget() {
            for(int i = 0; i < fingers.Length; i++) {
                if(fingers[i].secondaryOffset == 0 && currentPressure[i] == 0)
                    continue;

                float currentTarget = currentPressure[i]*maxOffsets[i];
                float targetDistance = Mathf.Abs(currentTarget - fingers[i].secondaryOffset)/maxOffsets[i];
                targetDistance = Mathf.Pow(targetDistance, 2f);
                float distanceSpeed = Time.deltaTime * offsetSpeed * targetDistance;

                currentTarget = Mathf.Clamp(currentTarget, -maxOffsets[i], maxOffsets[i]);

                fingers[i].secondaryOffset = Mathf.MoveTowards(fingers[i].secondaryOffset, currentTarget, distanceSpeed + Time.deltaTime * offsetSpeed/2f);
                if(fingers[i].GetCurrentBend() < 0) {
                    fingers[i].secondaryOffset += -fingers[i].GetCurrentBend();
                }
            }
        }


        void CheckFingerTouch() {
            //var angularVelocity = CalculateCustomAngularVelocity(hand.transform.rotation, hand.moveTo.transform.rotation, 2000f);
            var targetAngularVelocity = !hand.holdingObj ? hand.handFollow.lastAngularVelocity : Vector3.zero;
            //var lerpDelta = Vector3.SqrMagnitude(targetAngularVelocity - smoothAngularVelocity) * Time.fixedDeltaTime * 1200f + Time.fixedDeltaTime * 360f;
            var lerpDelta =  Time.fixedDeltaTime * 1200f;
            //smoothAngularVelocity = Vector3.Lerp(smoothAngularVelocity, targetAngularVelocity, lerpDelta);
            smoothAngularVelocity = targetAngularVelocity;//Vector3.MoveTowards(smoothAngularVelocity, targetAngularVelocity, lerpDelta);

            //Check for overlap with large sphere
            int numColliders = Physics.OverlapSphereNonAlloc(hand.transform.TransformPoint(largeSphereCheckerPoint), largeSphereCheckerRadius, collidersNonAlloc, touchMask, QueryTriggerInteraction.Ignore);
            if (numColliders > 0) {
                //Check for overlap with each finger
                for(int i = 0; i < fingerTips.Length; i++) {

                    fingerTips[i] = hand.transform.InverseTransformPoint(fingers[i].tip.position);

                    Vector3 fingerTipWorld = hand.transform.TransformPoint(fingerTips[i]);
                    float radius = fingers[i].tipRadius*fingerRadiusMultiplier*Mathf.Abs(hand.transform.lossyScale.x) * (isTouching[i] ? 2f : 1f);
                    int numCollidersFinger = Physics.OverlapSphereNonAlloc(fingerTipWorld, radius, collidersNonAlloc, touchMask, QueryTriggerInteraction.Ignore);
                    if (numCollidersFinger > 0) {
                        var pressure = CalculateFingerTipPressure(fingerTipWorld, hand.transform.position, smoothAngularVelocity, hand.palmTransform.forward, out bool isForceTop, fingers[i].tipRadius);
                        var evaluatedPressure = pressureBendCurve.Evaluate((pressure-minPressure)/maxPressure);

                        isTouching[i] = true;
                        if(pressure < minPressure)
                            continue;

                        //Difference between currentPressure[i] and evaluatedPressure
                        float pressureDifference = Mathf.Abs(currentPressure[i] - evaluatedPressure);
                        pressureDifference = Mathf.Pow(pressureDifference, 2f);


                        currentPressure[i] = Mathf.MoveTowards(currentPressure[i], evaluatedPressure * (isForceTop ? -1 : 1), pressureDifference * Time.fixedDeltaTime * 30f + Time.fixedDeltaTime * 5f);

                    }
                    else {
                        currentPressure[i] =  Mathf.MoveTowards(currentPressure[i], 0, maxOffsets[i]*Time.fixedDeltaTime*30f);
                        isTouching[i] = false;
                    }
                }
            }
            else {
                for(int i = 0; i < fingers.Length; i++) {
                    isTouching[i] = false;
                    if(currentPressure[i] == 0)
                        continue;
                    currentPressure[i] = Mathf.MoveTowards(currentPressure[i], 0, maxOffsets[i]*Time.fixedDeltaTime*30f);
                }
            }
        }

       
        
        Vector3 CalculateCustomAngularVelocity(Quaternion currentRotation, Quaternion targetRotation, float maxAngularVelocity) {
            Quaternion rotationDifference = targetRotation * Quaternion.Inverse(currentRotation);
            var eularAngularVelocity = rotationDifference.eulerAngles;

            return eularAngularVelocity;
        }

        float CalculateFingerTipPressure(Vector3 fingertipPoint, Vector3 wristPoint, Vector3 angularVelocity, Vector3 palmDirection, out bool isForceOnTop, float fingertipArea) {
            Vector3 leverArm = fingertipPoint - wristPoint;
            Vector3 torque = Vector3.Cross(leverArm, angularVelocity);

            float forceMagnitude = torque.magnitude / leverArm.magnitude;
            float pressure = forceMagnitude / fingertipArea;

            Vector3 relativePoint;
            if(float.IsNaN(angularVelocity.x) || float.IsNaN(angularVelocity.y) || float.IsNaN(angularVelocity.z))
                relativePoint = fingertipPoint + hand.body.velocity*Time.fixedDeltaTime;
            else 
                relativePoint = Quaternion.Euler(angularVelocity)*leverArm + hand.body.velocity*Time.fixedDeltaTime;

            var plane = new Plane(palmDirection, Vector3.zero);
            isForceOnTop = plane.GetSide(relativePoint);

            return pressure;
        }



        void CreateEncapsulatingSphere(Vector3[] fingerTips, out Vector3 sphereCenter, out float sphereRadius) {
            sphereCenter = new Vector3();

            foreach(var point in fingerTips) {
                sphereCenter += point;
            }
            sphereCenter /= fingerTips.Length;

            sphereRadius = 0f;
            foreach(var point in fingerTips) {
                float distance = Vector3.Distance(sphereCenter, point);
                sphereRadius = Mathf.Max(sphereRadius, distance);
            }
        }

    }
}