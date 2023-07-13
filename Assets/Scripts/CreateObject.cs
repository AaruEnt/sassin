using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateObject : MonoBehaviour
{
    public GameObject newObj;

    public void CreateNewObject()
    {
        Instantiate(newObj, transform.position, Quaternion.identity, null);
    }
}
