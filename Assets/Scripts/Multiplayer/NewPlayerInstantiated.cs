using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Autohand;

public class NewPlayerInstantiated : MonoBehaviourPunCallbacks
{
    public Camera thisCam;
    public Autohand.Hand leftHand;
    public Autohand.Hand rightHand;
    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.IsMine)
        {
            thisCam.enabled = false;
            thisCam.gameObject.GetComponent<AudioListener>().enabled = false;
            leftHand.enabled = false;
            rightHand.enabled = false;
        }
    }
}
