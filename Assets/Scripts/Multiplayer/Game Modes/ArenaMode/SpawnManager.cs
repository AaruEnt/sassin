using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class SpawnManager : MonoBehaviour
{
    public List<GameObject> allowedSpawnables;
    public List<GameObject> spawnables;
    [Range(1,5)]
    public int maxResourceTypesAllowed = 1;
    public GameObject goardPrefab;
    public GameObject goardLordPrefab;
    public Dictionary<GameObject, Transform> crystalList = new Dictionary<GameObject, Transform>();
    public List<Transform> spawnLocations;
    public ScoreDisplay sd;
    [Button]
    public void DebugCreateLord() { SpawnGourdLord(); }

    [Range(0f, 100f)]
    public float goardSpawnOdds = 10f;


    public float spawnTimer = 10f;
    public int maxCrystals = 3;
    public int minCrystals = 1;


    private float timer = 0f;
    private float roundTime = 0f;
    private float goardLordSpawnTime = 300f; // 5 minutes
    private bool spawnedLord = false;
    internal Dictionary<string,int> scores = new Dictionary<string, int>();
    internal AvailableResources localResourcesGathered = new AvailableResources();

    private bool firstTimeSetup = false;

    // Start is called before the first frame update
    void Start()
    {
        SelectResourceType();
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < minCrystals; i++)
            {
                SpawnCrystal();
            }
        }
        else
            this.enabled = false;
        sd.UpdatePoints(scores);
    }

    private void OnEnable()
    {
        if (firstTimeSetup)
        {
            SelectResourceType();
            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < minCrystals; i++)
                {
                    SpawnCrystal();
                }
            }
            else
                this.enabled = false;
            sd.UpdatePoints(scores);
        }
        firstTimeSetup = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (crystalList.Count < maxCrystals)
        {
            timer += Time.deltaTime;
        }
        if (timer >= spawnTimer)
        {
            timer = 0f;
            SpawnCrystal();
        }
        roundTime += Time.deltaTime;
        if (roundTime >= goardLordSpawnTime && !spawnedLord)
        {
            spawnedLord = true;
            PhotonNetwork.Instantiate(goardLordPrefab.name, Vector3.zero, Quaternion.identity, 0);
        }
    }

    internal void SelectResourceType()
    {
        int spawnNum = Randomizer.RandomInt(maxResourceTypesAllowed, 1);
        for (int i = 0; i < spawnNum; i++)
        {
            spawnables.Add(Randomizer.PickRandomObject(allowedSpawnables));
        }
    }

    internal void SpawnCrystal()
    {
        List<Transform> tmp = new List<Transform>(crystalList.Values);
        List<Transform> tmp2 = new List<Transform>(spawnLocations);
        foreach (Transform t in tmp)
        {
            tmp2.Remove(t);
        }
        if (tmp2.Count == 0)
        {
            UnityEngine.Debug.LogWarning("No valid spawn locations found");
            return;
        }
        Transform loc = Randomizer.PickRandomObject(tmp2);
        GameObject toSpawn = Randomizer.PickRandomObject(spawnables);
        if (Randomizer.Prob(goardSpawnOdds))
            toSpawn = goardPrefab;
        crystalList.Add(PhotonNetwork.Instantiate(toSpawn.name, loc.position, Quaternion.identity, 0), loc);
    }

    public bool RemoveCrystal(GameObject crystal)
    {
        if (crystalList.ContainsKey(crystal))
        {
            crystalList.Remove(crystal);
            return true;
        }
        return false;
    }

    public void AddPoints(string name, int points = 1)
    {
            if (scores.ContainsKey(name))
                scores[name] += points;
            else
                scores.Add(name, points);
            sd.UpdatePoints(scores);
    }

    public void UpdatePointTotal()
    {
        sd.UpdatePoints(scores);
    }

    public void SpawnGourdLord()
    {
        spawnedLord = true;
        PhotonNetwork.Instantiate(goardLordPrefab.name, Vector3.zero, Quaternion.identity, 0);
    }

    internal void AddLocalResources(AvailableResources r)
    {
        localResourcesGathered.sandCrystal += r.sandCrystal;
        localResourcesGathered.oceanCrystal += r.oceanCrystal;
        localResourcesGathered.wood += r.wood;
        localResourcesGathered.stone += r.stone;
        localResourcesGathered.food += r.food;
        localResourcesGathered.leather += r.leather;
    }

    internal List<string> GetWinningPlayer()
    {
        List<string> currWinner = new List<string>();
        int points = 0;
        foreach (KeyValuePair<string, int> s in scores)
        {
            if (s.Value > points)
            {
                currWinner.Clear();
                currWinner.Add(s.Key);
                points = s.Value;
            }
            if (s.Value == points)
            {
                currWinner.Add(s.Key);
            }
        }
        return currWinner;
    }
}
