using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Autohand;
using NaughtyAttributes;
using System.Linq;
using Photon.Realtime;

public class Gather_Invasion : MonoBehaviourPunCallbacks
{
    public static Gather_Invasion instance;
    [Header("References")]
    public GameObject gatherSpawn;
    public GameObject invasionSpawn;
    public Transform gatherSpawnPoint;
    public Transform invasionSpawnPoint;
    public List<Transform> consecutiveCrystalSpawns = new List<Transform>();
    public Boat boat;
    private Dictionary<GameObject, Transform> crystalRefs = new Dictionary<GameObject, Transform>();

    [Header("Prefabs")]
    public GameObject sandCrystal;
    public GameObject oceanCrystal;
    private GameObject gatherCrystalObj;
    [Header("Variables")]
    public float boatSpawnTime = 600f; // time in seconds
    internal float boatTimer = 0f;
    public float boatWaitTime = 30f; // time in seconds
    private bool boatSpawned = false;


    internal bool usingSandCrystal;
    internal float sandCrystalOdds = 50f;

    public int crystalsAheadAllowed = 3;

    private GameObject player;
    int i = 0;
    internal int points = 0;
    internal int skulls = 0;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        player = Stats.LocalStatsInstance.gameObject;
        var mv = player.GetComponent<AutoHandPlayer>();
        if (PhotonNetwork.IsMasterClient)
        {
            invasionSpawn.SetActive(false);
            gatherSpawn.SetActive(true);

            if (Randomizer.Prob(sandCrystalOdds))
            {
                gatherCrystalObj = sandCrystal;
                usingSandCrystal = true;
            } else
            {
                gatherCrystalObj = oceanCrystal;
                usingSandCrystal = false;
            }

            bool tmp = mv.useMovement;
            mv.useMovement = false;
            player.transform.position = gatherSpawnPoint.position;
            mv.useMovement = tmp;
            Stats.LocalStatsInstance.OnDeath.AddListener(PlayerDeath);
            for (int j = 0; j < crystalsAheadAllowed; j++)
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

    private void Update()
    {
        boatTimer += Time.deltaTime;
        if (boatTimer >= boatSpawnTime && !boatSpawned)
        {
            this.photonView.RPC("ExitBoat", RpcTarget.All);
        }
    }

    [PunRPC]
    public void ExitBoat()
    {
        boatSpawned = true;
        boat.waitTime = boatWaitTime;
        boat.gameObject.SetActive(true );
    }

    private void SpawnCrystal()
    {
        while (crystalRefs.Values.Contains(consecutiveCrystalSpawns[i]))
            i++;
        var c = Instantiate(gatherCrystalObj, consecutiveCrystalSpawns[i]);
        c.transform.localPosition = Vector3.zero;
        c.transform.localRotation = Quaternion.identity;
        var g = c.GetComponentInChildren<Grabbable>();
        g.OnGrabEvent += CrystalGrabbed;
        crystalRefs.Add(c, consecutiveCrystalSpawns[i] );
        i++;
        if (i >= consecutiveCrystalSpawns.Count)
            i = 0;
    }

    public void CrystalGrabbed(Hand h, Grabbable g)
    {
        points++;
        crystalRefs.Remove(g.gameObject);
        if (crystalRefs.Count < crystalsAheadAllowed)
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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        crystalsAheadAllowed = PhotonNetwork.CurrentRoom.PlayerCount + 3;
    }

    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        crystalsAheadAllowed = PhotonNetwork.CurrentRoom.PlayerCount + 3;
    }

    internal bool SkullProbability(bool lowerProb = false)
    {
        float prob = 1f;
        switch (PhotonNetwork.CurrentRoom.PlayerCount)
        {
            case 1: prob = 0f; break;
            case 2: prob = 1f; break;
            case 3: prob = 0.5f; break;
        }
        if (lowerProb)
        {
            prob /= 2;
        }
        prob -= skulls;
        if (prob <= 5f)
            prob = 5f;
        if (Randomizer.Prob(prob))
        {
            skulls++;
            return true;
        } else
            return false;
    }
}
