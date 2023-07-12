using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class CheckCanSee : MonoBehaviour
{
    public UnityEvent OnSee;
    public GameObject eyes;
    [Tag]
    public string tag;
    public LayerMask mask;

    private bool canSee = false;
    private Transform target;

    void Update()
    {
        RaycastHit hitinfo;
        if (canSee)
        {
            bool tmp = Physics.Linecast(eyes.transform.position, target.transform.position, out hitinfo);
            if (tmp && hitinfo.collider.gameObject.CompareTag(tag)) {
                OnSee.Invoke();
            } else
            {
                UnityEngine.Debug.Log(hitinfo.collider.name);
            }
        }
    }


    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag(tag))
        {
            canSee = true;
            target = col.transform;
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.CompareTag(tag))
        {
            canSee = true;
            target = col.transform;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag(tag))
        {
            canSee = false;
        }
    }
}
