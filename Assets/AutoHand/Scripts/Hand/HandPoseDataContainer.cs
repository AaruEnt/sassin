using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Autohand {
    public class HandPoseDataContainer : MonoBehaviour {
        [AutoHeader("Auto Hand Pose")]
        public bool ignoreMe;
        [HideInInspector, SerializeField]
        public HandPoseData rightPose;
        [HideInInspector]
        public bool rightPoseSet = false;
        [HideInInspector, SerializeField]
        public HandPoseData leftPose;
        [HideInInspector]
        public bool leftPoseSet = false;
        [Tooltip("Purely for organizational purposes in the editor")]
        public string poseName = "";
        public int poseIndex = 0;


#if UNITY_EDITOR
        [HideInInspector]
        public bool showEditorTools = true;
        [HideInInspector, Tooltip("Used to pose for the grabbable")]
        public Hand editorHand;
#endif

        [HideInInspector, Tooltip("Scriptable options NOT REQUIRED (will be saved locally instead if empty) -> Create scriptable throught [Auto Hand/Custom Pose]")]
        public HandPoseScriptable poseScriptable;


        //If the old data type is set but not the new data type
        public bool isDataDepricated {
            get {
                bool isOldRightPoseSet = rightPoseSet && rightPose.posePositions.Length > 0;
                bool isOldLeftPoseSet = leftPoseSet && leftPose.posePositions.Length > 0;
                bool isNewRightPoseSet = !rightPose.isDataDeprecated;
                bool isNewLeftPoseSet = !leftPose.isDataDeprecated;
                return (isOldRightPoseSet && !isNewRightPoseSet) || (isOldLeftPoseSet && !isNewLeftPoseSet);
            }
        }

        public void UpdateDepricatedData(Hand hand) {

            if(hand.left && leftPoseSet) {
                leftPose.UpdateDepricatedData(hand, transform);
            }
            else if(!hand.left && rightPoseSet) {
                rightPose.UpdateDepricatedData(hand, transform);
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }


        /// <summary>Saves/Overwrites the pose data of this grabbable pose to be the given hand relative to this grabbable</summary>
        public virtual void SaveHandPose(Hand hand) {
            if(hand.left)
                leftPose = new HandPoseData(hand, transform);
            else if(!hand.left)
                rightPose = new HandPoseData(hand, transform);
        }

        public virtual ref HandPoseData GetHandPoseData(bool left) {
            if(poseScriptable != null) {
                if(left) 
                    return ref poseScriptable.leftPose;
                else
                    return ref poseScriptable.rightPose;
            }

            if(left) 
                return ref leftPose;
            else 
                return ref rightPose;
        }


        public void SetHandPose(Hand hand) {
            HandPoseData pose;
            if(hand.left) {
                if(leftPoseSet) pose = leftPose;
                else return;
            }
            else {
                if(rightPoseSet) pose = rightPose;
                else return;
            }

            pose.SetPose(hand, transform);
        }

#if UNITY_EDITOR
        [ContextMenu("SAVE RIGHT")]
        public void EditorSavePoseRight() {
            if(editorHand != null)
                EditorSaveGrabPose(editorHand);
            else
                Debug.Log("Editor Hand must be assigned");
        }

        [ContextMenu("SAVE LEFT")]
        public void EditorSavePoseLeft() {
            if(editorHand != null)
                EditorSaveGrabPose(editorHand);
            else
                Debug.Log("Editor Hand must be assigned");
        }

        [ContextMenu("OVERWRITE SCRIPTABLE")]
        public void SaveScriptable() {
            if(poseScriptable != null) {
                if(rightPoseSet)
                    poseScriptable.SaveRightPose(rightPose);
                if(leftPoseSet)
                    poseScriptable.SaveLeftPose(leftPose);
            }
        }

        public void EditorCreateHandCopyTool(Hand hand, Transform relativeTo) {
            Hand handCopy;
            if(hand.name != "HAND COPY DELETE")
                handCopy = Instantiate(hand, relativeTo.transform.position, hand.transform.rotation);
            else
                handCopy = hand;

            handCopy.name = "HAND COPY DELETE";
            var referenceHand = handCopy.gameObject.AddComponent<EditorHand>();
            referenceHand.handPoseDataContainer = this;

            editorHand = handCopy;

            Selection.activeGameObject = editorHand.gameObject;
            SceneView.lastActiveSceneView.FrameSelected();

            if(hand.left && leftPoseSet) {
                leftPose.SetPose(handCopy, transform);
            }
            else if(!hand.left && rightPoseSet) {
                rightPose.SetPose(handCopy, transform);
            }
            else {
                handCopy.transform.position = relativeTo.transform.position;
                editorHand.RelaxHand();
            }

            var contrainer = new GameObject();
            contrainer.name = "HAND COPY CONTAINER DELETE";
            contrainer.transform.position = relativeTo.transform.position;
            contrainer.transform.rotation = relativeTo.transform.rotation;
            handCopy.transform.parent = contrainer.transform;
            if(hand.poseIndex != poseIndex)
                handCopy.RelaxHand();

            if(handCopy.transform.parent.GetComponentInChildren<MeshRenderer>()  == null && handCopy.transform.parent.GetComponentInChildren<SkinnedMeshRenderer>()  == null) {

                foreach(Finger finger in handCopy.fingers) {
                    for(int i = -1; i < finger.FingerJoints.Length; i++) {
                        Transform fingerTransform = null;
                        Transform childFingerTranform = null;
                        if(i == -1) {

                            fingerTransform = finger.FingerJoints[i+1].parent;
                            childFingerTranform = finger.FingerJoints[i+1];
                        }
                        else if(i < finger.FingerJoints.Length-1) {
                            fingerTransform = finger.FingerJoints[i];
                            childFingerTranform = finger.FingerJoints[i+1];
                        }
                        else if(finger.FingerJoints[i].childCount > 0) {
                            fingerTransform = finger.FingerJoints[i];
                            childFingerTranform = finger.tip;
                        }

                        if(childFingerTranform == null || fingerTransform == null)
                            continue;

                        float distance = Vector3.Distance(fingerTransform.position, childFingerTranform.position);
                        Vector3 direction = (fingerTransform.position - childFingerTranform.position).normalized;

                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = childFingerTranform.position + direction * (distance / 2);  // Offset in direction of bone
                        cube.transform.localScale = new Vector3(finger.tipRadius*2, distance+finger.tipRadius, finger.tipRadius*2);  // Scale based on bone length
                        cube.transform.up = direction;  // Orient cube in direction of bone
                        cube.transform.parent = fingerTransform;
                    }
                }

            }

            Undo.RegisterCreatedObjectUndo(contrainer, "Create Hand Copy Container");

            EditorGUIUtility.PingObject(handCopy);
            SceneView.lastActiveSceneView.FrameSelected();
        }

        public void EditorSaveGrabPose(Hand hand) {
            var pose = new HandPoseData(hand, transform);

            if(hand.left) {
                leftPose = pose;
                leftPoseSet = true;
                Debug.Log("Pose Saved - Left");
                if(poseScriptable != null)
                    if(!poseScriptable.leftSaved)
                        poseScriptable.SaveLeftPose(leftPose);
            }
            else {
                rightPose = pose;
                rightPoseSet = true;
                Debug.Log("Pose Saved - Right");
                if(poseScriptable != null)
                    if(!poseScriptable.rightSaved)
                        poseScriptable.SaveRightPose(rightPose);
            }
        }

        public void EditorClearPoses() {
            leftPoseSet = false;
            leftPose = new HandPoseData();
            rightPoseSet = false;
            rightPose = new HandPoseData();
        }
#endif

        public bool HasPose(bool left) {
            if(poseScriptable != null && ((left) ? poseScriptable.leftSaved : poseScriptable.rightSaved))
                return (left) ? poseScriptable.leftSaved : poseScriptable.rightSaved;
            return left ? leftPoseSet : rightPoseSet;
        }

        protected virtual void OnDrawGizmosSelected() {
            Transform targetTransform = null;
            if(this is GrabbablePose)
                targetTransform = transform;
            if(rightPoseSet && poseIndex == 0)
                GrabbablePoseGizmo.DrawHandGizmo(rightPose, targetTransform, Color.blue/2f);
            if(leftPoseSet)
                GrabbablePoseGizmo.DrawHandGizmo(leftPose, targetTransform, Color.red/2f);
        }
    }
}