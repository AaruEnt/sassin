using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class DestroyOnParticleStop : MonoBehaviourPun
{
    [SerializeField, Tooltip("Parent object to be destroyed if it exists")]
    private GameObject parentObj;
    public bool useNetworkDestroy = false;

    public UnityEvent OnBeforeDestroy;

    void Start() {
        transform.parent = null;
    }

    void OnParticleSystemStopped() {
        OnBeforeDestroy.Invoke();
        if (useNetworkDestroy)
        {
            if (parentObj)
                PhotonNetwork.Destroy(parentObj);
            PhotonNetwork.Destroy(this.gameObject);
        }
        if (parentObj)
            Destroy(parentObj);
        Destroy(this.gameObject);
    }
}
