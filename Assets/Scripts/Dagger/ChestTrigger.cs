using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Autohand;
using JointVR;

public class ChestTrigger : MonoBehaviour
{
    [SerializeField, Tooltip("The trigger button")]
    private SteamVR_Action_Boolean triggerAction;

    [SerializeField, Tooltip("The dagger summon component on the dagger to be summoned")]
    private DaggerSummon ds;

    // The hands currently within the trigger area. Was originally public to debug
    private List<Hand> hands = new List<Hand>();

    [SerializeField, Tooltip("The stab manager on the dagger, used to remove all stabs on summon")]
    private StabManager stabM;

    [SerializeField, Tooltip("The placepoint where the dagger goes on summon")]
    private PlacePoint place;

    private bool actionSet = false;


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
