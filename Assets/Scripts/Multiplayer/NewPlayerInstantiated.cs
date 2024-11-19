using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Autohand;
#if !UNITY_ANDROID
using Valve.VR;
#endif

public class NewPlayerInstantiated : MonoBehaviourPunCallbacks
{
    public Camera thisCam;
    public Autohand.Hand leftHand;
    public Autohand.Hand rightHand;
#if !UNITY_ANDROID
    public SteamVR_Behaviour_Pose left;
    public SteamVR_Behaviour_Pose right;
#endif
    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.IsMine)
        {
            thisCam.enabled = false;
            thisCam.gameObject.GetComponent<AudioListener>().enabled = false;
            leftHand.enabled = false;
            rightHand.enabled = false;

#if !UNITY_ANDROID
            left.enabled = false;
            right.enabled = false;
#endif
        }
    }
}
