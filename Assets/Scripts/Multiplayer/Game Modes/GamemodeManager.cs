using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamemodeManager : MonoBehaviourPunCallbacks
{
    private string MODE_PROP_KEY = "mod";
    public SpawnManager defaultGameMode;
    public ScoutMode scoutMode;


    // Start is called before the first frame update
    void Awake()
    {
        SetGamemode();
    }

    public void SetGamemode()
    {
        switch (PhotonNetwork.CurrentRoom?.CustomProperties[MODE_PROP_KEY])
        {
            case "":
                if (defaultGameMode)
                    defaultGameMode.enabled = true;
                if (scoutMode)
                    scoutMode.enabled = false;
                break;

            case "None":
                if (defaultGameMode)
                    defaultGameMode.enabled = true;
                if (scoutMode)
                    scoutMode.enabled = false;
                break;

            case "Arena":
                if (defaultGameMode)
                    defaultGameMode.enabled = true;
                if (scoutMode)
                    scoutMode.enabled = false;
                break;

            case "Scout":
                if (defaultGameMode)
                    defaultGameMode.enabled = false;
                if (scoutMode)
                    scoutMode.enabled = true;
                break;

            case "Gather/Invasion":
                if (defaultGameMode)
                    defaultGameMode.enabled = false;
                if (scoutMode)
                    scoutMode.enabled = false;
                break;

            default:
                if (defaultGameMode)
                    defaultGameMode.enabled = true;
                if (scoutMode)
                    scoutMode.enabled = false;
                break;
        }
    }
}
