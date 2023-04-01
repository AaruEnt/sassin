using System;
using UnityEngine;

public class randNum : MonoBehaviour
{
    [SerializeField, Tooltip("rand num generator for use during runtime")]
    internal System.Random rand = new System.Random();
}
