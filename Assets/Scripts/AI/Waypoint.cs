using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("Booleans")]
    [SerializeField, Tooltip("Should the AI turn left at this waypoint")]
    internal bool turnLeft = true;
    [SerializeField, Tooltip("Should the AI turn right at this waypoint")]
    internal bool turnRight = true;
}
