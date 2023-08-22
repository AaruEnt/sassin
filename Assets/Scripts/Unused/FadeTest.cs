using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using NaughtyAttributes;

public class FadeTest : MonoBehaviour
{
    [Button]
    public void VRFadeIn() { FadeIn(); }

    public Color customColor;

    public void FadeIn()
    {
        SteamVR_Fade.View(Color.black, 0f);
        SteamVR_Fade.View(Color.clear, 1f);
    }

    public void FadeOut()
    {
        SteamVR_Fade.View(Color.clear, 0f);
        SteamVR_Fade.View(Color.black, 1f);
    }

    public void FadeOutCustom()
    {
        SteamVR_Fade.View(customColor, 1f);
    }

    public void FadeIntermediate()
    {
        SteamVR_Fade.View(Color.black, 1f);
    }
}
