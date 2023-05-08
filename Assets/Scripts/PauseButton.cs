using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PauseButton : MonoBehaviour
{
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
}
