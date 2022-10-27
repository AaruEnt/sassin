using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObjectWithOffset : MonoBehaviour
{
    public GameObject followTarget;
    public bool followOn = true;

    internal Vector3 offset;
    internal Vector3 _startPos;

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
