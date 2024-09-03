using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkGourdLordHelper : MonoBehaviour
{
    internal SpawnManager spawnManager;
    internal ArenaWinner arenaWinner;
    // Start is called before the first frame update
    void Start()
    {
        spawnManager = GameObject.Find("SpawnManager")?.GetComponent<SpawnManager>();
        arenaWinner = GameObject.Find("ArenaWinner")?.GetComponent<ArenaWinner>();
    }

    private void OnDestroy()
    {
        spawnManager.AddPoints(PhotonNetwork.LocalPlayer.NickName, 5);
        arenaWinner.OnWin();
    }
}
