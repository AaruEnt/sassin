using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;
using NaughtyAttributes;

public class DaggerSummon : MonoBehaviour
{
    public PlacePoint place;

    public DaggerAnimator dAnim;

    internal bool isHeld = false; 

    [Button]
    public void SummonKnife() { SummonToPlacePoint(); }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SummonToPlacePoint()
    {
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

    public void SetIsHeld(bool held)
    {
        isHeld = held;
    }

    public void OverrideParent()
    {
        if (transform.parent == place.transform)
            transform.parent = null;
    }
}
