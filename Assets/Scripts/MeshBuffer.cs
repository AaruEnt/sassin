using UnityEngine;

namespace RockTools
{
    /// <summary>
    /// This is a more cache friendly object to hold a mesh's data. with each operation we modify this and later we convert this ti .
    /// </summary>
    public class MeshBuffer
    {
        public readonly Vector3[] vertices;
        public readonly int[] triangles;
        public readonly Color[] colors;

        public int vertexCount;
        public int triangleCount;
        public int colorCount;

        public MeshBuffer(int capacity)
        {
            vertices = new Vector3[capacity];
            triangles = new int[capacity];
            colors = new Color[capacity];
        }

        public void Clear()
        {
            vertexCount = 0;
            triangleCount = 0;
            colorCount = 0;
        }

        public void Append(Mesh mesh)
        {
            var mVertices = mesh.vertices;
            var mTriangles = mesh.triangles;
            var mColors = mesh.colors;
            Append(mVertices, mTriangles, mColors, mVertices.Length, mTriangles.Length, mColors.Length);
        }

        public void Append(MeshBuffer other)
        {
            Append(other.vertices, other.triangles, other.colors, other.vertexCount, other.triangleCount, other.colorCount);
        }

        private void Append(Vector3[] vs, int[] ts, Color[] cs, int vCount, int tCount, int cCount)
        {
            var cachedVertexCount = vertexCount;

            // append vertices
            vs.BlockCopy(0, vertices, vertexCount, vCount);
            vertexCount += vCount;

            // append triangles
            var tmp = new int[tCount];
            ts.BlockCopy(0, tmp, 0, tCount);

            for (var i = 0; i < tCount; i++)
                tmp[i] += cachedVertexCount;

            tmp.BlockCopy(0, triangles, triangleCount, tCount);
            triangleCount += tCount;

            // append colors
            cs.BlockCopy(0, colors, colorCount, cCount);
            colorCount += cCount;
        }

        public void AddVertex(Vector3 vector3)
        {
            vertices[vertexCount] = vector3;
            vertexCount++;
        }

        public void AddTriangle(int i0, int i1, int i2)
        {
            triangles[triangleCount] = i0;
            triangles[triangleCount + 1] = i1;
            triangles[triangleCount + 2] = i2;
            triangleCount += 3;
        }

        public void AddColor(Color color)
        {
            colors[colorCount] = color;
            colorCount++;
        }

        public void ReplaceVertex(int source, int destination)
        {
            vertices[destination] = vertices[source];
        }

        public void OverrideTriangles(int[] ts, int tCount)
        {
            ts.BlockCopy(0, triangles, 0, tCount);
            triangleCount = tCount;
        }

        public void Override(Vector3[] vs, int[] ts, Color[] cs)
        {
            Clear();
            Append(vs, ts, cs, vs.Length, ts.Length, cs.Length);
        }

        public Mesh GetMesh()
        {
            var mVertices = new Vector3[vertexCount];
            vertices.BlockCopy(0, mVertices, 0, vertexCount);

            var mTriangles = new int[triangleCount];
            triangles.BlockCopy(0, mTriangles, 0, triangleCount);

            var mColors = new Color[colorCount];
            colors.BlockCopy(0, mColors, 0, colorCount);

            return new Mesh {vertices = mVertices, triangles = mTriangles, colors = mColors};
        }
    }
}