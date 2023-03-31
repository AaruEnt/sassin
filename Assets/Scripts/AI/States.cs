using System.Collections;
using System.Collections.Generic;

// Guard/Enemy finite states
// patrol - default base state, the guard moves through a series of waypoints
// alert - enhanced version of patrol with faster movespeed and lower suspicion thresholds
// search - when the guard has seen or heard something suspicious and is actively looking for the source
// chase - when the guard is currently chasing the player
// wounded - transitory state for when the guard should be stunned by a hit to the knee
public enum EnemyState {
    patrol,
    alert,
    search,
    chase,
    wounded
}

// Player states, used for stealth
// innocuous - when the player is doing nothing suspicious that would attract attention or invite suspicion
// suspicious - when the player will be regarded as suspicious on sight
public enum PlayerStates {
    innocuous,
    suspicious
}

// Civillian states
// normal - default state. The civillian moves to their destination at a walking pace
// nosy - something has caught their attention and they are investigating. Rushes straight to the cause without looking around
public enum CivillianState {
    normal,
    nosy
}
