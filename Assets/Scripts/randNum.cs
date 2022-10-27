using System;
using UnityEngine;

public class randNum : MonoBehaviour
{
    // rand num generator for use during runtime
    public System.Random rand;
    void Start()
    {
        rand = new System.Random();
    }
}
