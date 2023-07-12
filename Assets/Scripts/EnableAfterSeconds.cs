using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableAfterSeconds : MonoBehaviour
{
    public bool enableOnAwake;
    public bool destroyOnFinish = false;
    public float defaultSeconds = 1f;
    public GameObject toEnable;
    public GameObject newObject;


    private float timer = 0f;
    private bool isRunning = false;
    private float currDelay = 0f;
    private bool create = false;
    public Transform spawnPoint;

    void Awake()
    {
        if (enableOnAwake)
        {
            isRunning = true;
            currDelay = defaultSeconds;
            timer = 0f;
        }
    }


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
                    Transform spawn = transform;
                    if (spawnPoint)
                        spawn = spawnPoint;
                    var tmp = Instantiate(newObject, spawn.position, transform.rotation, null);
                    tmp.transform.rotation = transform.rotation;
                    isRunning = false;
                    timer = 0f;
                    create = false;
                }

                if (destroyOnFinish)
                {
                    Destroy(this);
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

    public void CancelCreation()
    {
        isRunning = false;
    }
}
