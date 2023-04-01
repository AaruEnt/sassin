using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollCollisionHelper : MonoBehaviour
{
    [SerializeField, Tooltip("The 'stats' component in a parent object")]
    private Stats stats;

    // Passes the collision on to the parent
    void OnCollisionEnter(Collision col)
    {
        stats.OnCollisionEnter(col);
    }

    void OnCollisionExit(Collision col)
    {
        stats.OnCollisionExit(col);
    }
}
