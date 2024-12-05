using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Briefcase : MonoBehaviour
{
    public Grabbable grabbable;
    public Transform openCloseTransform;

    public float openCloseSpeed = 1;
    public AnimationCurve openCloseCurve;

    public float openAngle = 110;
    public float closeAngle = -10;

    bool isOpen = true;
    float openCloseState = 1;

    Vector3 targetOpenRotation;
    Vector3 targetCloseRotation;


    private void OnEnable() {
        targetOpenRotation = new Vector3(openAngle, 0, 0);
        targetCloseRotation = new Vector3(closeAngle, 0, 0);
        openCloseState = isOpen ? 1 : 0;
        openCloseTransform.localRotation = Quaternion.Euler(Vector3.Lerp(targetCloseRotation, targetOpenRotation, openCloseCurve.Evaluate(openCloseState)));
    }

    public void Open() {
        isOpen = true;
    }

    public void Close() {
        isOpen = false;
    }


    void Update() {
        if(isOpen && openCloseState < 1) {
            openCloseState += Time.deltaTime * openCloseSpeed;
            openCloseState = Mathf.Clamp01(openCloseState);
            openCloseTransform.localRotation = Quaternion.Euler(Vector3.Lerp(targetCloseRotation, targetOpenRotation, openCloseCurve.Evaluate(openCloseState)));
            if(openCloseState >= 1) {
                OnOpen();
            }
        }
        else if(!isOpen && openCloseState > 0) {
            openCloseState -= Time.deltaTime * openCloseSpeed;
            openCloseState = Mathf.Clamp01(openCloseState);
            openCloseTransform.localRotation = Quaternion.Euler(Vector3.Lerp(targetCloseRotation, targetOpenRotation, openCloseCurve.Evaluate(openCloseState)));
            if(openCloseState <= 0) {
                OnClose();
            }
        }
    }


    public void OnOpen() {
        foreach(var placepoint in grabbable.childPlacePoints) {
            placepoint.enabled = true;
        }
    }


    public void OnClose() {
        foreach(var placepoint in grabbable.childPlacePoints) {
            placepoint.enabled = false;
        }
    }
}
