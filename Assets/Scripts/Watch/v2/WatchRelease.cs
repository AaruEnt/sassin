using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WatchRelease : MonoBehaviour
{
    [SerializeField, Tooltip("When this rigidbody enters the trigger, calls the OnRelease")]
    private Rigidbody exitRB;

    [SerializeField, Tooltip("Called when above rigidbody enters this trigger")]
    private UnityEvent OnRelease;


    void OnTriggerEnter(Collider col)
    {
        if (col.attachedRigidbody == exitRB)
        {
            OnRelease.Invoke();
        }
    }
}
