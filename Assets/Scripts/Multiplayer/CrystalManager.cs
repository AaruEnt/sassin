using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalManager : MonoBehaviour
{
    public GameObject crystalPrefab;
    public Dictionary<GameObject, Transform> crystalList = new Dictionary<GameObject, Transform>();
    public List<Transform> spawnLocations;
    public ScoreDisplay sd;


    public float spawnTimer = 10f;
    public int maxCrystals = 3;
    public int minCrystals = 1;


    private float timer = 0f;
    internal Dictionary<string, int> scores = new Dictionary<string, int>();

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
        if (PhotonNetwork.IsMasterClient)
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
    }

    internal void SpawnCrystal()
    {
        List<Transform> tmp = new List<Transform>(crystalList.Values);
        Transform loc = Randomizer.PickRandomObject(spawnLocations);
        while (tmp.Contains(loc))
        {
            loc = Randomizer.PickRandomObject(spawnLocations);
        }
        crystalList.Add(PhotonNetwork.Instantiate(crystalPrefab.name, loc.position, Quaternion.identity, 0), loc);
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
        }
        sd.UpdatePoints(scores);
    }
}
