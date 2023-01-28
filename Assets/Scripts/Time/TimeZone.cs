using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeZone : MonoBehaviour
{
    [SerializeField, Tooltip("A enum containing 1 of 4 potential time zones")]
    internal timeZone time;

    [SerializeField, Tooltip("The default layer")]
    internal int layer;

    [SerializeField, Tooltip("The time layer")]
    internal int previewLayer;
}

public enum timeZone
{
    timeZone1,
    timeZone2,
    timeZone3,
    timeZone4
}
