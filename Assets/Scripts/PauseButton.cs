using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_ANDROID
using Valve.VR;
#endif

public class PauseButton : MonoBehaviour
{
#if !UNITY_ANDROID
    public SteamVR_Action_Boolean pauseButton;
    public GameObject pauseMenu;
    // Start is called before the first frame update

    void Update()
    {
        if (pauseButton != null && pauseButton.lastStateDown)
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
        }
    }
#endif
}
