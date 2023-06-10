using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class HookahCharge : MonoBehaviour
{
    public AnimationCurve upDownBob;
    public float multiplier = 0.001f;

    public float degressPerSecondRotation = 25f;
    private float _startY;

    void Start()
    {
        _startY = transform.localPosition.y;
    }

    void Update()
    {
        // Up/Down bobbing
        transform.localPosition = new Vector3(transform.localPosition.x, _startY + upDownBob.Evaluate(((Time.time * 0.25f) % upDownBob.length)) * multiplier, transform.localPosition.z);

        // Rotating
        transform.Rotate(Vector3.up, degressPerSecondRotation * Time.deltaTime, Space.World);
    }
}
