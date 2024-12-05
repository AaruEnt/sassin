using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.Events;


public class OpenXRControllerEvent : MonoBehaviour{
    public InputActionProperty action;
    public UnityEvent inputEvent;

    protected virtual void OnEnable(){
        action.action.Enable();
        action.action.performed += OnInputEvent;
    }

    protected virtual void OnDisable(){
        action.action.performed -= OnInputEvent;
    }

    protected virtual void OnInputEvent(InputAction.CallbackContext context) {
        inputEvent?.Invoke();
    }
}
