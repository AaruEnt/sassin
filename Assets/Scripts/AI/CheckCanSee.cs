using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Autohand;

public class CheckCanSee : MonoBehaviour
{
    public UnityEvent OnSee;
    public GameObject eyes;
    [Tag]
    public string tag;
    public LayerMask mask;

    private bool canSee = false;
    private Transform target;

    public bool defaultOnSee = false;
    private GameObject player;
    private AutoHandPlayer aPlayer;

    private void Start()
    {
        if (defaultOnSee)
        {
            player = GameObject.Find("SteamVR Player Container Variant");
            aPlayer = player.GetComponentInChildren<AutoHandPlayer>();
        }
    }

    void Update()
    {
        RaycastHit hitinfo;
        if (canSee)
        {
            bool tmp = Physics.Linecast(eyes.transform.position, target.transform.position, out hitinfo);
            if (tmp && hitinfo.collider.gameObject.CompareTag(tag)) {
                if (defaultOnSee)
                    aPlayer.GetComponent<Stats>().DebugKill();
                else
                    OnSee.Invoke();
            } else
            {
                UnityEngine.Debug.Log(hitinfo.collider.name);
            }
        }
    }


    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag(tag))
        {
            canSee = true;
            target = col.transform;
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.CompareTag(tag))
        {
            canSee = true;
            target = col.transform;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag(tag))
        {
            canSee = false;
        }
    }
}
