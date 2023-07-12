using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;
using Valve.VR;
using NaughtyAttributes;

public class DaggerSummon : MonoBehaviour
{
    [SerializeField, Tooltip("The place point that the dagger is positioned on")]
    private PlacePoint place;

    [SerializeField, Tooltip("The animation manager for the dagger")]
    private DaggerAnimator dAnim;

    internal bool isHeld = false;

    public bool tempDisableSummon = false;

    [Button]
    public void SummonKnife() { SummonToPlacePoint(); }

    // voids momentum and prepares dagger to be put in placepoint
    public void SummonToPlacePoint()
    {
        if (tempDisableSummon)
            return;
        if (isHeld)
            return;
        Rigidbody rb = GetComponent<Rigidbody>();
        Grabbable g = GetComponent<Grabbable>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = place.placedOffset.position;
        transform.parent = place.transform;
        place.Place(g);
        if (dAnim)
            dAnim.ToggleOff();
    }

    // Used to track if the dagger is held, to prevent summoning
    public void SetIsHeld(bool held)
    {
        isHeld = held;
    }

    // Overrides the parent to null
    public void OverrideParent()
    {
        if (transform.parent == place.transform)
            transform.parent = null;
    }

    public void SummonToGrabPoint(Hand hand, Grabbable gb = null)
    {
        if (tempDisableSummon)
            return;
        if (isHeld)
            return;
        Rigidbody rb = GetComponent<Rigidbody>();
        Grabbable g = GetComponent<Grabbable>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        transform.position = hand.transform.position;
        hand.TryGrab(g);
        if (dAnim)
            dAnim.ToggleOff();
    }

    public void SetEnabled(bool enabled)
    {
        tempDisableSummon = !enabled;
    }
}
