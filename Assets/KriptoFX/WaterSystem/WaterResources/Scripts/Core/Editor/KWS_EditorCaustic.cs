#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace KWS
{
    public class KWS_EditorCaustic
    {
        public void DrawCausticEditor(WaterSystem waterSystem)
        {
            Handles.lighting = false;
            var causticAreaScale = new Vector3(waterSystem.Settings.CausticOrthoDepthAreaSize, waterSystem.Settings.Transparent, waterSystem.Settings.CausticOrthoDepthAreaSize);
            var causticAreaPos   = waterSystem.Settings.CausticOrthoDepthPosition - Vector3.up * waterSystem.Settings.Transparent * 0.5f;
            Handles.matrix = Matrix4x4.TRS(causticAreaPos, Quaternion.identity, causticAreaScale);

            Handles.color = new Color(0, 0.75f, 1, 0.15f);
            Handles.CubeHandleCap(0, Vector3.zero, Quaternion.identity, 1, EventType.Repaint);
            Handles.color = new Color(0, 0.75f, 1, 0.9f);
            Handles.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}

#endif