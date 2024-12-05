using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand.Demo{
    public class XRHandPointGrabLink : MonoBehaviour{
        public HandDistanceGrabber pointGrab;
        public XRHandControllerLink link;

        [Header("Input")]
        public CommonButton pointInput;
        public CommonButton selectInput;

        bool pointing;
        bool selecting;

        private void Start() {
            if(link == null) {
                link = GetComponentInParent<XRHandControllerLink>();
                if(link == null)
                    Debug.LogError("No XRHandControllerLink connected - input will not work", this);
            }

            if(pointGrab == null) {
                pointGrab = GetComponentInParent<HandDistanceGrabber>();
                if(pointGrab == null)
                    Debug.LogError("No HandDistanceGrabber connected - input will not work", this);
            }
        }

        void Update(){
            if(link == null || pointGrab == null)
                return;

            if (link.ButtonPressed(pointInput) && !pointing) {
                pointing = true;
                pointGrab.StartPointing();
            }

            if (!link.ButtonPressed(pointInput) && pointing){
                pointing = false;
                pointGrab.StopPointing();
            }

            
            if (link.ButtonPressed(selectInput) && !selecting) {
                selecting = true;
                pointGrab.SelectTarget();
            }
            
            if (!link.ButtonPressed(selectInput) && selecting){
                selecting = false;
                pointGrab.CancelSelect();
            }
        }
    }
}
