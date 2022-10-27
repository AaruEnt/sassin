using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnParticleStop : MonoBehaviour
{
    public GameObject parentObj;

    void Start() {
        transform.parent = null;
    }

    void OnParticleSystemStopped() {
        Destroy(parentObj);
        Destroy(this.gameObject);
    }
}
