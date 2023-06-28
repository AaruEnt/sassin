using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class NetworkPositionLink : MonoBehaviour
{
    private GameObject networkPlayer;
    private NetworkPlayer player;
    // Start is called before the first frame update
    void Start()
    {
        if (NetworkPlayer.player)
        {
            networkPlayer = NetworkPlayer.player;
            player = networkPlayer.GetComponent<NetworkPlayer>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (networkPlayer != null && player != null)
        {
            UnityEngine.Debug.Log(string.Format("pos: {0}, rot: {1}", this.transform.position, this.transform.rotation));
            player.UpdatePositionRotation(this.transform, this.transform.rotation);
        } else
        {
            UnityEngine.Debug.Log("Looking for self");
            if (NetworkPlayer.player)
            {
                networkPlayer = NetworkPlayer.player;
                player = networkPlayer.GetComponent<NetworkPlayer>();
            }
        }
    }
}
