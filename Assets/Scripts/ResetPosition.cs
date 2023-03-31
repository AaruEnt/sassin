using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    // Resets the local position to 0,0,0
    public void ResetPos()
    {
        transform.localPosition = Vector3.zero;
    }
}
