using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeZoneManager : MonoBehaviour
{
    public int startTimeZone = 1;
    public GameObject particles;
    private int currTimeZone;
    private TimeZone[] timeZoneObjects;
    private List<GameObject> objsWithTime;
    private bool isFirstEnable = true;

    void Start() {
        currTimeZone = startTimeZone;
        timeZoneObjects = (TimeZone[])FindObjectsOfType<TimeZone>();
        objsWithTime = new List<GameObject>();
        foreach (var obj in timeZoneObjects) {
            objsWithTime.Add(obj.transform.gameObject);
        }
        UpdateTimeZone();
        isFirstEnable = false;
    }

    public void SetTimeZone(int time) {
        if (time > 0 && time < 5) {
            currTimeZone = time;
            UpdateTimeZone();
        }
    }

    private void UpdateTimeZone() {
        if (!isFirstEnable)
            particles.SetActive(true);
        foreach(var obj in objsWithTime) {
            if (!obj) {
                continue;
            }
            
            TimeZone t = obj.GetComponent<TimeZone>();
            if (currTimeZone == 1) {
                if (t.time != timeZone.timeZone1) {
                    DisableObject(obj);
                } else {
                    EnableObject(obj);
                }
            }
            if (currTimeZone == 2) {
                if (t.time != timeZone.timeZone2) {
                    DisableObject(obj);
                } else {
                    EnableObject(obj);
                }
            }
            if (currTimeZone == 3) {
                if (t.time != timeZone.timeZone3) {
                    DisableObject(obj);
                } else {
                    EnableObject(obj);
                }
            }
            if (currTimeZone == 4) {
                if (t.time != timeZone.timeZone4) {
                    DisableObject(obj);
                } else {
                    EnableObject(obj);
                }
            }
        }
    }

    private void DisableObject(GameObject obj) {
        if (!obj)
            return;

        //Renderer[] r = obj.GetComponentsInChildren<Renderer>(true);
        Collider[] c = obj.GetComponentsInChildren<Collider>(true);
        Rigidbody[] ri = obj.GetComponentsInChildren<Rigidbody>(true);
        Transform[] tr = obj.GetComponentsInChildren<Transform>(true);
        TimeZone tz = obj.GetComponent<TimeZone>();

        foreach (var col in c) {
            col.enabled = false;
        }

        //foreach (var ren in r) {
        //    ren.enabled = false;
        //}

        foreach (var rig in ri) {
            rig.useGravity = false;
            rig.isKinematic = true;
        }

        foreach (var t in tr) {
            t.gameObject.layer = tz.previewLayer;
        }
    }

    private void EnableObject(GameObject obj) {
        if (!obj)
            return;

        //Renderer[] r = obj.GetComponentsInChildren<Renderer>(true);
        Collider[] c = obj.GetComponentsInChildren<Collider>(true);
        Rigidbody[] ri = obj.GetComponentsInChildren<Rigidbody>(true);
        Transform[] tr = obj.GetComponentsInChildren<Transform>(true);

        foreach (var col in c) {
            col.enabled = true;
        }

        //foreach (var ren in r) {
        //    ren.enabled = true;
        //}

        foreach (var rig in ri) {
            rig.useGravity = true;
            rig.isKinematic = false;
        }

        TimeZone tz = obj.GetComponent<TimeZone>();

        foreach (var t in tr) {
            t.gameObject.layer = tz.layer;
        }
    }
}
