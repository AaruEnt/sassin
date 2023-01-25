#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand.Demo;
using Valve.VR;

namespace Autohand{
    //THIS SCRIPT IS A TEMPORARY DEMO SCRIPT
    //BETTER MOVEMENT OPTIONS COMING SOON
    public class SteamVRMover : MonoBehaviour{
        [Header("TEMP DEMO SCRIPT - Advanced script coming soon")]
        [Header("Controllers")]
        public SteamVRHandControllerLink moverController;
        public SteamVRHandControllerLink turningController;
        public SteamVR_Action_Vector2 moverAxis;
        public SteamVR_Action_Boolean turnRightButton;
        public SteamVR_Action_Boolean turnLeftButton;

        [Header("Body")]
        public GameObject cam;
        private CharacterController controller;
        private CapsuleCollider collisionCapsule;

        [Header("Settings")]
        public bool snapTurning;
        public float turnAngle;
        public float speed = 5;
        public float gravity = 1;

        private float currentGravity = 0;

        private bool axisReset = true;

        Vector3 moveAxis;
        float turningAxis;

        //Driver
        public void LateUpdate(){
            if(snapTurning){
                bool rightTurn = turningController.ButtonPressed(turnRightButton);
                bool leftTurn = turningController.ButtonPressed(turnLeftButton);
                turningAxis = 0;
                if(rightTurn)
                    turningAxis = 1;
                else if(leftTurn)
                    turningAxis = -1;
            }
            else turningAxis = moverController.GetAxis2D(moverAxis).x;
            
            Turning();


            moveAxis = moverController.GetAxis2D(moverAxis);
            Move(moveAxis.x, moveAxis.z, moveAxis.y);
        }


        private void Awake(){
            gameObject.layer = LayerMask.NameToLayer("HandPlayer");
            controller = GetComponent<CharacterController>();
        }


        public void Move(float x, float y, float z){

            Vector3 direction = new Vector3(x, y, z);
            Vector3 headRotation = new Vector3(0, cam.transform.eulerAngles.y, 0);

            direction = Quaternion.Euler(headRotation) * direction;

            currentGravity = Physics.gravity.y * gravity;

            if (controller.isGrounded)
                currentGravity = 0;

            controller.Move(new Vector3(direction.x * speed, direction.y * speed + currentGravity, direction.z * speed) * Time.deltaTime);
        }


        void Turning(){
            //Snap turning
            if (snapTurning){
                if (turningAxis > 0.7f && axisReset){
                    transform.rotation *= Quaternion.Euler(0, turnAngle, 0);
                    axisReset = false;
                }
                else if (turningAxis < -0.7f && axisReset){
                    transform.rotation *= Quaternion.Euler(0, -turnAngle, 0);
                    axisReset = false;
                }

                if (Mathf.Abs(turningAxis) < 0.4f)
                    axisReset = true;
            }

            //Smooth turning
            else{
                transform.rotation *= Quaternion.Euler(0, Time.deltaTime * turnAngle * turningAxis, 0);
            }
        }
    }
}
#endif
