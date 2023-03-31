using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleCollision : MonoBehaviour
{
    [SerializeField, Tooltip("The rigidbody to toggle collision on")]
    private Rigidbody rb;

    public void EnableAllCollision()
    {
        rb.detectCollisions = false;
    }

    public void DisableAllCollision()
    {
        rb.detectCollisions = true;
    }
}
