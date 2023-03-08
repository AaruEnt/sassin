using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BF_AddSand : MonoBehaviour
{
    public Material sandMaterial;
    public float angle = 80f;
    public bool isAuto = false;
    public bool useIntersection = false;
    public float intersectionOffset = 0.25f;
    public bool useUpdatedRotation = false;

    private Mesh originalMesh;
    private MeshFilter meshFilter;
    private Mesh newMesh;
    private GameObject newGO;
    private float yIntersection = 0f;
    private Quaternion ySlope = Quaternion.identity;
    private float zNormal = 0;
    private Vector3 normalHit = Vector3.zero;
    private float oldyIntersection = -1f;

    private int[] oldTri;
    private Vector3[] oldVert;
    private Vector3[] oldNorm;
    private Vector3[] oldNormWorld;
    private Vector2[] oldUV;
    private Color[] oldCol;

    private List<int> triangles = new List<int>();
    private List<Vector3> vertexs = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Color> cols = new List<Color>();

    void Start()
    {
        CheckValues();
        BuildInitialGeometry();
    }

    private void Update()
    {
        if (useIntersection)
        {
            CheckIntersection();
        }

        if (useUpdatedRotation)
        {
            UpdateVertexColor();
        }
    }


    private void CheckValues()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        originalMesh = meshFilter.mesh;
        oldTri = originalMesh.triangles;
        oldVert = originalMesh.vertices;
        oldNorm = originalMesh.normals;
        oldNormWorld = oldNorm;

        if (isAuto)
        {
            int k = 0;
            foreach (Vector3 norm in oldNorm)
            {
                oldNormWorld[k] = this.transform.localToWorldMatrix.MultiplyVector(norm).normalized;
                k++;
            }
            oldCol = new Color[oldVert.Length];
        }
        else
        {
            oldCol = originalMesh.colors;
        }
        oldUV = originalMesh.uv;
    }

    private void CheckIntersection()
    {
        int layerMask = 1 << 0 | 1 << 4;
        RaycastHit hit;
        RaycastHit hitIfHit;

        if (Physics.Raycast(transform.position + Vector3.up*5f, Vector3.down, out hit, 200, layerMask))
        {
            if (hit.transform != this.transform)
            {
                yIntersection = hit.point.y + intersectionOffset;
                //Vector3 tangent = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;

                ySlope = Quaternion.LookRotation(hit.normal, Vector3.forward);

                zNormal = ((hit.normal.normalized.z) + 1f) / 2f;
                normalHit = hit.normal.normalized;

                if (yIntersection != oldyIntersection)
                {
                    UpdateSlopeColor();
                }
                oldyIntersection = yIntersection;
            }
            else
            {
                if (Physics.Raycast(hit.point + Vector3.up * -0.05f, Vector3.down, out hitIfHit, 200, layerMask))
                {
                    if (hitIfHit.transform != this.transform)
                    {
                        yIntersection = hitIfHit.point.y + intersectionOffset;
                        //Vector3 tangent = Vector3.ProjectOnPlane(Vector3.down, hitIfHit.normal).normalized;

                        ySlope =  Quaternion.LookRotation(hitIfHit.normal, Vector3.forward);
                        zNormal = ((hitIfHit.normal.normalized.z) + 1f) / 2f;
                        normalHit = hitIfHit.normal.normalized;

                        if (yIntersection != oldyIntersection)
                        {
                            UpdateSlopeColor();
                        }
                        oldyIntersection = yIntersection;
                    }
                }
            }
        }
    }

    private void ClearGeometry()
    {
        triangles.Clear();
        triangles.TrimExcess();
        vertexs.Clear();
        vertexs.TrimExcess();
        uvs.Clear();
        uvs.TrimExcess();
        cols.Clear();
        cols.TrimExcess();
    }

    private void BuildInitialGeometry()
    {
        if (meshFilter == null)
        {
            meshFilter = gameObject.GetComponent<MeshFilter>();
        }
        newMesh = new Mesh();
        newGO = new GameObject();
        MeshFilter mF = newGO.AddComponent<MeshFilter>();
        MeshRenderer mR = newGO.AddComponent<MeshRenderer>();
        mF.mesh = newMesh;
        mR.material = sandMaterial;

        mR.material.SetFloat("_ISADD", 1);
        mR.material.EnableKeyword("IS_ADD");

        if (useIntersection)
        {
            if (mR.material.GetFloat("_USEINTER") == 0)
            {
                mR.material.SetFloat("_USEINTER", 1);
                mR.material.EnableKeyword("USE_INTER");
            }

        }
        else
        {
            if (mR.material.GetFloat("_USEINTER") == 1)
            {
                mR.material.SetFloat("_USEINTER", 0);
                mR.material.DisableKeyword("USE_INTER");
            }
        }




        newGO.transform.parent = this.transform;
        newGO.transform.localPosition = Vector3.zero;
        newGO.transform.localScale = Vector3.one;
        newGO.transform.localRotation = Quaternion.identity;

        int indexNewV = 0;
        foreach (Vector3 v in oldVert)
        {
            vertexs.Add(v + new Vector3(0,0,0));
            uvs.Add(oldUV[indexNewV]);

            indexNewV++;
        }
        indexNewV = 0;
        foreach (int innt in oldTri)
        {
            triangles.Add(oldTri[indexNewV]);

            indexNewV++;
        }

        if (isAuto)
        {
            int j = 0;
            foreach (Vector3 norm in oldNormWorld)
            {
                if(j>= oldCol.Length)
                {
                    break;
                }
                oldCol[j] = Color.red;
                float theAngle = Vector3.Angle(Vector3.up, norm);
                if (theAngle < (angle+10f))
                {
                    Color lerpedColor = Color.Lerp(Color.white, Color.red, Mathf.Max(0f,theAngle- angle/2f) / (angle / 2f));
                    oldCol[j] = lerpedColor;
                }
                j++;
            }
        }

        cols = oldCol.ToList();
        newMesh.vertices = vertexs.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.uv = uvs.ToArray();
        newMesh.colors = cols.ToArray();

        newMesh.normals = originalMesh.normals;

        RecalculateNormalsSeamless(newMesh);
        newMesh.RecalculateBounds();
        newMesh.Optimize();
    }

    private void UpdateVertexColor()
    {
        Color[] updatedColors = newMesh.colors;
        Vector3[] newNormWorld = newMesh.normals;

        if (isAuto)
        {
            int k = 0;
            foreach (Vector3 norm in newMesh.normals)
            {
                newNormWorld[k] = this.transform.localToWorldMatrix.MultiplyVector(norm).normalized;
                k++;
            }
        }


        if (isAuto)
        {
            int j = 0;
            foreach (Vector3 norm in newNormWorld)
            {
                if (j >= updatedColors.Length)
                {
                    break;
                }

                float theAngle = Vector3.Angle(Vector3.up, norm);
                if (theAngle < (angle + 10f))
                {
                    Color lerpedColor = Color.Lerp(new Color(1,1,1, updatedColors[j].a), new Color(1, 0, 0, updatedColors[j].a), Mathf.Max(0f, theAngle - angle / 2f) / (angle / 2f));
                    updatedColors[j].r = lerpedColor.r;
                    updatedColors[j].g = lerpedColor.g;
                    updatedColors[j].b = lerpedColor.b;
                    updatedColors[j].a = lerpedColor.a;
                }
                j++;
            }
        }
        newMesh.colors = updatedColors;
    }

    private void UpdateSlopeColor()
    {
        // This is not perfect for now but gets the job done... //
        int j = 0;

        Color[] updatedColors = newMesh.colors;
        Vector2[] updatedUV4 = new Vector2[updatedColors.Count()];
        Vector2[] updatedUV5 = new Vector2[updatedColors.Count()];
        Vector2[] updatedUV6 = new Vector2[updatedColors.Count()];
        Vector2[] updatedUV7 = new Vector2[updatedColors.Count()];
        foreach (Color norm in newMesh.colors)
        {
            updatedColors[j].a = yIntersection;
            updatedUV4[j] = new Vector2(normalHit.x, normalHit.y);
            updatedUV5[j] = new Vector2(zNormal, normalHit.z);
            updatedUV6[j] = new Vector2(ySlope.x, ySlope.y);
            updatedUV7[j] = new Vector2(ySlope.z, ySlope.w);
            j++;
        }

        newMesh.colors = updatedColors;
        newMesh.uv4 = updatedUV4;
        newMesh.uv5 = updatedUV5;
        newMesh.uv6 = updatedUV6;
        newMesh.uv7 = updatedUV7;
    }

    private void RecalculateNormalsSeamless(Mesh mesh)
    {
        var trianglesOriginal = mesh.triangles;
        var triangles = trianglesOriginal.ToArray();

        var vertices = mesh.vertices;

        var mergeIndices = new Dictionary<int, int>();

        for (int i = 0; i < vertices.Length; i++)
        {
            var vertexHash = vertices[i].GetHashCode();

            if (mergeIndices.TryGetValue(vertexHash, out var index))
            {
                for (int j = 0; j < triangles.Length; j++)
                    if (triangles[j] == i)
                        triangles[j] = index;
            }
            else
                mergeIndices.Add(vertexHash, i);
        }

        mesh.triangles = triangles;

        var normals = new Vector3[vertices.Length];

        mesh.RecalculateNormals();
        var newNormals = mesh.normals;

        for (int i = 0; i < vertices.Length; i++)
            if (mergeIndices.TryGetValue(vertices[i].GetHashCode(), out var index))
                normals[i] = newNormals[index];

        mesh.triangles = trianglesOriginal;
        mesh.normals = normals;
    }
}
