using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObjectWithOffset : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("The target to be followed")]
    public Transform Parent;


    [Header("Variables")]
    [SerializeField, Tooltip("Offset between this object and the target to be maintained")]
    internal Vector3 pos, fw, up;

    [SerializeField, Tooltip("The starting position of the object")]
    internal Vector3 _startPos;


    [Header("Boolean Toggles")]
    [Tooltip("Is the follow currently activated")]
    public bool followOn = true;
    [SerializeField, Tooltip("Is the rotation follow currently activated")]
    internal bool followRotOn = true;
    [SerializeField, Tooltip("Ignore offsets - set to exact position/rotation")]
    internal bool ignoreOffsets = true;
    public bool startDisabled = false;



    // Start is called before the first frame update
    void Start()
    {
        if (!Parent)
            Destroy(this);
        pos = Parent.transform.InverseTransformPoint(transform.position);
        fw = Parent.transform.InverseTransformDirection(transform.forward);
        up = Parent.transform.InverseTransformDirection(transform.up);
        if (startDisabled)
            this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        var newpos = Parent.transform.TransformPoint(pos);
        var newfw = Parent.transform.TransformDirection(fw);
        var newup = Parent.transform.TransformDirection(up);
        var newrot = Quaternion.LookRotation(newfw, newup);
        if (!ignoreOffsets)
        {
            if (followOn)
                transform.position = newpos;
            else
                transform.localPosition = _startPos;
            if (followRotOn)
            {
                transform.rotation = newrot;
            }
        }
        else
        {
            if (followOn)
                transform.position = Parent.transform.position;
            if (followRotOn)
                transform.rotation = Parent.transform.rotation;
        }
    }
}
