using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RockTools
{
    public static class MeshExporter
    {
        private static int StartIndex;

        private static string MeshToString(MeshFilter mf, Transform t)
        {
            var r = t.localRotation;

            var numVertices = 0;
            var m = mf.sharedMesh;
            if (!m)
            {
                return "####Error####";
            }

            var mats = mf.GetComponent<Renderer>().sharedMaterials;

            var sb = new StringBuilder();

            foreach (var vv in m.vertices)
            {
                var v = t.TransformPoint(vv);
                numVertices++;
                sb.Append($"v {v.x} {v.y} {-v.z}\n");
            }

            sb.Append("\n");
            foreach (var nn in m.normals)
            {
                var v = r * nn;
                sb.Append($"vn {v.x} {v.y} {-v.z}\n");
            }

            sb.Append("\n");
            foreach (Vector3 v in m.uv)
            {
                sb.Append($"vt {v.x} {v.y}\n");
            }

            for (var material = 0; material < m.subMeshCount; material++)
            {
                sb.Append("\n");
                sb.Append("usemtl ").Append(mats[material].name).Append("\n");
                sb.Append("usemap ").Append(mats[material].name).Append("\n");

                var triangles = m.GetTriangles(material);
                for (var i = 0; i < triangles.Length; i += 3)
                {
                    sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                        triangles[i + 2] + 1 + StartIndex, triangles[i + 1] + 1 + StartIndex,
                        triangles[i] + 1 + StartIndex));
                }
            }

            StartIndex += numVertices;
            return sb.ToString();
        }

        public static void ExportSelection(bool makeSubmeshes)
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.Log("Didn't Export Any Meshes; Nothing was selected!");
                return;
            }

            var meshName = Selection.gameObjects[0].name;
            var fileName = EditorUtility.SaveFilePanel("Export .obj file", "", meshName, "obj");

            StartIndex = 0;

            var meshString = new StringBuilder();

            meshString.Append("#" + meshName + ".obj"
                              + "\n#" + DateTime.Now.ToLongDateString()
                              + "\n#" + DateTime.Now.ToLongTimeString()
                              + "\n#-------"
                              + "\n\n");

            var t = Selection.gameObjects[0].transform;

            var originalPosition = t.position;
            t.position = Vector3.zero;

            if (!makeSubmeshes)
            {
                meshString.Append("g ").Append(t.name).Append("\n");
            }

            meshString.Append(ProcessTransform(t, makeSubmeshes));

            WriteToFile(meshString.ToString(), fileName);

            t.position = originalPosition;

            StartIndex = 0;
            Debug.Log("Exported Mesh: " + fileName);
        }

        private static string ProcessTransform(Transform t, bool makeSubmeshes)
        {
            var meshString = new StringBuilder();

            meshString.Append("#" + t.name
                                  + "\n#-------"
                                  + "\n");

            if (makeSubmeshes)
            {
                meshString.Append("g ").Append(t.name).Append("\n");
            }

            var mf = t.GetComponent<MeshFilter>();
            if (mf != null)
            {
                meshString.Append(MeshToString(mf, t));
            }

            for (var i = 0; i < t.childCount; i++)
            {
                meshString.Append(ProcessTransform(t.GetChild(i), makeSubmeshes));
            }

            return meshString.ToString();
        }

        private static void WriteToFile(string s, string filename)
        {
            using (var sw = new StreamWriter(filename))
            {
                sw.Write(s);
            }
        }
    }
}