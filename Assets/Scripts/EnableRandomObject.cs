using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableRandomObject : MonoBehaviour
{
    public List<GameObject> pickFrom = new List<GameObject> ();
    public float probability = 100f;

    // Start is called before the first frame update
    void Start()
    {
        if (Randomizer.Prob(probability))
        {
            var tmp = Randomizer.PickRandomObject(pickFrom);

            if (tmp)
            {
                tmp.SetActive(true);
            }
        }
    }
}
