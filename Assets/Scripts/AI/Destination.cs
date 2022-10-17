using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destinations : MonoBehaviour
{
    // All child destination objects of parent
    public List<Vector3> destinationPoints;
    // Event signalling all destination objects are loaded
    public event EventHandler onDestinationsGenerated;
    // Start is called before the first frame update
    void Start()
    {
        EventHandler handler = onDestinationsGenerated;
        destinationPoints = new List<Vector3>();
        foreach (var point in GetComponentsInChildren<Transform>()) {
            if (!(point.position == transform.position))
                destinationPoints.Add(point.position);
        }
        EventArgs e = new EventArgs();
        handler?.Invoke(this, e);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
