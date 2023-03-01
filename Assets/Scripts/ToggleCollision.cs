using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleCollision : MonoBehaviour
{
    public Rigidbody rb;

    public void EnableAllCollision()
    {
        rb.detectCollisions = false;
    }

    public void DisableAllCollision()
    {
        rb.detectCollisions = true;
    }
}
