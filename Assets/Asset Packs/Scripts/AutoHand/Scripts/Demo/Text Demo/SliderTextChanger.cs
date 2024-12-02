using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand.Demo{
    public class SliderTextChanger : MonoBehaviour{
        public TMPro.TextMeshPro text;
        public PhysicsGadgetConfigurableLimitReader sliderReader;

        float lastValue = 0;

        void Update(){
            var value = sliderReader.GetValue();
            if(value != lastValue) {
                lastValue = value;
                text.text = Math.Round(value, 2).ToString();
            }
        }
    }
}
