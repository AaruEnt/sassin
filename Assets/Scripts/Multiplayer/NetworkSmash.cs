using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

public class NetworkSmash : MonoBehaviourPun
{
    public UnityEvent e;

    public void CallInvokeEvent()
    {
        this.photonView.RPC("InvokeEvent", RpcTarget.All);
    }

    [PunRPC]
    public void InvokeEvent()
    {
        e.Invoke();
    }
}
