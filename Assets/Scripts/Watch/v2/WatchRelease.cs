using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WatchRelease : MonoBehaviour
{
    public Rigidbody exitRB;
    public UnityEvent OnRelease;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.attachedRigidbody == exitRB)
        {
            OnRelease.Invoke();
        }
    }
}
