using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UISliderEvents : MonoBehaviour, IPointerUpHandler
{
    public UnityEvent onPointerUp;
    public void OnPointerUp(PointerEventData eventData)
    {
        onPointerUp.Invoke();
    }
}
