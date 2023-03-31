using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderUpdater : MonoBehaviour
{

    public SkinnedMeshRenderer meshRenderer;
    public MeshCollider col;

    public void UpdateCollider()
    {
        Mesh colliderMesh = new Mesh();
        meshRenderer.BakeMesh(colliderMesh);
        col.sharedMesh = null;
        col.sharedMesh = colliderMesh;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCollider();
    }


}
