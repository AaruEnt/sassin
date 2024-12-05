using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxColliderSerializationFixer : MonoBehaviour
{
    //THIS SCRIPT SERVES TO FIX A SERIALIZATION BUG THAT OCCURS WHEN PACKAGING
    //A SCENE OR PREFAB WITH A BOX COLLIDER FROM 2023 OR HIGHER,
    //AND UNPACKING THAT SCENE ON A UNITY VERSION BEFORE 2022_2

    [System.Serializable]
    public struct ColliderData {
        public BoxCollider collider;
        public Vector3 size;
    }

    [SerializeField]
    private List<ColliderData> colliderSizes = new List<ColliderData>();

    [ContextMenu("Save Colliders")]
    public void SaveColliderSizes() {
        colliderSizes.Clear();
        SaveColliderSizesRecursive(transform);
    }

#if UNITY_2022_1_OR_NEWER
#else
    public void Start() {
        ApplyColliderSizesRecursive();
    }
#endif

    private void SaveColliderSizesRecursive(Transform currentTransform) {
        BoxCollider[] boxCollider = currentTransform.GetComponents<BoxCollider>();
        if(boxCollider != null && boxCollider.Length > 0) {
            for(int i = 0; i < boxCollider.Length; i++) {
                ColliderData data = new ColliderData {
                    size = boxCollider[i].size,
                    collider = boxCollider[i]
                };
                colliderSizes.Add(data);
            }
        }

        foreach(Transform child in currentTransform) {
            SaveColliderSizesRecursive(child);
        }
    }

    [ContextMenu("Apply Colliders")]
    public void ApplyColliderSizesRecursive() {
        //Debug.Log("Applying Collider  Resizes: This is a fix to a Unity Error where box collider sizes are not saved properly when downloading a scene from Unity 2022 or higher on a project from 2021 or lower. ");
        foreach(var collider in colliderSizes) {
            if(collider.collider != null)
                collider.collider.size = collider.size;
        }
    }
}
