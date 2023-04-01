using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BF_LightSwitch : MonoBehaviour
{
    public GameObject lightScene;
    public Material skyboxScene;

    void OnEnable()
    {
        if(lightScene != null)
        {
            lightScene.SetActive(true);
        }
        if(skyboxScene != null)
        {
            RenderSettings.skybox = skyboxScene;
        }
    }

    void OnDisable()
    {
        if (lightScene != null)
        {
            lightScene.SetActive(false);
        }
    }

}
