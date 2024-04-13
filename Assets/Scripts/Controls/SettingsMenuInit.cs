using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuInit : MonoBehaviour
{
    public AutoHandPlayer player;
    public Slider masterVolume;
    public Slider sfxVolume;
    public Slider bgmVolume;

    public Slider snap;
    public Slider smooth;

    public Toggle snapSmooth;

    public GameObject snapParent;
    public GameObject smoothParent;


    private string masterVolumeName = "MasterVolume";
    private string bgmVolumeName = "BGMMain";
    private string sfxVolumeName = "SFXMain";


    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey(masterVolumeName))
        {
            masterVolume.value = PlayerPrefs.GetFloat(masterVolumeName);
        }
        if (PlayerPrefs.HasKey(bgmVolumeName))
        {
            bgmVolume.value = PlayerPrefs.GetFloat(bgmVolumeName);
        }
        if (PlayerPrefs.HasKey(sfxVolumeName))
        {
            sfxVolume.value = PlayerPrefs.GetFloat(sfxVolumeName);
        }
        if (PlayerPrefs.HasKey("Snap"))
        {
            bool s = PlayerPrefs.GetInt("Snap") == 1 ? true : false;
            player.snapTurning = s;
            if (s)
            {
                snapParent.SetActive(true);
                smoothParent.SetActive(false);
            }
            else
            {
                snapParent.SetActive(false);
                smoothParent.SetActive(true);
            }
        }
        if (PlayerPrefs.HasKey("SnapAngle"))
        {
            player.snapTurnAngle = PlayerPrefs.GetInt("SnapAngle");
        }
        if (PlayerPrefs.HasKey("SmoothSpeed"))
        {
            player.smoothTurnSpeed = PlayerPrefs.GetFloat("SmoothSpeed");
        }
    }

    public void SnapParentToggle(bool toggle)
    {
        if (toggle)
        {
            snapParent.SetActive(true);
            smoothParent.SetActive(false);
        }
        else
        {
            snapParent.SetActive(false);
            smoothParent.SetActive(true);
        }
    }
}
