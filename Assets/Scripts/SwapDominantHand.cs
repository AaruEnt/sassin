using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

// Controlls collision between hands and watches, as well as the visibility of models attached to watch
public class SwapDominantHand : MonoBehaviour
{
    [SerializeField, Tooltip("Is the watch currently on the left hand")]
    private bool WatchLeftHand = true; // assumes watch on left hand

    [SerializeField, Tooltip("The left watch")]
    private GameObject leftWatch;

    [SerializeField, Tooltip("The right watch")]
    private GameObject rightWatch;

    [SerializeField, Tooltip("The left hand")]
    private GameObject leftHand;

    [SerializeField, Tooltip("The right hand")]
    private GameObject rightHand;

    [SerializeField, Tooltip("The gameobject parented to all visible left watch models")]
    private GameObject leftModels;

    [SerializeField, Tooltip("The gameobject parented to all visible right watch models")]
    private GameObject rightModels;

    [SerializeField, Tooltip("The grabbable component on the left watch")]
    private Grabbable leftGrab;

    [SerializeField, Tooltip("The grabbable component on the right watch")]
    private Grabbable rightGrab;

    private Collider[] leftHandColliders;
    private Collider[] rightHandColliders;
    private Collider[] leftWatchColliders;
    private Collider[] rightWatchColliders;

    // Grab all unserialized vars, and set visible watch to correct hand
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
            SwapToLeft();
        } else
        {
            SwapToRight();
        }
    }

    // Toggle watch between left/right hands
    public void OnSwapCalled()
    {
        if (WatchLeftHand)
            SwapToRight();
        else
            SwapToLeft();
    }

    // DIsable right watch and enable left watch
    void SwapToLeft()
    {
        WatchLeftHand = true;
        EnableAllModels(leftModels);
        DisableAllModels(rightModels);
        rightGrab.enabled = false;
        leftGrab.enabled = true;
        StartCoroutine(EnableCollision(leftWatchColliders, rightHandColliders));
        StartCoroutine(DisableCollision(rightWatchColliders, leftHandColliders));
    }

    // Disable left watch and enable right watch
    void SwapToRight()
    {
        WatchLeftHand = false;
        DisableAllModels(leftModels);
        EnableAllModels(rightModels);
        rightGrab.enabled = true;
        leftGrab.enabled = false;
        StartCoroutine(EnableCollision(rightWatchColliders, leftHandColliders));
        StartCoroutine(DisableCollision(leftWatchColliders, rightHandColliders));
    }

    // disable all mesh renderers in children of target object
    void DisableAllModels(GameObject obj)
    {
        Renderer[] rList = obj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer m in rList)
        {
            m.enabled = false;
        }
    }

    // enable all mesh renderers in children of target object
    void EnableAllModels(GameObject obj)
    {
        Renderer[] rList = obj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer m in rList)
        {
            m.enabled = true;
        }
    }

    // Disable collision between watch and hand for disabled watch
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

    // ENable collision between watch and hand for enabled watch
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
