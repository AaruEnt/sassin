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
            this.photonView.RPC("InitializeServerValues", RpcTarget.All, Randomizer.Prob(50f), (float)Randomizer.RandomRange(-1f, 1f), (float)Randomizer.RandomRange(-180f, 180f));
        }
    }

    [PunRPC]
    public void InitializeServerValues(bool rotate, float speed, float startRotDegrees)
    {
        r.enabled = rotate;
        r.rotVector.y = speed;
        r.gameObject.transform.Rotate(new Vector3(0f, 1f, 0f) * startRotDegrees, Space.Self);
        StartCoroutine(EnableSight());
    }

    private IEnumerator EnableSight()
    {
        yield return new WaitForSeconds(2f);
        eyes.SetActive(true);
    }
}
