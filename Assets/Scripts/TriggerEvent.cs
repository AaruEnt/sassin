using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    public GameObject obj;
    [Tag]
    public string tag;
    public UnityEvent t;

    void OnTriggerEnter(Collider col)
    {
        Debug.Log(col.gameObject.name);
        if ((obj && obj == col.gameObject) || (tag != "" && col.gameObject.CompareTag(tag)))
            t.Invoke();
    }
}
