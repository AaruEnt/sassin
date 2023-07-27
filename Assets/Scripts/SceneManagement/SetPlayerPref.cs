using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPlayerPref : MonoBehaviour
{
    public void SetPlayerPrefFlag(string flagName)
    {
        PlayerPrefs.SetInt(flagName, 1);
    }

    public void UnsetPlayerPrefFlag(string flagName)
    {
        PlayerPrefs.SetInt(flagName, 0);
    }
}
