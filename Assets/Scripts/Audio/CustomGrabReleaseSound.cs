using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to override the default grab sound when the grabbable this script is attached to is grabbed
public class CustomGrabReleaseSound : MonoBehaviour
{
    [SerializeField, Tooltip("The custom sound on grip. If empty, the default sound will be used.")]
    internal AudioClip customGrabSound;
    [SerializeField, Tooltip("The custom sound on release. If empty, the default sound will be used.")]
    internal AudioClip customReleaseSound;
}
