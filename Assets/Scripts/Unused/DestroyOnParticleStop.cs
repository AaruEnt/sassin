using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnParticleStop : MonoBehaviour
{
    [SerializeField, Tooltip("Parent object to be destroyed if it exists")]
    private GameObject parentObj;

    void Start() {
        transform.parent = null;
    }

    void OnParticleSystemStopped() {
        if (parentObj)
            Destroy(parentObj);
        Destroy(this.gameObject);
    }
}
