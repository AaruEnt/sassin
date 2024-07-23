using UnityEditor;
using UnityEngine;

namespace CloudFine.ThrowLab
{
    // IngredientDrawerUIE
    [CustomPropertyDrawer(typeof(ThrowConfigurationSet))]
    public class ThrowConfigurationSetDrawer : PropertyDrawer
    {
        private bool foldout = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);
            float lineHeight = base.GetPropertyHeight(property, label);
            var rect = new Rect(position.x, position.y, position.width, lineHeight);

            // Draw label
            foldout = EditorGUI.Foldout(rect, foldout, label, true);
            
            if (foldout)
            {
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                SerializedProperty _configDict = property.FindPropertyRelative("_deviceConfigurations");
                var deviceTypes = System.Enum.GetValues(typeof(Device));

                if (_configDict.arraySize != deviceTypes.Length)
                {
                    _configDict.arraySize = deviceTypes.Length;
                }
                foreach (Device type in deviceTypes)
                {
                    rect.y += lineHeight;
                    Rect propRect = EditorGUI.PrefixLabel(rect, new GUIContent(type.ToString()));
                    EditorGUI.PropertyField(propRect, _configDict.GetArrayElementAtIndex((int)type), GUIContent.none);
                }

                EditorGUI.indentLevel = indent;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (foldout)
            {
                int lines = System.Enum.GetValues(typeof(Device)).Length;
                return base.GetPropertyHeight(property, label) * (lines + 1);
            }
            return base.GetPropertyHeight(property, label);

        }
    }
}