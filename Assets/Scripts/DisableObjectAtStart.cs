using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableObjectAtStart : MonoBehaviour
{
    public GameObject[] objects;
    // Start is called before the first frame update
    void Start()
    {
        foreach (var obj in objects)
        {
            obj.SetActive(false);
        }
    }
}
