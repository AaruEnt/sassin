using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoutTargetActivation : MonoBehaviour
{
    private string targetTag = "ScoutTarget";
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(targetTag))
        {
            var t = other.GetComponent<ScoutTarget>();
            if (!t)
            {
                Debug.LogWarning("Scout target not found on tagged object: " + other.gameObject.name);
            }
            else
            {
                t.onScoutTargetEntered.Invoke();
                t.enabled = false;
            }
        }
    }
}
