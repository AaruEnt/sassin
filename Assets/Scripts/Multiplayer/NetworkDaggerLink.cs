using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkDaggerLink : MonoBehaviour
{
    public GameObject models;

    private GameObject networkDagger;
    private NetworkDaggerv2 dagger;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkDaggerv2.instance)
        {
            dagger = NetworkDaggerv2.instance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (dagger != null)
        {
            dagger.UpdatePosRot(models.transform.position, models.transform.rotation);
        }
        else
        {
            if (NetworkDaggerv2.instance)
            {
                dagger = NetworkDaggerv2.instance;
            }
        }
    }
}
