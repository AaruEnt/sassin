using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    [SerializeField, Tooltip("Damage modifier applied when weakpoint hit")]
    internal float damageMod = 1.5f;
}
