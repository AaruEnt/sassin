using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using NaughtyAttributes;

public class FadeTest : MonoBehaviour
{
    [Button]
    public void VRFadeIn() { FadeIn(); }

    void FadeIn()
    {
        SteamVR_Fade.View(Color.black, 0f);
        SteamVR_Fade.View(Color.clear, 1f);
    }
}
