using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {
    public static class HandExtensions {

        


        public static void HandIgnoreCollider(this Hand hand, Collider collider, bool ignore) {
            for(int i = 0; i < hand.handColliders.Count; i++)
                Physics.IgnoreCollision(hand.handColliders[i], collider, ignore);
        }


        public static void SetLayerRecursive(this Hand hand, Transform obj, int newLayer) {
            obj.gameObject.layer = newLayer;
            for(int i = 0; i < obj.childCount; i++) {
                hand.SetLayerRecursive(obj.GetChild(i), newLayer);
            }
        }


    }
}