using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnCollisionEvent : MonoBehaviour
{
    public float layer = -1;
    public GameObject ignore;
    public UnityEvent OnCol;

    // Start is called before the first frame update
    void OnTriggerEnter(Collider col) {
        if ((layer != -1 && col.gameObject.layer != layer) || col.gameObject == ignore)
            return;
        Debug.Log(col.gameObject.name);
        OnCol.Invoke();
    }
}
