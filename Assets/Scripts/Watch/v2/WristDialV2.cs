using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

public class WristDialV2 : PhysicsGadgetHingeAngleReader
{
    [SerializeField, Tooltip("The number of options to set the dial to")]
    private int numberOfOptions;
    [SerializeField, Tooltip("The dial position represented as the timezone it is pointing to")]
    internal int dialPosToTimezone;
    [SerializeField, Tooltip("The collider the player grabs")]
    private Collider grabCol;
    [SerializeField, Tooltip("The base of the button")]
    private Collider baseCol;
    [SerializeField, Tooltip("The audio source that plays the click sound")]
    private AudioSource click;

    [SerializeField, Tooltip("Reference to the time zone manager in the scene")]
    private TimeZoneManager manager;

    public List<GameObject> ignoreCollision;
    private List<Collider> ignoreColliders;
    private HingeJoint hJoint;
    private int lastDialPos;

    // Sets a bunch of collision ignores, grabs private vars, and sets timezone
    protected override void Start()
    {
        base.Start();
        foreach (var obj in ignoreCollision)
        {
            foreach (var col in obj.transform.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(grabCol, col);
                Physics.IgnoreCollision(baseCol, col);
            }
        }
        hJoint = GetComponent<HingeJoint>();
        dialPosToTimezone = ConvertValueToPos();
        lastDialPos = dialPosToTimezone;
    }

    // Updates the dialPosToTimezone var and plays the click sound if needed
    void Update()
    {
        dialPosToTimezone = ConvertValueToPos();
        if (lastDialPos != dialPosToTimezone && click)
        {
            click.Stop();
            click.Play();
        }
        lastDialPos = dialPosToTimezone;
    }

    // Gets the value of the dial from GetValueReal and converts it to the timezone the position indicates
    int ConvertValueToPos()
    {
        float val = GetValueReal();
        float num = 1f / numberOfOptions;

        int res = 0;
        float tmp = 0f;

        

        while (tmp <= val && tmp <= 100f)
        {
            tmp += num;
            res += 1;
        }

        res = res > numberOfOptions ? numberOfOptions : res <= 0 ? 1 : res;

        return (res);
    }

    // Converts the rotation of the dial to a float in range from 0f-1f
    public float GetValueReal()
    {
        Vector3 angles = transform.localRotation.eulerAngles;
        float maxAngle = hJoint.limits.max - hJoint.limits.min;
        
        return angles.y / maxAngle;
    }

    // Tells the timezone manager to change the timezone
    public void OnSetTimeZone()
    {
        manager.SetTimeZone(dialPosToTimezone);
    }
}
