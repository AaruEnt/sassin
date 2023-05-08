using System.Collections.Generic;
using UnityEngine;

namespace RockTools
{
    /// <summary>
    /// MeshSplitter is used to cut a mesh with a plane
    /// </summary>
    public static class MeshSplitter
    {
        private static readonly List<int> splitTriangles;
        private static readonly List<Vector3> splitVertices;
        private static readonly List<Color> splitColors;

        static MeshSplitter()
        {
            splitTriangles = new List<int>(300);
            splitVertices = new List<Vector3>(300);
            splitColors = new List<Color>(300);
        }

        public static void Split(MeshBuffer meshBuffer, Plane plane)
        {
            splitTriangles.Clear();
            splitVertices.Clear();
            splitColors.Clear();

            for (var i = 0; i < meshBuffer.triangleCount; i += 3)
            {
                var ii0 = meshBuffer.triangles[i];
                var ii1 = meshBuffer.triangles[i + 1];
                var ii2 = meshBuffer.triangles[i + 2];

                var p1 = meshBuffer.vertices[ii0];
                var p1Side = plane.GetSide(p1);

                var p2 = meshBuffer.vertices[ii1];
                var p2Side = plane.GetSide(p2);

                var p3 = meshBuffer.vertices[ii2];
                var p3Side = plane.GetSide(p3);

                var c1 = meshBuffer.colors[ii0];
                var c2 = meshBuffer.colors[ii1];
                var c3 = meshBuffer.colors[ii2];

                // all points are on the same side
                if (p1Side == p2Side && p1Side == p3Side)
                {
                    if (p1Side)
                        AddVertices(p1, p2, p3, c1, c2, c3);
                }
                else
                {
                    //we need the two points where the plane intersects the triangle.
                    Vector3 i1;
                    Vector3 i2;

                    Color i1c;
                    Color i2c;

                    //point 1 and 2 are on the same side
                    if (p1Side == p2Side)
                    {
                        //Cast a ray from p2 to p3 and from p3 to p1 to get the intersections                       
                        i1 = GetPointOfIntersection(p2, p3, plane, c2, c3, out i1c);
                        i2 = GetPointOfIntersection(p3, p1, plane, c3, c1, out i2c);

                        if (p1Side)
                        {
                            AddVertices(p1, p2, i1, c1, c2, i1c);
                            AddVertices(p1, i1, i2, c1, i1c, i2c);
                        }
                        else
                            AddVertices(i1, p3, i2, i1c, c3, i2c);
                    }
                    //point 1 and 3 are on the same side
                    else if (p1Side == p3Side)
                    {
                        //Cast a ray from p1 to p2 and from p2 to p3 to get the intersections                       
                        i1 = GetPointOfIntersection(p1, p2, plane, c1, c2, out i1c);
                        i2 = GetPointOfIntersection(p2, p3, plane, c2, c3, out i2c);

                        if (p1Side)
                        {
                            AddVertices(p1, i1, p3, c1, i1c, c3);
                            AddVertices(i1, i2, p3, i1c, i2c, c3);
                        }
                        else
                            AddVertices(i1, p2, i2, i1c, c2, i2c);
                    }
                    //p1 is alone
                    else
                    {
                        //Cast a ray from p1 to p2 and from p1 to p3 to get the intersections                       
                        i1 = GetPointOfIntersection(p1, p2, plane, c1, c2, out i1c);
                        i2 = GetPointOfIntersection(p1, p3, plane, c1, c3, out i2c);

                        if (p1Side)
                            AddVertices(p1, i1, i2, c1, i1c, i2c);
                        else
                        {
                            AddVertices(i1, p2, p3, i1c, c2, c3);
                            AddVertices(i1, p3, i2, i1c, c3, i2c);
                        }
                    }
                }
            }

            meshBuffer.Override(splitVertices.ToArray(), splitTriangles.ToArray(), splitColors.ToArray());
        }

        private static Vector3 GetPointOfIntersection(Vector3 p1, Vector3 p2, Plane plane, Color a, Color b, out Color c)
        {
            var dist_p1_p2 = p2 - p1;
            var ray = new Ray(p1, dist_p1_p2);
            plane.Raycast(ray, out var distance);
            c = Color.Lerp(a, b, distance / dist_p1_p2.magnitude);
            return ray.GetPoint(distance);
        }

        private static void AddVertices(Vector3 p1, Vector3 p2, Vector3 p3, Color c1, Color c2, Color c3)
        {
            var vCount = splitVertices.Count;

            splitVertices.Add(p1);
            splitVertices.Add(p2);
            splitVertices.Add(p3);

            splitColors.Add(c1);
            splitColors.Add(c2);
            splitColors.Add(c3);

            for (var i = 0; i < 3; i++)
                splitTriangles.Add(vCount + i);
        }
    }
}