using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaintainPositionRotation : MonoBehaviour
{
    [SerializeField, Tooltip("The object to be treated as the parent of this object.")]
    private Transform Parent;//Uppercase to avoid confusion with transform.parent
    private Vector3 pos, fw, up;

    void Start()
    {
        if (!Parent)
            Debug.LogError("No parent set for object " + gameObject.name);

        pos = Parent.transform.InverseTransformPoint(transform.position);
        fw = Parent.transform.InverseTransformDirection(transform.forward);
        up = Parent.transform.InverseTransformDirection(transform.up);
    }
    void Update()
    {
        var newpos = Parent.transform.TransformPoint(pos);
        var newfw = Parent.transform.TransformDirection(fw);
        var newup = Parent.transform.TransformDirection(up);
        var newrot = Quaternion.LookRotation(newfw, newup);
        transform.position = newpos;
        transform.rotation = newrot;
    }
}
