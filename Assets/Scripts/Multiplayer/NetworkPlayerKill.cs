using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using JointVR;

public class NetworkPlayerKill : MonoBehaviourPun
{
    public void CallPlayerStabbed(StabManager manager, GameObject obj)
    {
        manager.UnstabAll();
        bool b = PhotonNetwork.IsMasterClient | !photonView.Owner.IsMasterClient;
        Gather_Invasion.instance.SkullProbability(b);
        this.photonView.RPC("PlayerStabbed", RpcTarget.All);
    }

    public void CallPlayerStabbed()
    {
        bool b = PhotonNetwork.IsMasterClient | !photonView.Owner.IsMasterClient;
        Gather_Invasion.instance.SkullProbability(b);
        this.photonView.RPC("PlayerStabbed", RpcTarget.All);
    }

    [PunRPC]
    public void PlayerStabbed()
    {
        if (photonView.IsMine && Stats.LocalStatsInstance)
        {
            Stats.LocalStatsInstance.DebugKill();
        }
        return;
    }
}
