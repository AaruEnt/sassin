using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject crystalPrefab;
    public GameObject goardPrefab;
    public Dictionary<GameObject, Transform> crystalList = new Dictionary<GameObject, Transform>();
    public List<Transform> spawnLocations;
    public ScoreDisplay sd;

    [Range(0f, 100f)]
    public float goardSpawnOdds = 10f;


    public float spawnTimer = 10f;
    public int maxCrystals = 3;
    public int minCrystals = 1;


    private float timer = 0f;
    internal Dictionary<string,int> scores = new Dictionary<string, int>();

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < minCrystals; i++)
            {
                SpawnCrystal();
            }
        }
        sd.UpdatePoints(scores);
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
        GameObject toSpawn = crystalPrefab;
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

    public void AddPoints(string name)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (scores.ContainsKey(name))
                scores[name] += 1;
            else
                scores.Add(name, 1);
            sd.UpdatePoints(scores);
        }
    }

    public void UpdatePointTotal()
    {
        sd.UpdatePoints(scores);
    }
}
