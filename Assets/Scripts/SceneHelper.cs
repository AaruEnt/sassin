using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SceneHelper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SteamVR_Fade.View(Color.black, 0f);
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(1f);
        SteamVR_Fade.View(Color.clear, 1.5f);
    }
}
