using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.XR;

namespace Autohand {

    public class EditorHandTool : EditorWindow {

        float bendFingers = 0;
        bool[] fingerStates = new bool[] { };
        float[] fingerBendStates = new float[] { };
        float[] lastFingerBendStates = new float[] { };

        Hand handCopy;
        HandPoseDataContainer pose;
        GrabbablePose grabbablePose;
        GrabbablePoseAdvanced advancedPose;
        HandPoseArea poseArea;

        const int smallestWindowHeight = 320;

        public static void ShowWindow(Hand hand, HandPoseDataContainer pose) {
            var window = GetWindow<EditorHandTool>("Hand Pose Tool", true);
            window.handCopy = hand;
            window.pose = pose;
            window.maxSize = new Vector2(400, smallestWindowHeight + 180);
            window.minSize = new Vector2(160, smallestWindowHeight);
            window.Show(true);
            window.ShowTab();
            if(pose is GrabbablePoseAdvanced)
                window.position = new Rect(50, 100, 240, smallestWindowHeight + 180);
            else
                window.position = new Rect(50, 100, 240, smallestWindowHeight);
        }


        public static void ShowWindow(Hand hand, HandPoseArea poseArea) {
            var window = GetWindow<EditorHandTool>("Hand Pose Tool", true);
            window.handCopy = hand;
            window.poseArea = poseArea;
            window.maxSize = new Vector2(400, smallestWindowHeight);
            window.minSize = new Vector2(160, smallestWindowHeight);
            window.Show(true);
            window.ShowTab();
            window.position = new Rect(50, 100, 240, smallestWindowHeight);
        }

        void CheckInit() {
            if(fingerStates.Length == 0) {
                fingerStates = new bool[handCopy.fingers.Length];
                fingerBendStates = new float[handCopy.fingers.Length];
                lastFingerBendStates = new float[handCopy.fingers.Length];

                for(int i = 0; i < fingerStates.Length; i++) {
                    fingerStates[i] = true;
                    fingerBendStates[i] = 0;
                }

                handCopy.SetLayerRecursive(handCopy.transform, LayerMask.NameToLayer(handCopy.left ? Hand.leftHandLayerName : Hand.rightHandLayerName));
            }
        }


        public void OnGUI() {

            CheckInit();

            if(handCopy == null) {
                Close();
                return;
            }

            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(0, 0, position.width, position.height));

            var rect1 = EditorGUILayout.BeginVertical();
            GUI.color = Color.grey;
            GUI.Box(rect1, GUIContent.none);
            EditorGUILayout.EndVertical();

            GUILayout.EndArea();
            Handles.EndGUI();


            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(0, 0, position.width, position.height));
            var rect = EditorGUILayout.BeginVertical();

            GUI.color = Color.grey;
            GUI.Box(rect, GUIContent.none);
            GUI.Box(rect, GUIContent.none);
            GUI.Box(rect, GUIContent.none);

            ShowTitle();

            ShowFingerMask();

            ShowGrabSlider();

            ShowGrabButton();

            ShowInvertButtons();

            ShowSaveButtons();

            //ShowMultiPoseOptions();

            ShowAdvancedPoseOptions();

            ShowDeleteButton();

            GUILayout.Space(30f);
            EditorGUILayout.EndVertical();

            GUILayout.EndArea();

            Handles.EndGUI();
        }


        //void ShowMultiPoseOptions() {

        //    if(pose is GrabbableMultiPose) 
        //        multiPose = pose as GrabbableMultiPose;
        //    else 
        //        multiPose = null;

        //    if(multiPose != null && leftPosesList == null && rightPosesList == null) {
        //        InitMultiPoseLists();
        //    }
        //    if(multiPose != null) {
        //        GUILayout.Space(20);
        //        Rect tempRect = GUILayoutUtility.GetRect(0, 1000, 0, 1000, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

        //        float columnWidth = (tempRect.width - 10) / 2;

        //        Rect leftListRect = new Rect(tempRect.x, tempRect.y, columnWidth, tempRect.height);
        //        Rect rightListRect = new Rect(tempRect.x + columnWidth + 2, tempRect.y, columnWidth, tempRect.height);

        //        if(leftPosesList != null) {
        //            EditorGUI.LabelField(new Rect(leftListRect.x, leftListRect.y - 20, leftListRect.width, 20), "Left Hand Poses", EditorStyles.boldLabel);
        //            leftListRect.y -= 15; 
        //            leftPosesList.DoList(leftListRect);
        //        }

        //        if(rightPosesList != null) {
        //            EditorGUI.LabelField(new Rect(rightListRect.x, rightListRect.y - 20, rightListRect.width, 20), "Right Hand Poses", EditorStyles.boldLabel);
        //            rightListRect.y -= 15; 
        //            rightPosesList.DoList(rightListRect);
        //        }
        //    }
        //}

        void ShowAdvancedPoseOptions() {
            if(pose is GrabbablePoseAdvanced) {
                advancedPose = (pose as GrabbablePoseAdvanced);
            }
            else {
                advancedPose = null;
            }

            if(advancedPose != null) {
                if(handCopy.left && !advancedPose.leftPoseSet)
                    GUI.backgroundColor = Color.red;
                else if(!handCopy.left && !advancedPose.rightPoseSet)
                    GUI.backgroundColor = Color.red;


                GUILayout.Space(20);
                Rect tempRect = GUILayoutUtility.GetRect(0, 1000, 0, 1000, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

                float columnWidth = (tempRect.width - 10);
                Rect rect = new Rect(tempRect.x+5, tempRect.y, columnWidth, tempRect.height);

                EditorGUI.LabelField(new Rect(rect.x, rect.y - 20, rect.width, 20), "Advanced Pose Options", EditorStyles.boldLabel);
                rect.y -= 15;

                EditorGUI.BeginChangeCheck();

                advancedPose.maxRange = EditorGUI.FloatField(new Rect(rect.x, rect.y + 20, rect.width, 20), "Max Range", advancedPose.maxRange);
                advancedPose.minRange = EditorGUI.FloatField(new Rect(rect.x, rect.y + 40, rect.width, 20), "Min Range", advancedPose.minRange);
                advancedPose.testRange = EditorGUI.Slider(new Rect(rect.x, rect.y + 60, rect.width, 20),  advancedPose.testRange, advancedPose.minRange, advancedPose.maxRange);

                advancedPose.maxAngle = EditorGUI.IntField(new Rect(rect.x, rect.y + 82, rect.width, 20), "Max Angle", advancedPose.maxAngle);
                advancedPose.minAngle = EditorGUI.IntField(new Rect(rect.x, rect.y + 102, rect.width, 20), "Min Angle", advancedPose.minAngle);

                advancedPose.testAngle = EditorGUI.IntSlider(new Rect(rect.x, rect.y + 122, rect.width, 20), advancedPose.testAngle, advancedPose.minAngle, advancedPose.maxAngle);



                if(EditorGUI.EndChangeCheck()) {
                    advancedPose.EditorTestValues(handCopy);
                    EditorUtility.SetDirty(advancedPose);
                    Undo.RegisterCompleteObjectUndo(advancedPose, "Advanced Pose Options");
                }
            }
        }

        void ShowTitle() {
            GUI.color = Color.white;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Hand Pose Tool", AutoHandExtensions.LabelStyle(TextAnchor.MiddleCenter, FontStyle.Bold, 16));
            GUILayout.EndHorizontal();
        }

        Vector2 leftPosesScrollPosition;
        Vector2 rightPosesScrollPosition;
        
        ReorderableList leftPosesList;
        ReorderableList rightPosesList;

        //GrabbableMultiPose multiPose; 
        //void InitMultiPoseLists() {
        //    multiPose = pose as GrabbableMultiPose;
        //    if(multiPose != null) {
        //        // Initialize the ReorderableList for extraLeftPoses
        //        leftPosesList = new ReorderableList(multiPose.extraLeftPoses, typeof(HandPoseData), true, true, true, true);
        //        ConfigureReorderableList(leftPosesList, multiPose.extraLeftPoses, false);

        //        // Initialize the ReorderableList for extraRightPoses
        //        rightPosesList = new ReorderableList(multiPose.extraRightPoses, typeof(HandPoseData), true, true, true, true);
        //        ConfigureReorderableList(rightPosesList, multiPose.extraRightPoses, true);
        //    }
        //}

        void ConfigureReorderableList(ReorderableList list, List<HandPoseData> poses, bool isRightHand) {
            list.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, isRightHand ? "Extra Right Poses" : "Extra Left Poses");
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                EditorGUI.LabelField(rect, "Pose: " + index);
            };

            list.onSelectCallback = (ReorderableList l) => {
                Debug.Log("Selected pose: " + pose);
                poses[l.index].SetPose(handCopy, pose.transform);
            };

            list.onAddCallback = (ReorderableList l) => {
                poses.Add(new HandPoseData(handCopy, pose.transform));
            };
        }

        void ShowFingerMask() {
            for(int i = 0; i < fingerStates.Length; i++) {
                GUILayout.BeginHorizontal();
                var layoutRect = GUILayoutUtility.GetRect(position.width, 20);
                layoutRect.width = layoutRect.width / 2f;
                layoutRect.x += 5f;

                fingerStates[i] = GUI.Toggle(layoutRect, fingerStates[i], handCopy.fingers[i].name);

                layoutRect.position = new Vector2(layoutRect.width, layoutRect.position.y);
                layoutRect.width = layoutRect.width - 10f;
                fingerBendStates[i] = GUI.HorizontalSlider(layoutRect, fingerBendStates[i], 0, 1);

                if(fingerStates[i] && lastFingerBendStates[i] != fingerBendStates[i]) {
                    lastFingerBendStates[i] = fingerBendStates[i];
                    handCopy.fingers[i].SetFingerBend(fingerBendStates[i]);
                }

                GUILayout.EndHorizontal();
            }
        }


        void ShowGrabSlider() {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();

            GUI.backgroundColor = Color.white;

            bendFingers = GUILayout.HorizontalSlider(bendFingers, 0, 1);

            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f, 1f);
            if(GUILayout.Button("Set Fingers Bend")) {
                Undo.RegisterFullObjectHierarchyUndo(handCopy.gameObject, "Auto Pose");

                for(int i = 0; i < handCopy.fingers.Length; i++) {
                    if(fingerStates[i])
                        handCopy.fingers[i].SetFingerBend(bendFingers);
                }
            }

            GUILayout.EndHorizontal();
        }


        void ShowGrabButton() {
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f, 1f);

            if(GUILayout.Button("Auto Pose")) {
                Undo.RegisterFullObjectHierarchyUndo(handCopy.gameObject, "Auto Pose");

                for(int i = 0; i < handCopy.fingers.Length; i++) {
                    if(fingerStates[i])
                        handCopy.fingers[i].BendFingerUntilHit(100, ~LayerMask.GetMask(Hand.rightHandLayerName, Hand.leftHandLayerName));
                }
            }
            GUILayout.EndHorizontal();
        }


        void ShowInvertButtons() {
            GUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f, 1f);

            if(GUILayout.Button("Invert X")) {
                var undoObject = new UnityEngine.Object[] { handCopy, handCopy.transform.parent };
                Undo.RecordObjects(undoObject, "Invert X");

                var scale = handCopy.transform.parent.localScale;
                scale.x = -scale.x;
                handCopy.transform.parent.localScale = scale;
                handCopy.left = !handCopy.left;
            }
            if(GUILayout.Button("Invert Y")) {
                var undoObject = new UnityEngine.Object[]{ handCopy, handCopy.transform.parent };
                Undo.RecordObjects(undoObject, "Invert Y");

                var scale = handCopy.transform.parent.localScale;
                scale.x = -scale.x;
                handCopy.transform.parent.Rotate(new Vector3(0, 0, 180));
                handCopy.transform.parent.localScale = scale;
                handCopy.left = !handCopy.left;
            }
            if(GUILayout.Button("Invert Z")) {
                var undoObject = new UnityEngine.Object[] { handCopy, handCopy.transform.parent };
                Undo.RecordObjects(undoObject, "Invert Z");

                var scale = handCopy.transform.parent.localScale;
                handCopy.transform.parent.Rotate(new Vector3(0, 180, 0));
                scale.x = -scale.x;
                handCopy.transform.parent.localScale = scale;
                handCopy.left = !handCopy.left;
            }

            GUILayout.EndHorizontal();
        }


        void ShowSaveButtons() {
            EditorGUILayout.Space();
            GUI.backgroundColor = Color.grey;

            GUILayout.BeginHorizontal();

            if(pose != null)
                EditorGUILayout.ObjectField(pose, typeof(GrabbablePose), true);
            else
                EditorGUILayout.ObjectField(poseArea, typeof(HandPoseArea), true);

            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();


            if(pose != null) {
                if(pose.leftPoseSet)
                    GUI.backgroundColor = Color.green;
                else
                    GUI.backgroundColor = Color.red;


                if(GUILayout.Button("Save Left")) {
                    if(pose.poseIndex != pose.editorHand.poseIndex) {
                        Debug.Log("Automatically overriding local Pose Index to match hand Pose Index");
                        pose.poseIndex = pose.editorHand.poseIndex;
                    }

                    pose.EditorSaveGrabPose(pose.editorHand);
                }


                if(pose.rightPoseSet)
                    GUI.backgroundColor = Color.green;
                else
                    GUI.backgroundColor = Color.red;


                if(GUILayout.Button("Save Right")) {
                    if(pose.poseIndex != pose.editorHand.poseIndex) {
                        Debug.Log("Automatically overriding local Pose Index to match hand Pose Index");
                        pose.poseIndex = pose.editorHand.poseIndex;
                    }

                    pose.EditorSaveGrabPose(pose.editorHand);
                }

            }
            else {
                var pose = poseArea;

                if(pose.leftPoseSet)
                    GUI.backgroundColor = Color.green;
                else
                    GUI.backgroundColor = Color.red;

                if(GUILayout.Button("Save Left")) {
                    if(pose.poseIndex != pose.editorHand.poseIndex)
                        Debug.LogError("CANNOT SAVE: Your hand's \"Pose Index\" value does not match the local \"Pose Index\" value");
                    else
                        pose.EditorSaveGrabPose(pose.editorHand);
                }


                if(pose.rightPoseSet)
                    GUI.backgroundColor = Color.green;
                else
                    GUI.backgroundColor = Color.red;


                if(GUILayout.Button("Save Right")) {
                    if(pose.poseIndex != pose.editorHand.poseIndex)
                        Debug.LogError("CANNOT SAVE: Your hand's \"Pose Index\" value does not match the local \"Pose Index\" value");
                    else
                        pose.EditorSaveGrabPose(pose.editorHand);
                }
            }


            GUILayout.EndHorizontal();
        }


        void ShowDeleteButton() {
            EditorGUILayout.Space();
            GUI.backgroundColor = Color.grey;

            GUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(handCopy, typeof(Hand), true);
            GUILayout.EndHorizontal();

            GUI.backgroundColor = new Color(1f, 0f, 0f, 1f);

            if(GUILayout.Button("Delete Hand Copy")) {
                if(pose != null)
                    Selection.activeGameObject = pose.gameObject;
                else
                    Selection.activeGameObject = poseArea.gameObject;
                DestroyImmediate(handCopy.transform.parent.gameObject);
                Close();
            }

        }
    }

}