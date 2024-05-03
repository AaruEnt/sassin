using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GoardRandomizer : MonoBehaviourPun
{
    public RotateContinuously r;
    public GameObject eyes;
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            this.photonView.RPC("InitializeServerValues", RpcTarget.All, Randomizer.Prob(50f), Randomizer.RandomRange(-1f, 1f));
        }
    }

    [PunRPC]
    public void InitializeServerValues(bool rotate, float speed)
    {
        r.enabled = rotate;
        r.rotVector.y = speed;
        StartCoroutine(EnableSight());
    }

    private IEnumerator EnableSight()
    {
        yield return new WaitForSeconds(2f);
        eyes.SetActive(true);
    }
}
