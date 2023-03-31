using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    // disables the variable unused warning
#pragma warning disable 0414
    [SerializeField, Tooltip("The current state of the player")]
    internal PlayerStates state = PlayerStates.innocuous;

    [SerializeField, Tooltip("Unused for now, mechanic difficulty should be based on it later")]
    private float playerSuspicion = 0f;
#pragma warning restore 0414
}
