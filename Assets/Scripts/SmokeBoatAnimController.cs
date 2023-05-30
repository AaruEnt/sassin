using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class SmokeBoatAnimController : MonoBehaviour
{
    public GameObject toUnparent;
    public GameObject toReparent;
    private Vector3 _startPos;
    
    public void Unparent()
    {
        _startPos = toUnparent.transform.localPosition;
        toUnparent.transform.parent = null;
    }

    public void ReParent()
    {
        if (toReparent != null)
        {
            toUnparent.transform.parent = toReparent.transform;
            toUnparent.SetActive(false);
            toUnparent.transform.localPosition = _startPos;
            toUnparent.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Destroy(toUnparent);
        }
    }
}
