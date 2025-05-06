using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using System.Diagnostics;

public class NetworkDaggerv2 : MonoBehaviourPunCallbacks
{
    public static NetworkDaggerv2 instance;
    public GameObject models;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            instance = this;
            models.SetActive(false);
        }
    }

    internal void UpdatePosRot(Vector3 newPos, Quaternion newRot)
    {
        if (photonView.IsMine)
        {
            transform.position = newPos;
            models.transform.rotation = newRot;
        }
    }
}
