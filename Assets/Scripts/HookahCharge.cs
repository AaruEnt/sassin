using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class HookahCharge : MonoBehaviour
{
    public AnimationCurve upDownBob;
    public float multiplier = 0.001f;
    public bool randomizeStartTime = false;

    public float degressPerSecondRotation = 25f;
    private float _startY;
    private float _startOffset;

    void Start()
    {
        _startY = transform.localPosition.y;
        _startOffset = (float)Randomizer.GetDouble(upDownBob.length);
        if (randomizeStartTime)
            _startOffset = 0f;
    }

    void Update()
    {
        // Up/Down bobbing
        transform.localPosition = new Vector3(transform.localPosition.x, _startY + upDownBob.Evaluate((((Time.time + _startOffset) * 0.25f) % upDownBob.length)) * multiplier, transform.localPosition.z);

        // Rotating
        transform.Rotate(Vector3.up, degressPerSecondRotation * Time.deltaTime, Space.World);
    }
}
