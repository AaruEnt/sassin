using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableAfterSeconds : MonoBehaviour
{
    public GameObject toEnable;
    public GameObject newObject;

    private float timer = 0f;
    private bool isRunning = false;
    private float currDelay = 0f;
    private bool create = false;

    void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime;
            if (timer >= currDelay)
            {
                if (toEnable && !create)
                {
                    toEnable.SetActive(true);
                    isRunning = false;
                    timer = 0f;
                }
                else if (newObject && create)
                {
                    UnityEngine.Debug.Log(transform.rotation);
                    var tmp = Instantiate(newObject, transform.position, transform.rotation, null);
                    tmp.transform.rotation = transform.rotation;
                    isRunning = false;
                    timer = 0f;
                    create = false;
                }
            }
        }
    }

    public void CallEnableAfterSeconds(float delay)
    {
        currDelay = delay;
        isRunning = true;
    }

    public void CreateAfterSeconds(float delay)
    {
        currDelay = delay;
        isRunning = true;
        create = true;
    }
}
