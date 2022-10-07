using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaintainLayer : MonoBehaviour
{
    public int layer;

    // Update is called once per frame
    void Update()
    {
        if (gameObject.layer != layer)
            gameObject.layer = layer;
    }
}
