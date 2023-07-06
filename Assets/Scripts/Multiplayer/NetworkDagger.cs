using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Diagnostics;

public class NetworkDagger : MonoBehaviourPunCallbacks
{
    public static GameObject dagger;

    public GameObject networkModels;

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
        if (photonView.IsMine && DaggerAnimator.LocalDaggerInstance != null && model == null)
        {
            model = DaggerAnimator.LocalDaggerInstance.models.gameObject;
        }
        if (photonView.IsMine)
        {
            foreach (Transform t in transform)
            {
                if (t != transform)
                    t.gameObject.SetActive(false);
            }
        }
        //UnityEngine.Debug.Log(model.transform.position);
        UpdatePositionRotation(model.transform, model.transform.rotation);
    }

    internal void UpdatePositionRotation(Transform pos, Quaternion rot)
    {
        networkModels.transform.position = pos.position;
        networkModels.transform.rotation = rot;
    }
}
