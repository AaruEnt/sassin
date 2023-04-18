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

    [SerializeField, Tooltip("The right AUtohand hand")]
    private Hand rHand;

    [SerializeField, Tooltip("The left Autohand hand")]
    private Hand lHand;


    private bool actionSetRight = false;
    private bool actionSetLeft = false;

    void Update()
    {
        //if (actionSetRight && triggerAction.GetStateDown(SteamVR_Input_Sources.RightHand))
        //{
        //    SummonDagger(rHand);
        //}
        //if (actionSetLeft && triggerAction.GetStateDown(SteamVR_Input_Sources.LeftHand))
        //{
        //    SummonDagger(lHand);
        //}
    }


    void OnTriggerEnter(Collider col)
    {
        if (!col.attachedRigidbody)
            return;
        Hand h = col.attachedRigidbody.transform.GetComponent<Hand>();
        if (h && !hands.Contains(h)) {
            if (h == rHand)
                actionSetRight = true;
            if (h == lHand)
                actionSetLeft = true;
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
            if (h == rHand)
                actionSetRight = false;
            if (h == lHand)
                actionSetLeft = false;
        }
        //if (place.enabled && place.placedObject)
        //    place.enabled = false;
    }

    public void SummonDagger(Hand hand)
    {
        if (actionSetRight && hand == rHand)
        {
            stabM.UnstabAll();
            //ds.SummonToPlacePoint();
            ds.SummonToGrabPoint(hand);
        }
        if (actionSetLeft && hand == lHand)
        {
            stabM.UnstabAll();
            //ds.SummonToPlacePoint();
            ds.SummonToGrabPoint(hand);
        }
    }
}
