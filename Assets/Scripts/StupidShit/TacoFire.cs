using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacoFire : MonoBehaviour
{
    public float summonPoints = 0f;
    public GameObject fire;
    public GameObject summonTaco;
    public Transform summonPoint;

    private float summonThreshhold = 10f;
    

    // Update is called once per frame
    void Update()
    {
        if (summonPoints > 0f)
        {
            summonPoints -= Time.deltaTime * 0.5f;
            fire.SetActive(true);
        }
        else
        {
            fire.SetActive(false);
        }
        if (summonPoints >= summonThreshhold)
        {
            Instantiate(summonTaco, summonPoint.position, Quaternion.identity, null);
            summonPoints = summonThreshhold - 1f;
        }
    }

    public void AddPoints(float points)
    {
        if (points > 0f)
        {
            summonPoints += points;
        }
    }
}
