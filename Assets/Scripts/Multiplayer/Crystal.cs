using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Crystal : MonoBehaviourPun
{
    public SpawnManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
    }

    public void GrabEffect()
    {
        this.photonView.RPC("PhotonGrabEffect", RpcTarget.All, this.photonView.Owner.NickName);
    }

    [PunRPC]
    internal void PhotonGrabEffect(string player)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        if (manager.RemoveCrystal(this.gameObject))
            this.photonView.RPC("AddPointToPlayer", RpcTarget.All, player);
    }

    [PunRPC]
    internal void AddPointToPlayer(string player)
    {
        UnityEngine.Debug.Log("Added 1 point to player: " + player);
        manager.AddPoints(player);
    }

    public void RemoveCrystalNoPoints()
    {
        this.photonView.RPC("PhotonRemoveCrystalNoPoints", RpcTarget.All);
    }

    [PunRPC]
    internal void PhotonRemoveCrystalNoPoints()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        manager.RemoveCrystal(this.gameObject);
    }
}
