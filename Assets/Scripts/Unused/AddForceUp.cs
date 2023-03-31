using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class AddForceUp : MonoBehaviour
{
    public float thrust = 2f;
    [Button]
    private void moveUp() { MoveUp(); }


    private void MoveUp()
    {
        var rb = GetComponent<Rigidbody>();

        rb.AddRelativeForce(transform.up * thrust, ForceMode.Impulse);
    }
}
