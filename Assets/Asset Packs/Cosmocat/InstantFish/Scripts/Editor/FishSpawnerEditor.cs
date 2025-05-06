using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Cosmocat.InstantFish
{
    [CustomEditor(typeof(FishSpawner)), CanEditMultipleObjects]
    public class FishSpawnerEditor : Editor
    {
        private BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

        // the OnSceneGUI callback uses the Scene view camera for drawing handles by default
        protected virtual void OnSceneGUI()
        {
            FishSpawner spawner = (FishSpawner)target;

            // copy the target object's data to the handle
            m_BoundsHandle.center = spawner.bounds.center;
            m_BoundsHandle.size = spawner.bounds.size;

            // draw the handle
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                // record the target object before setting new values so changes can be undone/redone
                Undo.RecordObject(spawner, "Change Bounds");

                // copy the handle's updated data back to the target object
                Bounds newBounds = new Bounds();
                newBounds.center = m_BoundsHandle.center;
                newBounds.size = m_BoundsHandle.size;
                spawner.bounds = newBounds;
            }
        }
    }

}
