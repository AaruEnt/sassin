using UnityEngine;
using System.Collections.Generic;

// Written by Steve Streeting 2017
// License: CC0 Public Domain http://creativecommons.org/publicdomain/zero/1.0/

/// <summary>
/// Component which will the object's light while active by changing its
/// intensity based on an amplitude value. The flickering can be
/// sharp or smoothed depending on the value of the smoothing parameter.
///
/// Just activate / deactivate this component as usual to pause / resume flicker
/// </summary>
public class LightFlickerEffect : MonoBehaviour
{


    float minIntensity = 0f;
    float maxIntensity = 1f;
    float initialIntensity;

    [Tooltip("How much amplitude will the randomness have; 0 = no flicker")]
    [Range(0f, 1f)]
    public float amplitude = 0.2f;
     
    [Tooltip("How much to smooth out the randomness; lower values = sparks, higher = lantern")]
    [Range(1, 50)]
    public int smoothing = 5;

    // Continuous average calculation via FIFO queue
    // Saves us iterating every time we update, we just change by the delta
    Queue<float> smoothQueue;
    float lastSum = 0;


    /// <summary>
    /// Reset the randomness and start again. You usually don't need to call
    /// this, deactivating/reactivating is usually fine but if you want a strict
    /// restart you can do.
    /// </summary>
    public void Reset()
    {
        smoothQueue.Clear();
        lastSum = 0;
    }

    void Start()
    {

        initialIntensity = this.GetComponent<Light>().intensity;
        smoothQueue = new Queue<float>(smoothing);

    }

    void Update()
    {
        if (this.GetComponent<Light>() == null)
            return;

        // pop off an item if too big
        while (smoothQueue.Count >= smoothing)
        {
            lastSum -= smoothQueue.Dequeue();
        }

        // Generate random new item, calculate new average
        minIntensity = initialIntensity * (1 - amplitude) ;
        maxIntensity = initialIntensity * (1 + amplitude) ;

        float newVal = Random.Range(minIntensity, maxIntensity);
        smoothQueue.Enqueue(newVal);
        lastSum += newVal;

        // Calculate new smoothed average
        this.GetComponent<Light>().intensity = lastSum / (float)smoothQueue.Count;
    }

}