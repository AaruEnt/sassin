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
    private List<GameObject> ignore;


    [Header("Events")]
    [SerializeField, Tooltip("Activates on trigger entered")]
    private UnityEvent OnCol;

    private float cd = 0f;

    void Update()
    {
        if (cd > 0f)
            cd -= Time.deltaTime;
    }

    // Called when trigger collider entered
    void OnTriggerEnter(Collider col) {
        if ((layer != -1 && col.gameObject.layer != layer) || ignore.Contains(col.gameObject) && cd <= 0f)
            return;
        Debug.Log(col.gameObject.name);
        OnCol.Invoke();
    }

    public void SetCD(float toSet)
    {
        if (toSet <= 0f)
            return;
        cd = toSet;
    }
}
