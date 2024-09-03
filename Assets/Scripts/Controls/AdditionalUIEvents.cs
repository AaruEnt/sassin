using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AdditionalUIEvents : MonoBehaviour
{
    public UnityEvent startPointEvent;
    public UnityEvent endPointEvent;
    internal void OnPointStarted()
    {
        startPointEvent.Invoke();
    }

    internal void OnPointEnded()
    {
        endPointEvent.Invoke();
    }
}
