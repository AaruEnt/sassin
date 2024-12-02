using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Autohand {
    [CustomEditor(typeof(Hand)), CanEditMultipleObjects]
    public class HandEditor : Editor {

        public HandEditor(){ }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            Hand hand = target as Hand;

            if(GUILayout.Button("Convert Hand Left/Right")) {
                InvertHandData(hand);
            }

            if(GUILayout.Button("Copy Hand Data")) {
                if(hand.copyFromHand != null) {
                    for(int i = 0; i < hand.copyFromHand.fingers.Length; i++) {
                        if(hand.fingers[i] == null) 
                            continue;
                        
                        hand.fingers[i].CopyPoseData(hand.copyFromHand.fingers[i]);
                        EditorUtility.SetDirty(hand.fingers[i]);
                    }
                    Debug.Log("Auto Hand: Copied Hand Pose!");
                }
            }

            var fingerEnumNames = System.Enum.GetNames(typeof(FingerPoseEnum));
            var fingerEnumValues = System.Enum.GetValues(typeof(FingerPoseEnum));

            for(int i = 0; i < fingerEnumNames.Length-1; i++) {
                var pose = (FingerPoseEnum)fingerEnumValues.GetValue(i);

                //If pose is set button should be green
                GUI.backgroundColor = IsPoseSaved(hand, pose) ? Color.green : Color.red;


                if(hand.fingers != null) {
                    foreach(var finger in hand.fingers) {
                        if(finger == null)
                            continue;
                        if(finger.isMissingReferences) {
                            EditorGUILayout.HelpBox("Finger " + finger.name + " is missing references", MessageType.Error);
                            return;
                        }
                        else if(finger.isDataDepricated) {
                            finger.UpdateDepricatedValues();
                        }
                    }
                }

                if(GUILayout.Button("Save " + fingerEnumNames[i] + " Pose")) {
                    if(hand.fingers == null || hand.fingers.Length == 0) {
                        EditorGUILayout.HelpBox("Fingers not initalized", MessageType.Error);
                    }
                    else {
                        foreach(var finger in hand.fingers) {
                            if(finger == null)
                                continue;
                            finger.SavePose(hand, finger, pose);
                            EditorUtility.SetDirty(finger);
                            Debug.Log($"Saving pose {pose} for finger {finger.name}");
                        }
                    }
                }
            }

        }

        public void InvertHandData(Hand hand) {
            hand.transform.localScale = new Vector3(-hand.transform.localScale.x, hand.transform.localScale.y, hand.transform.localScale.z);
            foreach(var finger in hand.fingers) {
                if(finger == null)
                    continue;

                for(int i = 0; i < finger.poseData.Length; i++) {
                    if(finger.poseData[i].poseRelativeMatrix.Length == 0)
                        continue;       

                    finger.poseData[i].poseRelativeMatrix[0].m00 *= -1;
                }
            }
        }

        public bool IsPoseSaved(Hand hand, FingerPoseEnum poseType) {
            if(hand.fingers == null || hand.fingers.Length == 0)
                return false;

            foreach(var finger in hand.fingers) {
                if(finger == null)
                    return false;

                if(!finger.IsPoseSaved(poseType)) {
                    return false;
                }
            }

            return true;
        }
    }
}
