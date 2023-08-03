using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

public class HandHelper : MonoBehaviour
{
    public Autohand.Hand hand;
    private FixedJoint j;

    // Update is called once per frame
    void Update()
    {
        if (!hand.GetHeld() && transform.TryGetComponent<FixedJoint>(out j))
            Destroy(j);
    }
}
