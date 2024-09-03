using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPointEvent : MonoBehaviour
{
    HandCanvasPointer pointer;
    GameObject lastPoint;
    private void Start()
    {
        pointer = GetComponent<HandCanvasPointer>();
    }

    public void CallPointEvent(Vector3 point, GameObject target)
    {
        if (!target)
            target = pointer.currTarget;
        UnityEngine.Debug.Log("start point " + target?.name);
        AdditionalUIEvents additionalUIEvents = target?.GetComponent<AdditionalUIEvents>();
        if (additionalUIEvents == null)
            return;
        additionalUIEvents.OnPointStarted();
        lastPoint = target;
    }

    public void CallEndPointEvent(Vector3 point, GameObject target)
    {
        if (!target)
            target = pointer.currTarget != null ? pointer.currTarget : lastPoint;
        UnityEngine.Debug.Log("stop point " + target?.name);
        AdditionalUIEvents additionalUIEvents = target?.GetComponent<AdditionalUIEvents>();
        if (additionalUIEvents == null)
            return;
        additionalUIEvents.OnPointEnded();
    }
}
