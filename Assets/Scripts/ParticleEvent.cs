using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;
using System.Diagnostics;
using System.ComponentModel;

public class ParticleEvent : MonoBehaviour
{
    public UnityEvent t;

    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void OnParticleTrigger()
    {
        UnityEngine.Debug.Log(string.Format("Particle trigger called by {0}", transform.parent.name));
        t.Invoke();
    }
}
