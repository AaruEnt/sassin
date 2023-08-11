using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GourdGenecide : MonoBehaviour
{
    public GameObject portal;
    public List<GameObject> goodEffects = new List<GameObject>();
    public List<GameObject> evilEffects = new List<GameObject>();
    public List<GameObject> gourdSkulls = new List<GameObject>();
    public GameObject littleSkull;
    public GameObject lordSkull;
    public int genocideAmount = 11;

    private int killedGourds = 0;
    private bool goodEffectsActive = false;
    private bool evilEffectsActive = false;
    private bool goodEffectsForbidden = false;
    private bool gourdLordKilled = false;

    void Start()
    {
        portal.SetActive(true);
        foreach (var g in goodEffects)
        {
            g.SetActive(true);
        }

        foreach (var g in evilEffects)
        {
            g.SetActive(false);
        }
        goodEffectsActive = true;
    }

    void Update()
    {
        if ((goodEffectsActive && killedGourds > 0) || (evilEffectsActive && goodEffectsActive) || (goodEffectsActive && goodEffectsForbidden))
        {
            foreach (var g in goodEffects)
            {
                g.SetActive(false);
            }
            goodEffectsActive = false;
        }
        if (!evilEffectsActive && (killedGourds >= genocideAmount || gourdLordKilled))
        {
            foreach (var g in evilEffects)
            {
                g.SetActive(true);
            }
            evilEffectsActive = true;
        }

        if (goodEffectsActive || evilEffectsActive)
            portal.SetActive(true);
        else
            portal.SetActive(false);
    }

    public void AddKilledGourd()
    {
        killedGourds++;
        foreach (var g in gourdSkulls)
        {
            if (!g.activeSelf)
            {
                g.SetActive(true);
                break;
            }
        }
    }

    public void ForbidGoodEffects()
    {
        goodEffectsForbidden = true;
        if (littleSkull)
            littleSkull.SetActive(true);
    }

    public void LordGourdKilled()
    {
        killedGourds++;
        if (lordSkull)
            lordSkull.SetActive(true);
        gourdLordKilled = true;
    }
}
