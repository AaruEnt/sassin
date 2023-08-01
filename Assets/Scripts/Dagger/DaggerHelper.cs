using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

public class DaggerHelper : MonoBehaviour
{
    private Grabbable g;
    private Rigidbody r;

    public Transform followObjTMP;
    public AutoHandPlayer player;

    void Start()
    {
        g = GetComponent<Grabbable>();
        r = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        g.OnReleaseEvent += OnDaggerRelease;
    }

    void OnDisable()
    {
        g.OnReleaseEvent -= OnDaggerRelease;
    }

    void Update()
    {
        if (g.IsHeld() && transform.parent == null)
        {
            transform.parent = followObjTMP;
        }
        if (player.body.velocity.magnitude >= 5f)
            g.throwPower = 3f;
        else
            g.throwPower = 1.5f;
    }

    void OnDaggerRelease(Hand hand, Grabbable g)
    {
        
    }

    internal void UpdateParent()
    {
        if (g.IsHeld())
            transform.parent = followObjTMP;
    }
}
