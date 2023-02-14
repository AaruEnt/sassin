using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateContinuously : MonoBehaviour
{
    [Range(-1, 1)]
    public float xRot = 0f;
    [Range(-1, 1)]
    public float yRot = 0f;
    [Range(-1, 1)]
    public float zRot = 0f;
    public float rotSpeed = 1f;

    private Vector3 rotVector;
    // Start is called before the first frame update
    void Start()
    {
        rotVector = new Vector3(xRot, yRot, zRot);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotVector * (rotSpeed * Time.deltaTime), Space.Self);
    }
}
