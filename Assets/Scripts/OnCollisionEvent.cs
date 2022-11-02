using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnCollisionEvent : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField, Tooltip("")]
    private float layer = -1;


    [Header("References")]
    [SerializeField, Tooltip("Gameobject to ignore")]
    private GameObject ignore;


    [Header("Events")]
    [SerializeField, Tooltip("Activates on trigger entered")]
    private UnityEvent OnCol;

    // Called when trigger collider entered
    void OnTriggerEnter(Collider col) {
        if ((layer != -1 && col.gameObject.layer != layer) || col.gameObject == ignore)
            return;
        Debug.Log(col.gameObject.name);
        OnCol.Invoke();
    }
}
