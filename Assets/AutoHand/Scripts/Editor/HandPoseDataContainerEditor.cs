using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;



namespace Autohand {
    [CustomEditor(typeof(HandPoseDataContainer), true)]
    public class HandPoseDataContainerEditor : Editor {
        HandPoseDataContainer handPoseContainer;

        private void OnEnable() {
            handPoseContainer = target as HandPoseDataContainer;
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            var startBackground = GUI.backgroundColor; 
            if(handPoseContainer.gameObject != null) {
                EditorUtility.SetDirty(handPoseContainer);
                handPoseContainer.showEditorTools = DrawAutoToggleHeader("Show Editor Tools", handPoseContainer.showEditorTools);

                if(handPoseContainer.showEditorTools) {

                    ShowScriptableSaveButton();

                    ShowHandEditorHand();

                    ShowSaveButtons();

                    DrawHorizontalLine();

                    ShowDeleteOptions();
                }
            }

            GUI.backgroundColor = startBackground;
        }


        public void ShowScriptableSaveButton() {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            handPoseContainer.poseScriptable = (HandPoseScriptable)EditorGUILayout.ObjectField(new GUIContent("Pose Scriptable", "Allows you to save the pose to a scriptable pose, create scriptable pose by right clicking in project [Create > Auto hand > Custom Pose]"), handPoseContainer.poseScriptable, typeof(HandPoseScriptable), true);

            if(handPoseContainer.poseScriptable != null) {
                EditorUtility.SetDirty(handPoseContainer.poseScriptable);
                var rect = EditorGUILayout.GetControlRect();

                if(GUI.Button(rect, "Save Scriptable"))
                    handPoseContainer.SaveScriptable();

                EditorGUILayout.Space();
            }
            EditorGUILayout.Space();
        }

        public void ShowDeleteOptions() {
            GUI.backgroundColor = Color.red;

            if(GUILayout.Button("Delete Hand Copy")) {
                if(string.Equals(handPoseContainer.editorHand.transform.parent.name, "HAND COPY CONTAINER DELETE"))
                    DestroyImmediate(handPoseContainer.editorHand.transform.parent.gameObject);
                else
                    Debug.LogError("Not a copy - Will not delete");
            }
            if(GUILayout.Button("Clear Saved Poses"))
                handPoseContainer.EditorClearPoses();

        }

        public void ShowHandEditorHand() {
            handPoseContainer.editorHand = (Hand)EditorGUILayout.ObjectField(new GUIContent("Editor Hand", "This will be used as a reference to create a hand copy that can be used to model your new pose"), handPoseContainer.editorHand, typeof(Hand), true);

            if(GUILayout.Button("Create Hand Copy"))
                handPoseContainer.EditorCreateHandCopyTool(handPoseContainer.editorHand, handPoseContainer.transform);

            if(GUILayout.Button("Select Hand Copy"))
                Selection.activeGameObject = handPoseContainer.editorHand.gameObject;
        }

        public void DrawHorizontalLine() {

            var rect = EditorGUILayout.GetControlRect();
            rect.y += rect.height / 2f;
            rect.height /= 10f;

            EditorGUI.DrawRect(rect, Color.grey);
        }

        public bool DrawAutoToggleHeader(string label, bool value) {

            EditorGUILayout.Space();
            EditorGUILayout.Space();


            // draw header background and label
            var headerRect = EditorGUILayout.GetControlRect();

            var biggerRect = new Rect(headerRect);
            biggerRect.width += biggerRect.x * 2;
            biggerRect.x = 0;
            biggerRect.y -= 5f;
            biggerRect.height += 10f;
            EditorGUI.DrawRect(biggerRect, Constants.BackgroundColor);


            var labelStyle = Constants.LabelStyle;

            var oldColor1 = GUI.color;
            if(!value) {
                var newColor = new Color(0.65f, 0.65f, 0.65f, 1f);
                newColor.a = 1;
                GUI.contentColor = newColor;
            }

            EditorGUI.LabelField(headerRect, new GUIContent("   " + label), labelStyle);

            GUI.contentColor = oldColor1;

            var oldColor = GUI.color;
            GUI.color = value ? new Color(0.7f, 1f, 0.7f) : new Color(1f, 0.7f, 0.7f);

            var newRect = new Rect(headerRect);
            newRect.position = new Vector2(newRect.x + newRect.width - 18, newRect.y);
            value = EditorGUI.Toggle(newRect, value);

            GUI.color = oldColor;


            return value;
        }

        public void ShowSaveButtons() {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if(handPoseContainer.leftPoseSet)
                GUI.backgroundColor = Color.green;
            else
                GUI.backgroundColor = Color.red;


            if(GUILayout.Button("Save Left")) {
                if(handPoseContainer.poseIndex != handPoseContainer.editorHand.poseIndex)
                    Debug.LogError("CANNOT SAVE: Your hand's \"Pose Index\" value does not match the local \"Pose Index\" value");
                else
                    handPoseContainer.EditorSaveGrabPose(handPoseContainer.editorHand);
            }


            if(handPoseContainer.rightPoseSet)
                GUI.backgroundColor = Color.green;
            else
                GUI.backgroundColor = Color.red;


            if(GUILayout.Button("Save Right")) {
                if(handPoseContainer.poseIndex != handPoseContainer.editorHand.poseIndex)
                    Debug.LogError("CANNOT SAVE: Your hand's \"Pose Index\" value does not match the local \"Pose Index\" value");
                else
                    handPoseContainer.EditorSaveGrabPose(handPoseContainer.editorHand);
            }


            GUILayout.EndHorizontal();
        }

    }
}
