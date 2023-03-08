using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollCollisionHelper : MonoBehaviour
{
    public Stats stats;
    // Start is called before the first frame update
    void OnCollisionEnter(Collision col)
    {
        //if (col.body as Rigidbody)
        //    Debug.Log(string.Format("{0} hit {1}", col.gameObject.name, gameObject.name));
        stats.OnCollisionEnter(col);
    }

    void OnCollisionExit(Collision col)
    {
        stats.OnCollisionExit(col);
    }
}
