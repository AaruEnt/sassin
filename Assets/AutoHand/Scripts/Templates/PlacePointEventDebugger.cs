using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlacePoint))]
public class PlacePointEventDebugger : MonoBehaviour
{
    PlacePoint placePoint;

    void OnEnable()
    {
        placePoint = GetComponent<PlacePoint>();
        placePoint.OnPlaceEvent += (PlacePoint point, Grabbable grabbable) => { Debug.Log(name + "On Place: " + Time.time); };
        placePoint.OnRemoveEvent += (PlacePoint point, Grabbable grabbable) => { Debug.Log(name + "On Remove: " + Time.time); };
        placePoint.OnHighlightEvent += (PlacePoint point, Grabbable grabbable) => { Debug.Log(name + "On Highlight: " + Time.time); };
        placePoint.OnStopHighlightEvent += (PlacePoint point, Grabbable grabbable) => { Debug.Log(name + "On Stop Highlight: " + Time.time); };
    }


    void OnDisable()
    {
        placePoint = GetComponent<PlacePoint>();
        placePoint.OnPlaceEvent -= (PlacePoint point, Grabbable grabbable) => { Debug.Log(name + "On Place: " + Time.time); };
        placePoint.OnRemoveEvent -= (PlacePoint point, Grabbable grabbable) => { Debug.Log(name + "On Remove: " + Time.time); };
        placePoint.OnHighlightEvent -= (PlacePoint point, Grabbable grabbable) => { Debug.Log(name + "On Highlight: " + Time.time); };
        placePoint.OnStopHighlightEvent -= (PlacePoint point, Grabbable grabbable) => { Debug.Log(name + "On Stop Highlight: " + Time.time); };
    }
}
