using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventAfterSeconds : MonoBehaviour
{
    public bool startOnAwake = true;
    public float seconds;
    public UnityEvent callAfterSeconds;

    private float timer = 0f;
    private bool isRunning = false;

    // Start is called before the first frame update
    void OnEnable()
    {
        timer = 0f;
        if (startOnAwake)
        {
            isRunning = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime;
            if (timer >= seconds)
            {
                isRunning = false;
                callAfterSeconds.Invoke();
                timer = 0f;
            }
        }
    }
}
