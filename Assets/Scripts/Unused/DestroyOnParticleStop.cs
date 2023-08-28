using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DestroyOnParticleStop : MonoBehaviour
{
    [SerializeField, Tooltip("Parent object to be destroyed if it exists")]
    private GameObject parentObj;

    public UnityEvent OnBeforeDestroy;

    void Start() {
        transform.parent = null;
    }

    void OnParticleSystemStopped() {
        OnBeforeDestroy.Invoke();
        if (parentObj)
            Destroy(parentObj);
        Destroy(this.gameObject);
    }
}
