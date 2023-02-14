using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

// Controlls collision between hands and watches, as well as the visibility of models attached to watch
public class SwapDominantHand : MonoBehaviour
{
    public bool WatchLeftHand = true; // assumes watch on left hand
    public GameObject leftWatch;
    public GameObject rightWatch;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject leftModels;
    public GameObject rightModels;
    public Grabbable leftGrab;
    public Grabbable rightGrab;

    private Collider[] leftHandColliders;
    private Collider[] rightHandColliders;
    private Collider[] leftWatchColliders;
    private Collider[] rightWatchColliders;

    // Start is called before the first frame update
    void Start()
    {
        leftHandColliders = leftHand.GetComponentsInChildren<Collider>();
        rightHandColliders = rightHand.GetComponentsInChildren<Collider>();
        leftWatchColliders = leftWatch.GetComponentsInChildren<Collider>();
        rightWatchColliders = rightWatch.GetComponentsInChildren<Collider>();

        foreach (Collider c in leftHandColliders)
        {
            foreach (Collider cl in leftWatchColliders)
            {
                Physics.IgnoreCollision(c, cl);
            }
        }
        foreach (Collider c in rightHandColliders)
        {
            foreach (Collider cl in rightWatchColliders)
            {
                Physics.IgnoreCollision(c, cl);
            }
        }
        if (WatchLeftHand)
        {
            //EnableAllChildren(leftWatch);
            //DisableAllChildren(rightWatch);
            SwapToLeft();
        } else
        {
            SwapToRight();
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
        rightGrab.enabled = false;
        leftGrab.enabled = true;
        StartCoroutine(EnableCollision(leftWatchColliders, rightHandColliders));
        StartCoroutine(DisableCollision(rightWatchColliders, leftHandColliders));
    }

    void SwapToRight()
    {
        WatchLeftHand = false;
        //EnableAllChildren(rightWatch);
        //DisableAllChildren(leftWatch);
        DisableAllModels(leftModels);
        EnableAllModels(rightModels);
        rightGrab.enabled = true;
        leftGrab.enabled = false;
        StartCoroutine(EnableCollision(rightWatchColliders, leftHandColliders));
        StartCoroutine(DisableCollision(leftWatchColliders, rightHandColliders));
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

    private IEnumerator DisableCollision(Collider[] watch, Collider[] hand)
    {
        foreach (Collider c in watch)
        {
            foreach (Collider cl in hand)
            {
                Physics.IgnoreCollision(c, cl);
            }
        }
        yield return null;
    }

    private IEnumerator EnableCollision(Collider[] watch, Collider[] hand)
    {
        foreach (Collider c in watch)
        {
            foreach (Collider cl in hand)
            {
                Physics.IgnoreCollision(c, cl, false);
            }
        }
        yield return null;
    }
}
