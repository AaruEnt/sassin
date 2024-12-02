using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Autohand {
    [CustomEditor(typeof(EditorHand))]
    public class EditorHandEditor : Editor {

        bool[] fingerStates = new bool[] { };

        private void OnEnable() {
            var editorHand = (target as EditorHand);
            var hand = editorHand.hand;

            if(fingerStates.Length == 0)
                fingerStates = new bool[hand.fingers.Length];

            for(int i = 0; i < fingerStates.Length; i++)
                fingerStates[i] = true;

            hand.SetLayerRecursive(hand.transform, LayerMask.NameToLayer(hand.left ? Hand.leftHandLayerName : Hand.rightHandLayerName));

            if(editorHand.handPoseDataContainer != null)
                EditorHandTool.ShowWindow(hand, editorHand.handPoseDataContainer);
        }
    }
}