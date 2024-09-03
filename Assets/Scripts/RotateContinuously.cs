using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateContinuously : MonoBehaviour
{
    [Range(-1, 1)]
    [SerializeField, Tooltip("Rotation on the x axis")]
    private float xRot = 0f;

    [Range(-1, 1)]
    [SerializeField, Tooltip("Rotation on the y axis")]
    private float yRot = 0f;

    [Range(-1, 1)]
    [SerializeField, Tooltip("Rotation along the z axis")]
    private float zRot = 0f;

    [SerializeField, Tooltip("Speed of rotation")]
    private float rotSpeed = 1f;

    internal Vector3 rotVector;

    // Creates the rotation vector from the <axis>Rot variables
    void Start()
    {
        rotVector = new Vector3(xRot, yRot, zRot);
    }

    // Continuously rotates the object using the rotation vector
    void Update()
    {
        transform.Rotate(rotVector * (rotSpeed * Time.deltaTime), Space.Self);
    }
}
