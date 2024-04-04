using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;
using System.Diagnostics;

public class TriggerEvent : MonoBehaviour
{
    public GameObject obj;
    [Tag]
    public string tag;
    public UnityEvent<Collider> t;
    public UnityEvent<Collider> te;

    void OnTriggerEnter(Collider col)
    {
        if ((obj && obj == col.gameObject) || (tag != "" && col.gameObject.CompareTag(tag)))
            t.Invoke(col);
    }

    void OnTriggerExit(Collider col)
    {
        if ((obj && obj == col.gameObject) || (tag != "" && col.gameObject.CompareTag(tag)))
            te.Invoke(col);
    }
}
