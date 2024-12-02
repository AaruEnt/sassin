using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OpenXRAutoHandAxisFingerBender : MonoBehaviour{
    public Hand hand;
    public InputActionProperty bendAction;

    [HideInInspector]
    public float[] bendOffsets;
    float lastAxis;

    public void OnEnable() {
        if(bendAction.action != null) bendAction.action.Enable();
    }

    void LateUpdate()
    {
        var currAxis = bendAction.action.ReadValue<float>();
        for (int i = 0; i < bendOffsets.Length; i++)
        {
            hand.fingers[i].bendOffset += (currAxis - lastAxis) * bendOffsets[i];
        }
        lastAxis = currAxis;
    }
}
