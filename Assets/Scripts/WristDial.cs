using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;
using System;

public class WristDial : PhysicsGadgetHingeAngleReader
{
    [Header("Variables")]
    [SerializeField, Tooltip("Number of options for the dial")]
    private int numberOfOptions = 4;


    [Header("Layers")]
    [SerializeField, Tooltip("The layer this object should be on by default")]
    private int layer;

    [SerializeField, Tooltip("An additional acceptable layer (sanity check)")]
    private int layer2;

    
    [Header("References")]
    [SerializeField, Tooltip("Reference to the time zone manager in the scene")]
    private TimeZoneManager manager;


    // internal vars

    // The current timezone as an int converted from dial position
    internal int dialPosToTimezone;


    // private vars

    // the last time zone recorded
    private int lastTimeZone = -1;

    // runs the base start before ensuring the current layer is correct
    new void Start() {
        base.Start();
        if (transform.gameObject.layer != layer && transform.gameObject.layer != layer2)
            transform.gameObject.layer = layer;
    }

    // updates dialPosToTimezone for internal use
    void Update() {
        if (lastTimeZone == -1)
            OnActivate();
        dialPosToTimezone = ConvertValueToPos();
    }

    // converts the position of the dial to an int representation of which option is selected
    int ConvertValueToPos() {
        float val = GetValue();
        float num = 2f / numberOfOptions;

        float res = (float)Math.Ceiling((val + 1) / num);
        res -= 1;
        if (res < 0)
            res = 0;

        return ((int)res);
    }

    // Called whenever the current timezone does not match the previous timezone
    // Tells the timezonemanager to change the timezone
    public void OnActivate() {
        int currTimeZone = ConvertValueToPos();

        if (lastTimeZone == -1)
            lastTimeZone = currTimeZone;

        //Debug.Log(string.Format("{0}, {1}", lastTimeZone, currTimeZone));

        if (currTimeZone != lastTimeZone) {
            manager.SetTimeZone(currTimeZone + 1);
            lastTimeZone = currTimeZone;
        }
    }
}
