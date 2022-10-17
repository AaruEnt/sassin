using System.Collections;
using System.Collections.Generic;

// Guard/Enemy finite states
public enum EnemyState {
    patrol,
    alert,
    search,
    chase
}

// Player states, used for stealth
public enum PlayerStates {
    innocuous,
    suspicious
}

// Civillian states
public enum CivillianState {
    normal,
    nosy
}
