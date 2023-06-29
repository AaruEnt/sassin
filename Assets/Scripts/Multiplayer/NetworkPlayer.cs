using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using System.Diagnostics;

public class NetworkPlayer : MonoBehaviourPunCallbacks
{
    public Transform head;
    public Transform left;
    public Transform right;
    public static GameObject player;
    public Vector3 leftRotOffset;
    public Vector3 rightRotOffset;

    internal GameObject leftHand;
    internal GameObject rightHand;
    internal GameObject headCamera;

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
            MapPosition(head, XRNode.Head, Quaternion.identity.eulerAngles);
            MapPosition(left, leftHand, leftRotOffset);
            MapPosition(right, rightHand, rightRotOffset);
        }
    }

    void MapPosition(Transform target, XRNode node, Vector3 rotOffset)
    {
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);

        target.localPosition = position;
        target.rotation = rotation * Quaternion.Euler(rotOffset);
    }

    void MapPosition(Transform target, GameObject node, Vector3 rotOffset = default(Vector3))
    {
        UnityEngine.Debug.Log("In override");
        target.localPosition = node.transform.localPosition;
        target.rotation = node.transform.rotation;
    }

    internal void UpdatePositionRotation(Transform position, Quaternion rotation)
    {
        transform.position = position.position;
        transform.rotation = rotation;
    }

    #region IPunObservable implementation

    #endregion
}
