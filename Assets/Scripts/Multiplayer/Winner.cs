using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class Winner : MonoBehaviourPun
{
    public TMP_Text winningText;
    public AutoHandPlayer player;
    public Transform endPoint;
    

    public void OnWin(Collider winningCol)
    {
        PhotonView view = winningCol.GetComponent<PhotonView>();
        string winnerName = "";
        if (view)
            winnerName = view.Owner.NickName;
        this.photonView.RPC("CallOnWin", RpcTarget.All, winnerName);
    }

    [PunRPC]
    public void CallOnWin(string winner, PhotonMessageInfo info)
    {
        winningText.text = string.Format("{0} has won!", winner);

        Vector3 pos = endPoint.position;
        pos.x = Randomizer.Prob(50f) ? pos.x + (float)Randomizer.GetDouble(0.5f) : pos.x - (float)Randomizer.GetDouble(0.5f);
        pos.z = Randomizer.Prob(50f) ? pos.z + (float)Randomizer.GetDouble(0.5f) : pos.z - (float)Randomizer.GetDouble(0.5f);

        player.transform.position = endPoint.position;
        player.GetComponent<Stats>().trackedObjects.transform.position = endPoint.position;
    }
}
