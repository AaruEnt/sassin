using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaintainLayer : MonoBehaviour
{
    [SerializeField, Tooltip("The layer to be maintained. Will never allow another layer to be on this object")]
    private int layer;

    // Update is called once per frame
    void Update()
    {
        if (gameObject.layer != layer)
            gameObject.layer = layer;
    }
}
