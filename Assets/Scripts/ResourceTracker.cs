using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using NaughtyAttributes;
using Com.Aaru.Sassin;

public class ResourceTracker : MonoBehaviourPun
{
    public int sandCrystal = 0;
    public int oceanCrystal = 0;
    public int wood = 0;
    public int stone = 0;
    public int food = 0;
    public int leather = 0;

    public SpawnManager? sm;


    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        if (sm)
        {
            if (sm.scores.ContainsKey(PlayerManager.LocalPlayerInstance.GetPhotonView().Owner.NickName))
            {
                stone = sm.scores[PlayerManager.LocalPlayerInstance.GetPhotonView().Owner.NickName];
            }
        }
    }
}
