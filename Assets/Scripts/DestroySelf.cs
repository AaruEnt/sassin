using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class DestroySelf : MonoBehaviour
{
    [Tooltip("If left null, will destroy self")]
    public GameObject toDestroy;

    public float delay = 0f;

    public void CallDestroy()
    {
        if (toDestroy == null)
        {
            Destroy(this.gameObject, delay);
        }
        else
        {
            Destroy(toDestroy, delay);
        }
    }
}
