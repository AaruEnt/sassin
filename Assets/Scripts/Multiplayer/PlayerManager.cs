using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public static GameObject LocalPlayerInstance;

    public float health;
    public float maxHealth;
    internal Stats s;
    // Start is called before the first frame update
    void Awake()
    {
        if (photonView.IsMine)
        {
            PlayerManager.LocalPlayerInstance = this.gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine && Stats.LocalStatsInstance != null && s == null)
            s = Stats.LocalStatsInstance;
        if (s)
        {
            maxHealth = s.maxHealth;
            health = s.health;
        }
    }

    internal void UpdateHealth(float damage)
    {
        s.OnDamageReceived(damage);
    }
}
