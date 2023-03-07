using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollCollisionHelper : MonoBehaviour
{
    public Stats stats;
    // Start is called before the first frame update
    void OnCollisionEnter(Collision col)
    {
        stats.OnCollisionEnter(col);
    }

    void OnCollisionExit(Collision col)
    {
        stats.OnCollisionExit(col);
    }
}
