
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {

    public enum FingerEnum {
        none = -1,
        index,
        middle,
        ring,
        pinky,
        thumb
    }

    public enum FingerJointEnum {
        knuckle,
        middle,
        distal,
        tip
    }

    //You can add more poses here and they will automatically be added to the hands save pose button list
    //Dont change the order of the poses, only add new ones to the end
    public enum FingerPoseEnum {
        Open = 0,
        Closed = 1,
        PinchOpen = 2,
        PinchClosed = 3,
        TotalPoses
    }

    [System.Serializable]
    public struct FingerMask {
        public FingerEnum finger;
        public float weight;
    }



    [HelpURL("https://app.gitbook.com/s/5zKO0EvOjzUDeT2aiFk3/auto-hand/hand/finger-component")]
    public class Finger : MonoBehaviour {
        [Header("Hand Reference")]
        public Hand hand;
        [Header("Finger Joints")]
        public FingerEnum fingerType = FingerEnum.none;
        [Tooltip("This is the first joint on the finger, the knuckle that rotates the whole finger, on the thumb this should be joint closer the wrist (The first joint on your actual thumb that moves)")]
        public Transform knuckleJoint;
        [Tooltip("The second joint that connect the primary finger bone to the middle finger bone")]
        public Transform middleJoint;
        [Tooltip("The third that attached the middle finger bone and the top bone")]
        public Transform distalJoint;
        [Space]
        [Header("Finger Tip")]
        [Tooltip("This transfrom will represent the tip/stopper of the finger")]
        public Transform tip;
        [Tooltip("This determines the radius of the spherecast check when bending fingers")]
        public float tipRadius = 0.01f;
        [Tooltip("This will offset the fingers bend (0 is no bend, 1 is full bend)")]
        [Range(0, 1f)]
        public float bendOffset;
        public float fingerSmoothSpeed = 1;

        [HideInInspector]
        public float secondaryOffset = 0;

        public Transform[] FingerJoints {
            get {
                if(fingerJoints == null || fingerJoints.Length == 0)
                    fingerJoints = new Transform[] { knuckleJoint, middleJoint, distalJoint, tip };

                return fingerJoints;
            }
        }

        [SerializeField, HideInInspector]
        Transform[] fingerJoints;

        [SerializeField, HideInInspector]
        public FingerPoseData[] poseData;



        FingerPoseData _poseDataNonAlloc;
        public FingerPoseData poseDataNonAlloc {
            get {
                if(!_poseDataNonAlloc.isSet) {
                    _poseDataNonAlloc = new FingerPoseData();
                    _poseDataNonAlloc.poseRelativeMatrix = new Matrix4x4[4];
                    _poseDataNonAlloc.localRotations = new Quaternion[4];
                }
                return _poseDataNonAlloc;
            }
        }

        float bend = 0;


        //DEPRICATED FINGER POSE DATA
        [SerializeField]
        [HideInInspector]
        internal Quaternion[] minGripRotPose;

        [SerializeField]
        [HideInInspector]
        internal Vector3[] minGripPosPose;

        [SerializeField]
        [HideInInspector]
        internal Quaternion[] maxGripRotPose;

        [SerializeField]
        [HideInInspector]
        internal Vector3[] maxGripPosPose;

        public bool isMissingReferences { get { return knuckleJoint == null || middleJoint == null || distalJoint == null || tip == null; } }

        public bool isDataDepricated {
            get {
                return poseData == null || poseData.Length == 0 || (minGripPosPose.Length > 0 && poseData[(int)FingerPoseEnum.Open].isSet == false) || (maxGripPosPose.Length > 0 && poseData[(int)FingerPoseEnum.Closed].isSet == false);
            }
        }

        float lastHitBend;
        Collider[] results = new Collider[4];


        protected virtual void Awake() {
            if(hand == null)
                hand = GetComponentInParent<Hand>();
            if(hand == null)
                Debug.LogError("AUTO HAND: Missing hand reference, please assign the hand reference to the finger component", this);

            if(isDataDepricated)
                UpdateDepricatedValues();

            for(int i = 0; i < poseData.Length; i++)
                if(poseData[i].isSet)
                    poseData[i].CalculateAdditionalValues(hand.transform.lossyScale);


            if((knuckleJoint == null || middleJoint == null || distalJoint == null || tip == null))
                Debug.LogError("AUTO HAND: Missing finger connections, please connect all the joint values of the finger component (If your finger has less than the required joints add the same joint to two inputs)", this);
            if(poseData == null || poseData.Length == 0)
                Debug.LogError("AUTO HAND: Missing finger pose data, please set the open and closed finger pose data (If you have not set this up please do so in the inspector or use the context menu to set the open and closed finger pose data)", this);

        }




        /// <summary>Forces the finger to a bend until it hits something on the given physics layer</summary>
        /// <param name="steps">The number of steps and physics checks it will make lerping from 0 to 1</param>
        public virtual bool BendFingerUntilNoHit(int steps, int layermask, FingerPoseEnum fromPose = FingerPoseEnum.Open, FingerPoseEnum toPose = FingerPoseEnum.Closed) {
            return BendFingerUntilNoHit(steps, layermask, ref poseData[(int)fromPose], ref poseData[(int)toPose]);
        }

        /// <summary>Forces the finger to a bend until it hits something on the given physics layer</summary>
        /// <param name="steps">The number of steps and physics checks it will make lerping from 0 to 1</param>
        public virtual bool BendFingerUntilNoHit(int steps, int layermask, ref FingerPoseData fromPose, ref FingerPoseData toPose) {
            lastHitBend = 0;
            var fingerTipTransform = tip.transform;
            var handTransform = hand.transform;
            var handPosition = handTransform.position;
            var handRotation = handTransform.rotation;

            Vector3 lastFingerPos = Vector3.zero;
            for(float i = 0; i <= steps / 5f; i++) {
                lastHitBend = i / (steps / 5f);
                int overlapCount = CheckFingerBlendOverlap(layermask, handRotation, fingerTipTransform, ref fromPose, ref toPose, lastHitBend);

                if(overlapCount == 0) {
                    lastHitBend = Mathf.Clamp01(lastHitBend);
                    if(i == 0) {
                        poseDataNonAlloc.SetFingerPose(this, handRotation, knuckleJoint, middleJoint, distalJoint);
                        bend = lastHitBend;
                        //currBendOffset = lastHitBend;
                        return true;
                    }
                    break;
                }

            }

            lastHitBend -= (5f / steps);
            for(int i = 0; i <= steps / 10f; i++) {
                lastHitBend += (1f / steps);
                lastHitBend = Mathf.Clamp01(lastHitBend);
                int overlapCount = CheckFingerBlendOverlap(layermask, handRotation, fingerTipTransform, ref fromPose, ref toPose, lastHitBend);

                if(overlapCount == 0 || lastHitBend >= 1) {
                    bend = lastHitBend;
                    //currBendOffset = lastHitBend;
                    return true;
                }
            }

            lastHitBend = 1f;
            toPose.SetFingerPose(this, handRotation, knuckleJoint, middleJoint, distalJoint);
            return false;
        }



        /// <summary>Forces the finger to a bend until it hits something on the given physics layer</summary>
        /// <param name="steps">The number of steps and physics checks it will make lerping from 0 to 1</param>
        public virtual bool BendFingerUntilHit(int steps, int layermask, FingerPoseEnum fromPose = FingerPoseEnum.Open, FingerPoseEnum toPose = FingerPoseEnum.Closed) {
            return BendFingerUntilHit(steps, layermask, ref poseData[(int)fromPose], ref poseData[(int)toPose]);
        }

        /// <summary>Forces the finger to a bend until it hits something on the given physics layer</summary>
        /// <param name="steps">The number of steps and physics checks it will make lerping from 0 to 1</param>
        public virtual bool BendFingerUntilHit(int steps, int layermask, ref FingerPoseData fromPose, ref FingerPoseData toPose) {
            lastHitBend = 0;
            var fingerTipTransform = tip.transform;
            var handRotation = hand.transform.rotation;
            for(float i = 0; i <= steps / 5f; i++) {
                lastHitBend = i / (steps / 5f);
                int overlapCount = CheckFingerBlendOverlap(layermask, handRotation, fingerTipTransform, ref fromPose, ref toPose, lastHitBend);

                if(overlapCount > 0) {
                    lastHitBend = Mathf.Clamp01(lastHitBend);
                    if(i == 0) {
                        poseDataNonAlloc.SetFingerPose(this, handRotation, knuckleJoint, middleJoint, distalJoint);
                        bend = lastHitBend;
                        return true;
                    }
                    break;
                }

            }

            lastHitBend -= (5f / steps);
            for(int i = 0; i <= steps / 10f; i++) {
                lastHitBend += (1f / steps);
                lastHitBend = Mathf.Clamp01(lastHitBend);
                int overlapCount = CheckFingerBlendOverlap(layermask, handRotation, fingerTipTransform, ref fromPose, ref toPose, lastHitBend);

                if(overlapCount > 0 || lastHitBend >= 1) {
                    bend = lastHitBend;
                    //currBendOffset = lastHitBend;
                    return true;
                }
            }

            lastHitBend = 1f;
            toPose.SetFingerPose(this, handRotation, knuckleJoint, middleJoint, distalJoint); ;
            return false;
        }

        public virtual int CheckFingerBlendOverlap(int layermask, Quaternion handRotation, Transform fingerTipTransform, ref FingerPoseData fromPose, ref FingerPoseData toPose, float point) {
            poseDataNonAlloc.LerpData(ref fromPose, ref toPose, point, false);
            poseDataNonAlloc.SetFingerPose(this, handRotation, knuckleJoint, middleJoint, distalJoint);
            return Physics.OverlapSphereNonAlloc(fingerTipTransform.position, tipRadius, results, layermask, QueryTriggerInteraction.Ignore);
        }






        public virtual void UpdateFingerPose(float bend, FingerPoseEnum fromPose = FingerPoseEnum.Open, FingerPoseEnum toPose = FingerPoseEnum.Closed) { 
            UpdateFingerPose(bend, ref poseData[(int)fromPose], ref poseData[(int)toPose]);
        }

        public virtual void UpdateFingerPose(float bend, ref FingerPoseData fromPose, ref FingerPoseData toPose) {
            this.bend = bend;

            poseDataNonAlloc.LerpData(ref fromPose, ref toPose, bend);
            poseDataNonAlloc.SetFingerPose(this);
        }



        public virtual void SetFingerBend(float bend, ref FingerPoseData fromPose, ref FingerPoseData toPose) {
            this.bend = bend;
            poseDataNonAlloc.LerpData(ref fromPose, ref toPose, bend);
            poseDataNonAlloc.SetFingerPose(this);
        }

        /// <summary>Forces the finger to a bend ignoring physics and offset</summary>
        /// <param name="bend">0 is no bend / 1 is full bend</param>
        public virtual void SetFingerBend(float bend, FingerPoseEnum fromPose = FingerPoseEnum.Open, FingerPoseEnum toPose = FingerPoseEnum.Closed) {
            this.bend = bend;
            SetFingerBend(bend, ref poseData[(int)fromPose], ref poseData[(int)toPose]);
        }




        /// <summary>Sets the current finger to a bend without interfering with the target</summary>
        /// <param name="bend">0 is no bend / 1 is full bend</param>
        //public void SetCurrentFingerBend(float bend, FingerPoseEnum fromPose = FingerPoseEnum.Open, FingerPoseEnum toPose = FingerPoseEnum.Closed) {
        //    currBendOffset = bend;

        //    var openPose = poseData[(int)fromPose];
        //    var closedPose = poseData[(int)toPose];

        //    poseDataNonAlloc.LerpData(ref openPose, ref closedPose, bend);
        //    poseDataNonAlloc.SetFingerPose(this);
        //}



        [ContextMenu("Open")]
        public virtual void ResetBend() {
            var openPose = poseData[(int)FingerPoseEnum.Open];
            openPose.SetFingerPose(this);
        }

        [ContextMenu("Close")]
        public virtual void Grip() {
            var closedPose = poseData[(int)FingerPoseEnum.Closed];
            closedPose.SetFingerPose(this);
        }


        /// <summary>Returns the bend the finger ended with from the last BendFingerUntilHit() call</summary>
        public float GetLastHitBend() {
            return lastHitBend;
        }

        /// <summary>Saves the current pose the finger is taking to the given pose type</summary>
        public virtual void SavePose(Hand hand, Finger finger, FingerPoseEnum poseType) {
            if(poseData == null)
                poseData = new FingerPoseData[(int)FingerPoseEnum.TotalPoses];

            if(poseData.Length != (int)FingerPoseEnum.TotalPoses) {
                var oldData = poseData;
                poseData = new FingerPoseData[(int)FingerPoseEnum.TotalPoses];
                for(int i = 0; i < oldData.Length; i++) {
                    poseData[i] = oldData[i];
                }
            }

            Debug.Log("Pose Type: " + poseType + " - " + (int)poseType + " - Length: " + poseData.Length);

            if(!poseData[(int)poseType].isSet)
                poseData[(int)poseType] = new FingerPoseData(hand, finger);
            else
                poseData[(int)poseType].SetPoseData(hand, finger);
        }

        public virtual void SavePose(ref FingerPoseData fingerPoseData, FingerPoseEnum poseType) {
            if(!poseData[(int)poseType].isSet)
                poseData[(int)poseType] = new FingerPoseData(ref fingerPoseData);
            else
                poseData[(int)poseType].CopyFromData(ref fingerPoseData);
        }

        [ContextMenu("SAVE - Open Pose")]
        public void SaveOpenPose() {
            SavePose(hand, this, FingerPoseEnum.Open);
        }

        [ContextMenu("SAVE - Closed Pose")]
        public void SaveClosedPose() {
            SavePose(hand, this, FingerPoseEnum.Closed);
        }

        [ContextMenu("SAVE - Pinch Open Pose")]
        public void SavePinchOpenPose() {
            SavePose(hand, this, FingerPoseEnum.PinchOpen);
        }

        [ContextMenu("SAVE - Pinch Closed Pose")]
        public void SavePinchClosedPose() {
            SavePose(hand, this, FingerPoseEnum.PinchClosed);
        }


        /// <summary>Copies the pose data from the given finger to this finger</summary>
        public virtual void CopyPoseData(Finger finger) {
            for(int i = 0; i < finger.poseData.Length; i++) {
                if(poseData[i].isSet)
                    poseData[i].CopyFromData(finger.poseData[i]);
                else
                    poseData[i] = new FingerPoseData(ref finger.poseData[i]);
            }

        }

        /// <summary>Checks if the given pose type has been saved</summary>
        public virtual bool IsPoseSaved(FingerPoseEnum poseType) {
            if((int)poseType >= poseData.Length)
                return false;

            return (poseData != null && poseData.Length != 0) && poseData[(int)poseType].poseRelativeMatrix != null && poseData[(int)poseType].poseRelativeMatrix.Length > 0;
        }

        public virtual float GetCurrentBend() {
            bendOffset = Mathf.Clamp(bendOffset, 0, 1);
            return bendOffset+secondaryOffset;
        }


        private void OnDrawGizmos() {
            if(tip == null)
                return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(tip.transform.position, tipRadius);
        }


        public bool UpdateDepricatedValues() {

            try {
                if(hand == null)
                    hand = GetComponentInParent<Hand>();
                if(hand == null)
                    Debug.LogError("AUTO HAND: Missing hand reference, please assign the hand reference to the finger component", this);

                //Find enum type
                if(fingerType == FingerEnum.none) {
                    int fingerIndex = -1;
                    for(int i = 0; i < 5; i++) {
                        string enumFingerNames = Enum.GetName(typeof(FingerEnum), (FingerEnum)i).ToLower();
                        if(name.ToLower().Contains(enumFingerNames)) {
                            fingerIndex = i;
                            fingerType = (FingerEnum)fingerIndex;
                            break;
                        }
                    }

                    if(fingerIndex == -1) {
                        Debug.LogError("AUTO HAND: Could not find finger type in name, please set the finger type manually", this);
                        return false;
                    }
                }


                bool missingSetup = (knuckleJoint == null || middleJoint == null || distalJoint == null || tip == null);
                //Set up finger joints if none
                if(missingSetup && fingerType != FingerEnum.none) {
                    //If it has just 4 points we assume it's knuck->middle->distle->tip it doesn't have any additional joints
                    if(fingerJoints.Length == 3) {
                        if(knuckleJoint == null)
                            knuckleJoint = fingerJoints[0];
                        if(middleJoint == null)
                            middleJoint = fingerJoints[1];
                        if(distalJoint == null)
                            distalJoint = fingerJoints[2];
                    }
                    else {
                        Debug.LogError("AUTO HAND: Could not find correct finger joint, unsure about automatic setup, place connected finger values manually", this);
                        return false;
                    }
                }

                missingSetup = (knuckleJoint == null || middleJoint == null || distalJoint == null || tip == null);
                if(missingSetup) {
                    Debug.LogError("AUTO HAND: Missing finger connections, please connect all the joint values of the finger component (If your finger has less than the required joints add the same joint to two inputs)", this);
                    return false;
                }

                if(fingerJoints.Length > 0 && minGripPosPose.Length == fingerJoints.Length && maxGripPosPose.Length == fingerJoints.Length) {

                    var knuckleJointIndex = -1;
                    var middleJointIndex = -1;
                    var distalJointIndex = -1;

                    for(int i = 0; i < fingerJoints.Length; i++) {
                        if(fingerJoints[i] == knuckleJoint)
                            knuckleJointIndex = i;
                        if(fingerJoints[i] == middleJoint)
                            middleJointIndex = i;
                        if(fingerJoints[i] == distalJoint)
                            distalJointIndex = i;
                    }

                    var fingerPosesEnumCount = Enum.GetValues(typeof(FingerPoseEnum)).Length;
                    if(poseData == null || poseData.Length != fingerPosesEnumCount) {
                        poseData = new FingerPoseData[fingerPosesEnumCount];

                    }

                    var knuckleJointPosition = minGripPosPose[knuckleJointIndex];
                    var middleJointPosition = minGripPosPose[middleJointIndex];
                    var distalJointPosition = minGripPosPose[distalJointIndex];

                    var knuckleJointRotation = minGripRotPose[knuckleJointIndex];
                    var middleJointRotation = minGripRotPose[middleJointIndex];
                    var distalJointRotation = minGripRotPose[distalJointIndex];

                    var knuckleStartPosition = knuckleJoint.localPosition;
                    var middleStartPosition = middleJoint.localPosition;
                    var distalStartPosition = distalJoint.localPosition;

                    var knuckleStartRotation = knuckleJoint.localRotation;
                    var middleStartRotation = middleJoint.localRotation;
                    var distalStartRotation = distalJoint.localRotation;

                    knuckleJoint.localPosition = knuckleJointPosition;
                    middleJoint.localPosition = middleJointPosition;
                    distalJoint.localPosition = distalJointPosition;

                    knuckleJoint.localRotation = knuckleJointRotation;
                    middleJoint.localRotation = middleJointRotation;
                    distalJoint.localRotation = distalJointRotation;

                    poseData[(int)FingerPoseEnum.Open] = new FingerPoseData(hand, this);

                    knuckleJointPosition = maxGripPosPose[knuckleJointIndex];
                    middleJointPosition = maxGripPosPose[middleJointIndex];
                    distalJointPosition = maxGripPosPose[distalJointIndex];

                    knuckleJointRotation = maxGripRotPose[knuckleJointIndex];
                    middleJointRotation = maxGripRotPose[middleJointIndex];
                    distalJointRotation = maxGripRotPose[distalJointIndex];

                    knuckleJoint.localPosition = knuckleJointPosition;
                    middleJoint.localPosition = middleJointPosition;
                    distalJoint.localPosition = distalJointPosition;

                    knuckleJoint.localRotation = knuckleJointRotation;
                    middleJoint.localRotation = middleJointRotation;
                    distalJoint.localRotation = distalJointRotation;

                    poseData[(int)FingerPoseEnum.Closed] = new FingerPoseData(hand, this);

                    knuckleJoint.localPosition = knuckleStartPosition;
                    middleJoint.localPosition = middleStartPosition;
                    distalJoint.localPosition = distalStartPosition;

                    knuckleJoint.localRotation = knuckleStartRotation;
                    middleJoint.localRotation = middleStartRotation;
                    distalJoint.localRotation = distalStartRotation;

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(this);
#endif

                    Debug.LogWarning("AUTO HAND: Automatically updating finger joint data, recommend doing this manually to ensure correct setup", this);
                }


                for(int i = 0; i < poseData.Length; i++) {
                    if(poseData[i].isSet)
                        poseData[i].CalculateAdditionalValues(hand.transform.lossyScale);
                }

                return true;
            }
            catch(System.Exception e) {
                Debug.LogWarning("AUTO HAND: Error updating finger values, please check the finger component for errors or manually redo the hand Open/Closed pose after setting all finger values", this);
                Debug.LogWarning(e);
                return false;
            }
        }
    }
}
