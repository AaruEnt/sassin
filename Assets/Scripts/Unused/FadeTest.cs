using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_ANDROID
using Valve.VR;
#endif
using NaughtyAttributes;

public class FadeTest : MonoBehaviour
{
    [Button]
    public void VRFadeIn() { FadeIn(); }

    public Color customColor;

    private void Start()
    {
        FadeIn();
    }

    public void FadeIn()
    {
#if !UNITY_ANDROID
        SteamVR_Fade.View(Color.black, 0f);
        SteamVR_Fade.View(Color.clear, 1f);
#endif
    }

    public void FadeOut()
    {
#if !UNITY_ANDROID
        SteamVR_Fade.View(Color.clear, 0f);
        SteamVR_Fade.View(Color.black, 1f);
#endif
    }

    public void FadeOutCustom()
    {
#if !UNITY_ANDROID
        SteamVR_Fade.View(customColor, 1f);
#endif
    }
}
