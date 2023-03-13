using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeakPoint : MonoBehaviour
{
    [SerializeField, Tooltip("Damage modifier applied when weakpoint hit")]
    internal float damageMod = 1.5f;

    public UnityEvent OnWeakPointHit;
}
