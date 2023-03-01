using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Autohand;
using JointVR;

public class ChestTrigger : MonoBehaviour
{
    public SteamVR_Action_Boolean triggerAction;

    public DaggerSummon ds;

    public List<Hand> hands = new List<Hand>();

    public StabManager stabM;

    public PlacePoint place;

    private bool actionSet = false;

    void Update()
    {
        //Debug.Log(hands.Count);
        //if (hands.Count > 0)
        //    place.canRelease = true;
        //else
        //    place.canRelease = false;
    }

    void OnTriggerEnter(Collider col)
    {
        if (!col.attachedRigidbody)
            return;
        Hand h = col.attachedRigidbody.transform.GetComponent<Hand>();
        if (h && !hands.Contains(h)) {
            if (!actionSet)
            {
                actionSet = true;
                triggerAction.onStateDown += SummonDagger;
            }
            hands.Add(h);
        }
        //if (!place.enabled)
        //    place.enabled = true;
    }

    void OnTriggerExit(Collider col)
    {
        if (!col.attachedRigidbody)
            return;
        Hand h = col.attachedRigidbody.transform.GetComponent<Hand>();
        if (h && hands.Contains(h))
        {
            hands.Remove(h);
            if (hands.Count == 0)
            {
                actionSet = false;
                triggerAction.onStateDown -= SummonDagger;
            }
        }
        //if (place.enabled && place.placedObject)
        //    place.enabled = false;
    }

    void SummonDagger(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        //Debug.Log(fromSource);
        stabM.UnstabAll();
        ds.SummonToPlacePoint();
    }
}
