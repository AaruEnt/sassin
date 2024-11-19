using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_ANDROID
using Valve.VR;
#endif
public class SceneHelper : MonoBehaviour
{
#if !UNITY_ANDROID
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
#endif
}
