using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class MoveContinuously : MonoBehaviour
{
    public Vector3 direction;

    public float speed = 0.5f;


    // Update is called once per frame
    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
}
