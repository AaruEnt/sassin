using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    // What state the player is in
    public PlayerStates state = PlayerStates.innocuous;
    public float playerSuspicion = 0f; // Unused for now, mechanic difficulty should be based on it later
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
