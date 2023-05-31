using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class FadeToDestroy : MonoBehaviour
{
    [Tooltip("If left null, will destroy self")]
    public GameObject toDestroy;

    public float delay = 0f;
    public float fadeDelay = 0f;

    public bool autoDestroyOnCreate = false;

    private float timer = 0f;
    private MeshRenderer mr;

    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        var mat = new Material(mr.material);
        mr.material = mat;

        if (autoDestroyOnCreate)
        {
            if (toDestroy == null)
            {
                Destroy(this.gameObject, delay);
            }
            else
            {
                Destroy(toDestroy, delay);
            }
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        float blend = (timer - fadeDelay) / (delay - fadeDelay);
        if (timer >= fadeDelay)
        {
            var color = mr.material.color;
            color.a = 1 - blend;
            mr.material.SetColor("_BaseColor", color);
        }
    }
}
