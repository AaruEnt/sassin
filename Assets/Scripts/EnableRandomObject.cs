using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableRandomObject : MonoBehaviour
{
    public List<GameObject> pickFrom = new List<GameObject> ();

    // Start is called before the first frame update
    void Start()
    {
        var tmp = Randomizer.PickRandomObject(pickFrom);

        if (tmp) {
            tmp.SetActive(true);
        }
    }
}
