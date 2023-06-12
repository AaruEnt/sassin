using UnityEngine;
using System.Collections.Generic;

namespace Cosmocat.InstantFish
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Combiner : MonoBehaviour
    {
        void OnEnable()
        {
            List<MeshFilter> meshFilters = new List<MeshFilter>();

            // This is beacuse GetComponentsInChildren includes parent, do not want
            foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>())
            {
                if (mf.gameObject.GetInstanceID() != gameObject.GetInstanceID())
                {
                    meshFilters.Add(mf);
                }
            }
            if (meshFilters.Count == 0)
            {
                return;
            }

            Matrix4x4 myTransform = transform.worldToLocalMatrix;

            CombineInstance[] combine = new CombineInstance[meshFilters.Count];
            int i = 0;
            while (i < meshFilters.Count)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = myTransform * meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].GetComponent<MeshRenderer>().enabled = false;
                i++;
            }

            transform.GetComponent<MeshFilter>().mesh = new Mesh();
            transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);

            var newMesh = transform.GetComponent<MeshRenderer>();
            var oldMesh = meshFilters[0].gameObject.GetComponent<MeshRenderer>();

            // Copy first material from first mesh renderer
            newMesh.material = oldMesh.sharedMaterial;

            // Copy shadow settings from first mesh renderer
            newMesh.receiveShadows = oldMesh.receiveShadows;
            newMesh.shadowCastingMode = oldMesh.shadowCastingMode;

            transform.gameObject.SetActive(true);

        }
    }
}