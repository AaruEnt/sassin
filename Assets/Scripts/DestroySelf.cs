using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;

public class DestroySelf : MonoBehaviour
{
    [Tooltip("If left null, will destroy self")]
    public GameObject toDestroy;

    public float delay = 0f;

    public bool autoDestroyOnCreate = false;

    public bool networkDestroy = false;

    public void CallDestroy()
    {
        if (networkDestroy)
            PhotonNetwork.Destroy(toDestroy);
        if (toDestroy == null)
        {
            Destroy(this.gameObject, delay);
        }
        else
        {
            Destroy(toDestroy, delay);
        }
    }

    void Start()
    {
        if (autoDestroyOnCreate)
        {
            if (toDestroy == null)
            {
                Destroy(this.gameObject, delay);
            }
            else
            {
                Destroy(toDestroy, delay);
            }
        }
    }
}
