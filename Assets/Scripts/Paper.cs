using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JointVR;
using NaughtyAttributes;

public class Paper : MonoBehaviour
{
    public StabManager stabbedBy;
    [Layer]
    public int defaultLayer;
    [Layer]
    public int tearingLayer;
    private float timer = 0f;
    public AudioSource tearSound;

    void Update()
    {
        if (timer >= 0f)
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                gameObject.layer = defaultLayer;
            }
        }
    }
    
    public void TearPaper()
    {
        if (stabbedBy)
        {
            stabbedBy.UnstabTarget(GetComponent<Collider>());
            gameObject.layer = tearingLayer;
            timer = 0.5f;
            tearSound.Play();
            stabbedBy = null;
        }
    }

    public void SetStabbedBy(StabManager s, GameObject t)
    {
        stabbedBy = s;
    }

    public void UnSetStabbedBy(StabManager s, GameObject t)
    {
        stabbedBy = null;
    }
}
