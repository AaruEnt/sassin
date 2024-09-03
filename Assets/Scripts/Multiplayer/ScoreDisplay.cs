using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class ScoreDisplay : MonoBehaviourPun
{
    public TMP_Text txt;
    public SpawnManager sm;
    public void UpdatePoints(Dictionary<string, int> scores)
    {
        this.photonView.RPC("SetScores", RpcTarget.All, scores);
    }

    [PunRPC]
    public void SetScores(Dictionary<string, int> scores)
    {
        txt.text = "";
        foreach (KeyValuePair<string, int> pair in scores)
        {
            txt.text += string.Format("{0}: {1}\n", pair.Key, pair.Value);
        }
        sm.scores = scores;
    }
}
