using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapDominantHand : MonoBehaviour
{
    public bool WatchLeftHand = true; // assumes watch on left hand
    public GameObject leftWatch;
    public GameObject rightWatch;

    // Start is called before the first frame update
    void Start()
    {
        if (WatchLeftHand)
        {
            EnableAllChildren(leftWatch);
            DisableAllChildren(rightWatch);
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
        EnableAllChildren(leftWatch);
        DisableAllChildren(rightWatch);
    }

    void SwapToRight()
    {
        WatchLeftHand = false;
        EnableAllChildren(rightWatch);
        DisableAllChildren(leftWatch);
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
}
