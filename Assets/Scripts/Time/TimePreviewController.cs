using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimePreviewController : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("The camera displaying the timezone preview")]
    internal Camera previewCamera;

    [SerializeField, Tooltip("The dial controlling the timezone")]
    internal WristDial dial;

    [SerializeField, Tooltip("The layermask used for previews")]
    internal LayerMask mask;


    [Header("Variables")]
    [SerializeField, Tooltip("The layer for 'past' time")]
    internal int pastTimeLayer;

    [SerializeField, Tooltip("the layer for 'present' time")]
    internal int presentTimeLayer;

    [SerializeField, Tooltip("The layer for 'future' time")]
    internal int futureTimeLayer;


    // Unserialized vars

    // The starting layermask unmodified by timezones
    private LayerMask startMask;


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
