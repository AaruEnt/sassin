using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Diagnostics;
using JointVR;

public class NetworkDagger : MonoBehaviourPunCallbacks
{
    public static GameObject dagger;

    public GameObject networkModels;

    internal GameObject model;

    [SerializeField]
    internal float? damage;

    internal Rigidbody localRigidbody;
    internal float realDamage = 1f;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            dagger = this.gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine && StabManager.LocalDaggerInstance != null && model == null)
        {
            model = StabManager.LocalDaggerInstance.models;
            model.gameObject.SetActive(false);
            damage = StabManager.LocalDaggerInstance.gameObject.GetComponent<Weapon>()?.damage;
            if (damage == null)
            {
                damage = 1f;
            }
        }
        if (photonView.IsMine && localRigidbody == null && model != null)
        {
            localRigidbody = StabManager.LocalDaggerInstance.gameObject.GetComponent<Rigidbody>();
        }
        if (photonView.IsMine && localRigidbody)
        {
            float m = localRigidbody.velocity.magnitude;
            m = m > 2 ? 2 : m;
            realDamage = m * (float)damage;
        }
        if (photonView.IsMine)
        {
            foreach (Transform t in transform)
            {
                if (t != transform)
                    t.gameObject.SetActive(false);
            }
        }
        //UnityEngine.Debug.Log(model.transform.position);
        UpdatePositionRotation(model.transform, model.transform.rotation);
    }

    internal void UpdatePositionRotation(Transform pos, Quaternion rot)
    {
        networkModels.transform.position = pos.position;
        networkModels.transform.rotation = rot;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            var rb = col.body as Rigidbody;
            PlayerManager p = rb.gameObject.GetComponent<PlayerManager>();
            if (p == null || p.gameObject == PlayerManager.LocalPlayerInstance)
            {
                UnityEngine.Debug.Log("Error in finding playermanager");
                return;
            } else
            {
                p.UpdateHealth(realDamage);
            }
        }
    }
}
