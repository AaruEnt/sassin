using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public class Billboard : MonoBehaviour
    {
        void Update()
        {
            Camera cam = Camera.main;
            if (cam)
            {
                Vector3 targetPos = cam.transform.position;
                targetPos.y = transform.position.y;
                Vector3 forward = transform.position - targetPos;
                if (forward.magnitude > 0)
                {
                    transform.rotation = Quaternion.LookRotation(forward);
                }
            }
        }
    }
}