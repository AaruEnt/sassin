using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class CreateObject : MonoBehaviour
{
    public GameObject newObj;

    public bool isPaper = false; // tmp pending a better solution
    [ShowIf("isPaper")]
    public string paperText = "You're A Cunt";


    public void CreateNewObject()
    {
        var tmp = Instantiate(newObj, transform.position, Quaternion.identity, null);
        if (isPaper)
        {
            PaperConstructor p = tmp.GetComponent<PaperConstructor>();
            if (p)
                p.SetText(paperText);
        }
    }
}
