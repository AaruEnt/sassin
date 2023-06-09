using UnityEditor;
using UnityEngine;

namespace Cosmocat.InstantFish
{
    [CustomEditor(typeof(Fish))]
    public class ObjectBuilderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Fish fish = (Fish)target;
            if (GUILayout.Button("Refresh Profile"))
            {
                fish.RefreshProfile();
            }
        }
    }
}
