using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Autohand;
using NaughtyAttributes;

public class Gather_Invasion : MonoBehaviourPunCallbacks
{
    [Header("References")]
    public GameObject gatherSpawn;
    public GameObject invasionSpawn;
    public Transform gatherSpawnPoint;
    public Transform invasionSpawnPoint;
    public List<Transform> consecutiveCrystalSpawns = new List<Transform>();
    [Header("Prefabs")]
    public GameObject gatherCrystalObj;

    private GameObject player;
    int i = 0;
    internal int points = 0;
    internal int skulls = 0;

    // Start is called before the first frame update
    void Start()
    {
        player = Stats.LocalStatsInstance.gameObject;
        var mv = player.GetComponent<AutoHandPlayer>();
        if (PhotonNetwork.IsMasterClient)
        {
            invasionSpawn.SetActive(false);
            gatherSpawn.SetActive(true);
            bool tmp = mv.useMovement;
            mv.useMovement = false;
            player.transform.position = gatherSpawnPoint.position;
            mv.useMovement = tmp;
            Stats.LocalStatsInstance.OnDeath.AddListener(PlayerDeath);
            SpawnCrystal();
        } else
        {
            invasionSpawn.SetActive(true);
            gatherSpawn.SetActive(false);
            bool tmp = mv.useMovement;
            mv.useMovement = false;
            player.transform.position = invasionSpawnPoint.position;
            mv.useMovement = tmp;
        }
    }

    private void SpawnCrystal()
    {
        var c = Instantiate(gatherCrystalObj, consecutiveCrystalSpawns[i++]);
        c.transform.localPosition = Vector3.zero;
        c.transform.localRotation = Quaternion.identity;
        var g = c.GetComponentInChildren<Grabbable>();
        g.OnGrabEvent += CrystalGrabbed;
        if (i >= consecutiveCrystalSpawns.Count)
            i = 0;
    }

    public void CrystalGrabbed(Hand h, Grabbable g)
    {
        points++;
        SpawnCrystal();
        g.OnGrabEvent -= CrystalGrabbed;
    }

    public void PlayerDeath()
    {
        int tmp = (int)(0.9f * (float)points);
        int toSpawn = points - tmp;
        if (toSpawn < 1 && tmp > 0)
        {
            toSpawn = 1;
            tmp -= 1;
        }
        points = tmp;
        UnityEngine.Debug.LogFormat("Lost {0} points. New total: {1}", toSpawn, points);
        // spawn a bunch of crystals around death point
    }
}
