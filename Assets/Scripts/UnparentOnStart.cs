using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnparentOnStart : MonoBehaviour
{
    // Sets parent to be null on start
    void Start()
    {
        transform.parent = null;
    }
}
