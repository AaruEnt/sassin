using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;
using Photon.Pun;

public class TriggerEvent : MonoBehaviour
{
    public GameObject obj;
    [Tag]
    public string tag;
    public UnityEvent<Collider> t;
    public UnityEvent<Collider> te;
    public bool requireMasterConnection = false;

    private void Start()
    {
        if (requireMasterConnection)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Destroy(this); return;
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if ((obj && obj == col.gameObject) || (tag != "" && col.gameObject.CompareTag(tag)))
            t.Invoke(col);
    }

    void OnTriggerExit(Collider col)
    {
        if ((obj && obj == col.gameObject) || (tag != "" && col.gameObject.CompareTag(tag)))
            te.Invoke(col);
    }
}
