using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;

public class SetHeightByCC : MonoBehaviour
{

    public PlacePoint place;
    internal Grabbable placedObj = null;

    private float yOffset;


    // Start is called before the first frame update
    void Start()
    {
        place.OnPlaceEvent += OnPlaced;
        place.OnRemoveEvent += OnRemoved;
        yOffset = Camera.main.transform.localPosition.y - transform.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = Camera.main.transform.localPosition;
        pos.y -= yOffset;
        transform.localPosition = pos;

        if (placedObj)
            placedObj.transform.position = place.placedOffset.transform.position;

        var cAngles = Camera.main.transform.eulerAngles;
        cAngles.x = 0f;
        cAngles.z = 0f;
        transform.eulerAngles = cAngles;
    }

    void OnPlaced(PlacePoint point, Grabbable p)
    {
        placedObj = p;
    }

    void OnRemoved(PlacePoint point, Grabbable p)
    {
        placedObj = null;
    }
}
