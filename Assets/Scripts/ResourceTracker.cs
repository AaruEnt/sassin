using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using NaughtyAttributes;
using Com.Aaru.Sassin;

public class ResourceTracker : MonoBehaviourPun
{
    public int sandCrystal = 0;
    public int oceanCrystal = 0;
    public int wood = 0;
    public int stone = 0;
    public int food = 0;
    public int leather = 0;

    public int skulls = 0;

    public SpawnManager? sm;
    public Gather_Invasion? g_i;


    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (sm)
        {
            sandCrystal = sm.localResourcesGathered.sandCrystal;
            oceanCrystal = sm.localResourcesGathered.oceanCrystal;
            wood = sm.localResourcesGathered.wood;
            stone = sm.localResourcesGathered.stone;
            food = sm.localResourcesGathered.food;
            leather = sm.localResourcesGathered.leather;
        }
        if (g_i)
        {
            if (g_i.usingSandCrystal)
                sandCrystal = g_i.points;
            else
                oceanCrystal = g_i.points;
            skulls = g_i.skulls;
        }
    }

    public void EarlyLeaverPenalty()
    {
        sandCrystal /= 2;
        oceanCrystal /= 2;
        wood /= 2;
        stone /= 2;
        food /= 2;
        leather /= 2;
        skulls /= 2;
    }
}
