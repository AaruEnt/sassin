using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapDominantHand : MonoBehaviour
{
    public bool WatchLeftHand = true; // assumes watch on left hand
    public GameObject leftWatch;
    public GameObject rightWatch;
    public GameObject leftModels;
    public GameObject rightModels;

    // Start is called before the first frame update
    void Start()
    {
        if (WatchLeftHand)
        {
            //EnableAllChildren(leftWatch);
            //DisableAllChildren(rightWatch);
            DisableAllModels(rightModels);
        }
    }

    public void OnSwapCalled()
    {
        if (WatchLeftHand)
            SwapToRight();
        else
            SwapToLeft();
    }

    void SwapToLeft()
    {
        WatchLeftHand = true;
        //EnableAllChildren(leftWatch);
        //DisableAllChildren(rightWatch);
        EnableAllModels(leftModels);
        DisableAllModels(rightModels);
    }

    void SwapToRight()
    {
        WatchLeftHand = false;
        //EnableAllChildren(rightWatch);
        //DisableAllChildren(leftWatch);
        DisableAllModels(leftModels);
        EnableAllModels(rightModels);
    }

    void DisableAllChildren(GameObject obj)
    {
        foreach (Transform t in obj.transform)
        {
            if (t != obj.transform)
                t.gameObject.SetActive(false);
        }
    }

    void EnableAllChildren(GameObject obj)
    {
        foreach (Transform t in obj.transform)
        {
            if (t != obj.transform)
                t.gameObject.SetActive(true);
        }
    }

    void DisableAllModels(GameObject obj)
    {
        Renderer[] rList = obj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer m in rList)
        {
            m.enabled = false;
        }
    }

    void EnableAllModels(GameObject obj)
    {
        Renderer[] rList = obj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer m in rList)
        {
            m.enabled = true;
        }
    }
}
