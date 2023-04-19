using UnityEditor;
using UnityEngine;

namespace RockTools
{
    public static class ToolsMenu
    {
        [MenuItem("Tools/Rock Tools/New Rock Generator", false, 99)]
        public static void NewRockGenerator()
        {
            var rockGenerator = RockGenerator.GetInstance();
            Selection.activeObject = rockGenerator.gameObject;
        }

        [MenuItem("Tools/Rock Tools/Export Selected/Wavefront OBJ", false)]
        public static void ExportObj()
        {
            MeshExporter.ExportSelection(true);
        }

        [MenuItem("Tools/Rock Tools/Export Selected/Wavefront OBJ", true)]
        public static bool ExportObjValidate()
        {
            return SelectionHasMeshFilterInChildren();
        }

        private static bool SelectionHasMeshFilterInChildren()
        {
            return Selection.activeObject != null && Selection.activeObject is GameObject gameObject &&
                   gameObject.GetComponentInChildren<MeshFilter>() != null;
        }
    }
}