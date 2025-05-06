using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using Autohand.Demo;
using JointVR;

public class NetworkSmash : MonoBehaviourPun
{
    public Smash s;
    public GameObject spawnOnDeath;
    [Range(0f, 100f)]
    public float crystalSpawnOdds = 25f;

    public void CallInvokeEvent()
    {
        UnityEngine.Debug.LogWarning("Called invoke event");
        foreach (Collider c in GetComponentsInChildren<Collider>())
            StabManager.LocalDaggerInstance.UnstabTarget(c);
        bool spawnCrystal = false;
        if (Randomizer.Prob(crystalSpawnOdds))
            spawnCrystal = true;
        this.photonView.RPC("InvokeEvent", RpcTarget.All, spawnCrystal);
        //StartCoroutine(DestroyAfterSeconds(3f));
    }

    [PunRPC]
    public void InvokeEvent(bool spawnCrystal)
    {
        if (spawnCrystal && PhotonNetwork.IsMasterClient)
            PhotonNetwork.Instantiate(spawnOnDeath.name, transform.position, Quaternion.identity);
        s.DoSmash();
    }

    private IEnumerator DestroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        foreach (Collider c in GetComponentsInChildren<Collider>())
            StabManager.LocalDaggerInstance.UnstabTarget(c);
        PhotonNetwork.Destroy(this.gameObject);
    }
}
