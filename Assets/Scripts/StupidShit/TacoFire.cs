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
    private float cd = 0f;
    private int toSummon = 0;
    private float tacoCD = 0f;
    

    // Update is called once per frame
    void Update()
    {
        if (cd > 0f)
        {
            cd -= Time.deltaTime;
        }
        if (tacoCD > 0f)
        {
            tacoCD -= Time.deltaTime;
        }
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
        } else if (toSummon > 0 && tacoCD <= 0f)
        {
            Instantiate(summonTaco, summonPoint.position, Quaternion.identity, null);
            tacoCD = 0.25f;
            toSummon -= 1;
        }
    }

    public void AddPoints(float points)
    {
        if (cd > 0f)
        {
            return;
        }
        cd = 1.5f;
        if (points > 0f)
        {
            summonPoints += points;
        }
    }

    internal void AddPoints(float points, bool CDIgnore)
    {
        if (cd > 0f && !CDIgnore)
        {
            return;
        }
        cd = 1.5f;
        if (points > 0f)
        {
            summonPoints += points;
        }
    }

    internal void QueueTaco(int toQueue)
    {
        toSummon = toQueue > 0 ? toQueue : 0;
    }
}
