using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEverySeconds : MonoBehaviour
{
    public float timeBetweenSpawns = 1.5f;
    public GameObject toSpawn;

    private float timer = 0f;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > timeBetweenSpawns)
        {
            timer = 0f;
            Instantiate(toSpawn, transform.position, Quaternion.identity, null);
        }
    }
}
