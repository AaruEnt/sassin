using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimePreviewController : MonoBehaviour
{
    public Camera previewCamera;
    private LayerMask startMask;
    public int pastTimeLayer;
    public int presentTimeLayer;
    public int futureTimeLayer;
    public WristDial dial;
    public LayerMask mask;
    // Start is called before the first frame update
    void Start()
    {
        startMask = previewCamera.cullingMask;
    }

    // Update is called once per frame
    void Update()
    {
        int pos = dial.dialPosToTimezone;
        mask = startMask;

        //Debug.Log(pos);

        switch (pos) {
            case 0:
                mask = startMask | (1 << pastTimeLayer);
                break;
            case 1:
                mask = startMask | (1 << presentTimeLayer);
                break;
            case 2:
                mask = startMask | (1 << futureTimeLayer);
                break;
            default:
                break;
        }
        previewCamera.cullingMask = mask;
    }
}
