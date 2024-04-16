using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Autohand;

public class Settings : MonoBehaviour
{
    public AudioMixer mixer;
    public AutoHandPlayer player;

    private string masterVolumeName = "MasterVolume";
    private string bgmVolumeName = "BGMMain";
    private string sfxVolumeName = "SFXMain";

    public Transform cameraForward;
    public Transform controllerForward;
    public Transform offControllerForward;

    private void Start()
    {
        if (PlayerPrefs.HasKey(masterVolumeName))
        {
            mixer.SetFloat(masterVolumeName, GetConvertedVolume(PlayerPrefs.GetFloat(masterVolumeName)));
        }
        if (PlayerPrefs.HasKey(bgmVolumeName))
        {
            mixer.SetFloat(bgmVolumeName, GetConvertedVolume(PlayerPrefs.GetFloat(bgmVolumeName)));
        }
        if (PlayerPrefs.HasKey(sfxVolumeName))
        {
            mixer.SetFloat(sfxVolumeName, GetConvertedVolume(PlayerPrefs.GetFloat(sfxVolumeName)));
        }
        if (PlayerPrefs.HasKey("Snap"))
        {
            bool s = PlayerPrefs.GetInt("Snap") == 1 ? true : false;
            player.snapTurning = s;
        }
        if (PlayerPrefs.HasKey("SnapAngle"))
        {
            player.snapTurnAngle = PlayerPrefs.GetInt("SnapAngle");
        }
        if (PlayerPrefs.HasKey("SmoothSpeed"))
        {
            player.smoothTurnSpeed = PlayerPrefs.GetFloat("SmoothSpeed") * 100f;
        }
        if (PlayerPrefs.HasKey("ForwardFollow"))
        {
            SetForwardTransform(PlayerPrefs.GetInt("ForwardFollow") == 1);
        }
        if (PlayerPrefs.HasKey("HandFollowUsed"))
        {
            SwapForwardHand(PlayerPrefs.GetInt("HandFollowUsed") == 1);
        }
    }

    public void SetMasterVolume(float volume)
    {
        float tmp = GetConvertedVolume(volume);
        mixer.SetFloat(masterVolumeName, tmp);
        PlayerPrefs.SetFloat(masterVolumeName, volume);
    }

    public void SetBGMVolume(float volume)
    {
        float tmp = GetConvertedVolume(volume);
        mixer.SetFloat(bgmVolumeName, tmp);
        PlayerPrefs.SetFloat(bgmVolumeName, volume);
    }

    public void SetSFXVolume(float volume)
    {
        float tmp = GetConvertedVolume(volume);
        mixer.SetFloat(sfxVolumeName, tmp);
        PlayerPrefs.SetFloat(sfxVolumeName, volume);
    }

    public float GetConvertedVolume(float volume)
    {
        float p = volume * 80f;
        float p2 = -(80f - p);
        // range is -80 to 0, but input is 0-1
        return p2;
    }

    public void SetSnapSmooth(bool snap)
    {
        player.snapTurning = snap;
        PlayerPrefs.SetInt("Snap", snap == true ? 1 : 0);
    }

    public void SetSnapAngle(float angle)
    {
        player.snapTurnAngle = (int)angle;
        PlayerPrefs.SetInt("SnapAngle", (int)angle);
    }

    public void SetSmoothSpeed(float speed)
    {
        player.smoothTurnSpeed = speed * 100f;
        PlayerPrefs.SetFloat("SmoothSpeed", speed);
    }

    public void SetForwardTransform(bool forward)
    {
        // true is camera (default) false is right controller
        player.forwardFollow = forward == true ? cameraForward : controllerForward;
        PlayerPrefs.SetInt("ForwardFollow", forward == true ? 1 : 0);
        if (PlayerPrefs.HasKey("HandFollowUsed"))
            SwapForwardHand(PlayerPrefs.GetInt("HandFollowUsed") == 1);
    }

    public void SwapForwardHand(bool swap)
    {
        // false is right, true is left
        if (PlayerPrefs.GetInt("ForwardFollow") == 0)
            player.forwardFollow = swap == true ? offControllerForward : controllerForward;
        PlayerPrefs.SetInt("HandFollowUsed", swap == true ? 1 : 0);
    }
}
