using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class UnparentOnStart : MonoBehaviour
{
    public bool setNewPosRot = false;
    [ShowIf("setNewPosRot")]
    public Vector3 newPos;
    [ShowIf("setNewPosRot")]
    public Quaternion newRot;
    // Sets parent to be null on start
    void Start()
    {
        transform.parent = null;
        if (setNewPosRot)
        {
            transform.position = newPos;
            transform.rotation = newRot;
        }
    }
}
