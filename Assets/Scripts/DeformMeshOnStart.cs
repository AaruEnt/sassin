using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformMeshOnStart : MonoBehaviour
{
    

    public SkinnedMeshRenderer meshRenderer;
    public MeshCollider collider;


    // Start is called before the first frame update
    void Start()
    {
        if (meshRenderer != null && collider != null)
            UpdateCollider();
    }

    public void UpdateCollider()
    {
        Mesh colliderMesh = new Mesh();
        meshRenderer.BakeMesh(colliderMesh);
        collider.sharedMesh = null;
        collider.sharedMesh = colliderMesh;
    }

}
