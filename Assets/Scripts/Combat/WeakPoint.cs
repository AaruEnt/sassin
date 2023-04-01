using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeakPoint : MonoBehaviour
{
    [SerializeField, Tooltip("Damage modifier applied when weakpoint hit")]
    internal float damageMod = 1.5f;

    [SerializeField, Tooltip("Minimum damage from this weakpoint")]
    internal float minDamage = 0.1f;

    public UnityEvent OnWeakPointHit;
}
