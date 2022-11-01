using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObjectWithOffset : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("The target to be followed")]
    internal GameObject followTarget;


    [Header("Variables")]
    [SerializeField, Tooltip("Offset between this object and the target to be maintained")]
    internal Vector3 offset;

    [SerializeField, Tooltip("The starting position of the object")]
    internal Vector3 _startPos;


    [Header("Boolean Toggles")]
    [SerializeField, Tooltip("Is the follow currently activated")]
    internal bool followOn = true;

    

    // Start is called before the first frame update
    void Start()
    {
        if (!followTarget)
            Destroy(this);
        offset = transform.position - followTarget.transform.position;
        _startPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (followOn)
            transform.localPosition = _startPos + offset;
        else
            transform.localPosition = _startPos;
    }
}
