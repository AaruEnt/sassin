using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

public class Crystal : MonoBehaviourPun
{
    public SpawnManager manager;
    public UnityEvent OnGrabEffectComplete;
    public AvailableResources resourceType = new AvailableResources();
    private bool localIsGrabbed = false;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
    }

    public void GrabEffect()
    {
        if (!localIsGrabbed)
        {
            if (manager)
                manager.AddLocalResources(resourceType);
            this.photonView.RPC("PhotonGrabEffect", RpcTarget.All, PlayerManager.LocalPlayerInstance.GetPhotonView().Owner.NickName);
            StartCoroutine(GrabEffectDelayed());
            localIsGrabbed = true;
        }
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

    IEnumerator GrabEffectDelayed()
    {
        yield return new WaitForSeconds(1 / 90f);
        OnGrabEffectComplete.Invoke();
    }
}
