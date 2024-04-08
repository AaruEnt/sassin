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
    public GameObject endCol;
    

    public void OnWin(Collider winningCol)
    {
        string winnerName = PhotonNetwork.LocalPlayer.NickName;
        this.photonView.RPC("CallOnWin", RpcTarget.All, winnerName);
    }

    [PunRPC]
    public void CallOnWin(string winner, PhotonMessageInfo info)
    {
        winningText.text = string.Format("{0} has won!", winner);
        endCol.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }

        Vector3 pos = endPoint.position;
        pos.x = Randomizer.Prob(50f) ? pos.x + (float)Randomizer.GetDouble(0.3f) : pos.x - (float)Randomizer.GetDouble(0.3f);
        pos.z = Randomizer.Prob(50f) ? pos.z + (float)Randomizer.GetDouble(0.3f) : pos.z - (float)Randomizer.GetDouble(0.3f);

        player.transform.position = endPoint.position;
        player.GetComponent<Stats>().trackedObjects.transform.position = endPoint.position;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        player.useMovement = false;
        player.GetComponent<Stats>().AddIFrames(2f);
        StartCoroutine(EnableMovement());
    }

    private IEnumerator EnableMovement()
    {
        yield return new WaitForSeconds(1f);
        player.useMovement = true;
    }
}
