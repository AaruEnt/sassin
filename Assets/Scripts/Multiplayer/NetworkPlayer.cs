using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using System.Diagnostics;
using System.Collections.Specialized;

public class NetworkPlayer : MonoBehaviourPunCallbacks
{
    public Transform head;
    public Transform left;
    public Transform right;
    public CapsuleCollider bodyCol;
    public static GameObject player;
    public Vector3 leftRotOffset;
    public Vector3 rightRotOffset;

    internal GameObject leftHand;
    internal GameObject rightHand;
    internal GameObject headCamera;
    internal CapsuleCollider body;

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
            bodyCol.gameObject.SetActive(false);
            MapPosition(head, XRNode.Head, Quaternion.identity.eulerAngles);
            MapPosition(left, leftHand);
            MapPosition(right, rightHand);
            if (body)
                UpdateCollider(bodyCol, body);
        }
    }

    void MapPosition(Transform target, XRNode node, Vector3 rotOffset)
    {
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);

        target.localPosition = position;
        target.localRotation = rotation * Quaternion.Euler(rotOffset);
    }

    void MapPosition(Transform target, GameObject node)
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

    void UpdateCollider(CapsuleCollider copyTo, CapsuleCollider copyFrom)
    {
        float scale = copyFrom.height / 1.6f; // 1.6 is the base height of the collider pre scaling.
        Vector3 tmp = new Vector3(1f, scale, 1f);
        copyTo.transform.localScale = tmp;
        //copyTo.radius = copyFrom.radius;
        //copyTo.center = copyFrom.center;
        //var tmp2 = copyFrom.transform.localPosition;
        //tmp2.y = tmp2.y - (((scale * 0.8f) - (scale * copyFrom.center.y)));
        copyTo.transform.position = copyFrom.transform.position;
        var tmp2 = copyTo.transform.localPosition;
        tmp2.y = (((1f - scale) / 0.25f) * 0.05f) + -0.2f;
        copyTo.transform.localPosition = tmp2;

    }

    #region IPunObservable implementation

    #endregion
}
