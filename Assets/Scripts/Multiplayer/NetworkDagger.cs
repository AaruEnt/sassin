using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkDagger : MonoBehaviourPunCallbacks
{
    public static GameObject dagger;

    internal GameObject model;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            dagger = this.gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            foreach (Transform t in transform)
            {
                if (t != transform)
                    t.gameObject.SetActive(false);
            }
        }
    }

    internal void UpdatePositionRotation(Transform pos, Quaternion rot)
    {
        transform.position = pos.position;
        transform.rotation = rot;
    }
}
