using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviourPunCallbacks
{
    public Transform head;
    public Transform left;
    public Transform right;
    public static GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            player = this.gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine) {
            right.gameObject.SetActive(false);
            left.gameObject.SetActive(false);
            head.gameObject.SetActive(false);
            MapPosition(head, XRNode.Head);
            MapPosition(left, XRNode.LeftHand);
            MapPosition(right, XRNode.RightHand);
        }
    }

    void MapPosition(Transform target, XRNode node)
    {
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);

        target.localPosition = position;
        target.localRotation = rotation;
    }

    internal void UpdatePositionRotation(Transform position, Quaternion rotation)
    {
        transform.position = position.position;
        transform.rotation = rotation;
    }

    #region IPunObservable implementation

    #endregion
}
