using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Autohand {
    public class HandControllerLink : MonoBehaviourPunCallbacks {
        public static HandControllerLink handLeft, handRight;

        public Hand hand;

        public virtual void TryHapticImpulse(float duration, float amp, float freq = 10f) {

        }
    }
}