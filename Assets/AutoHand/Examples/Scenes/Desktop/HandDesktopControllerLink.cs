using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand {

    public class HandDesktopControllerLink : MonoBehaviour {
        [Header("Input References")]
        public Hand handRight;
        public Transform handRightFollow;
        public BoxCollider handRightBound;
        [Space]
        public Hand handLeft;
        public Transform handLeftFollow;
        public BoxCollider handLeftBound;
        [Space]
        public Camera headCamera;
        public Transform trackingContainer;
        [Space]
        public AutoHandPlayer player;

        [Header("Input Settings")]
        public float mouseHandMovementSpeed = 0.5f;
        public float mouseHandRotationSpeed = 10;
        public float mouseLookRotationSpeed = 15;
        public float handReturnSpeed = 1f;

        public KeyCode grabKey = KeyCode.Space;
        public KeyCode rightHandSelection = KeyCode.Mouse1;
        public KeyCode leftHandSelection = KeyCode.Mouse0;
        public KeyCode rotationKeycode = KeyCode.Mouse2;

        bool headRoationActive = false;
        bool rightHandSelected = false;
        bool leftHandSelected = false;
        bool rotationSelected = false;

        Vector3 handRightStartPosition;
        Vector3 handLeftStartPosition;

        private void Awake() {
            if(handRight != null) {
                handRight.follow = handRightFollow;
                handRightStartPosition = handRightFollow.localPosition;
            }

            if(handLeft != null) {
                handLeft.follow = handLeftFollow;
                handLeftStartPosition = handLeftFollow.localPosition;
            }
        }

        void Update() {
            CheckSelectionInput();
            CheckMovementInput();
        }

        void OnGUI() {
            ShowInputStates();
        }

        void ShowInputStates() {
            int x = Screen.width - 300;
            int y = 10;
            int width = 280;
            int height = 44;

            GUIStyle redStyle = new GUIStyle(GUI.skin.label);
            redStyle.normal.textColor = Color.red;
            redStyle.fontSize = 16;

            GUIStyle greenStyle = new GUIStyle(GUI.skin.label);
            greenStyle.normal.textColor = Color.green;
            greenStyle.fontSize = 16;

            GUI.Label(new Rect(x, y, width, height), $"Head Rotation Active (Default Selected)", headRoationActive ? greenStyle : redStyle);
            y += height + 5;
            GUI.Label(new Rect(x, y, width, height), $"Right Hand Selected (Press {rightHandSelection})", rightHandSelected ? greenStyle : redStyle);
            y += height + 5;
            GUI.Label(new Rect(x, y, width, height), $"Left Hand Selected (Press {leftHandSelection})", leftHandSelected ? greenStyle : redStyle);
            y += height + 5;
            GUI.Label(new Rect(x, y, width, height), $"Grab (Press {grabKey})", Input.GetKey(grabKey) ? greenStyle : redStyle);
            y += height + 5;
            GUI.Label(new Rect(x, y, width, height), $"Rotation Selected (Press {rotationKeycode})", rotationSelected ? greenStyle : redStyle);
        }

        void CheckSelectionInput() {
            if(handRight != null) {
                rightHandSelected = Input.GetKey(rightHandSelection);
            }

            if(handLeft != null) {
                leftHandSelected = Input.GetKey(leftHandSelection);
            }

            rotationSelected = Input.GetKey(rotationKeycode);
            headRoationActive = !rightHandSelected && !leftHandSelected;

        }
        float xRotation = 0f;
        void CheckMovementInput() {
            Vector2 mousePositionDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * Time.deltaTime * 4;

            if(Input.GetKeyDown(KeyCode.LeftControl))
                UnityEngine.Cursor.lockState = UnityEngine.Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;

            if(rightHandSelected) {
                if(rotationSelected) {
                    handRightFollow.Rotate(handRightFollow.forward, -mousePositionDelta.x * mouseHandRotationSpeed);
                    handRightFollow.Rotate(handRightFollow.right, mousePositionDelta.y * mouseHandRotationSpeed);
                    handRightBound.transform.Rotate(handRightFollow.forward, -mousePositionDelta.x * mouseHandRotationSpeed);
                    handRightBound.transform.Rotate(handRightFollow.right, mousePositionDelta.y * mouseHandRotationSpeed);

                }
                else {
                    handRightFollow.position += handRightFollow.right * mousePositionDelta.x * mouseHandMovementSpeed;
                    handRightFollow.position += handRightFollow.forward * mousePositionDelta.y * mouseHandMovementSpeed;
                }

                if(handRightBound != null) {
                    handRightFollow.position = handRightBound.ClosestPoint(handRightFollow.position);
                }
            }
            else {
                var handReturnSpeed = this.handReturnSpeed * Time.deltaTime * 60;
                handReturnSpeed = handReturnSpeed * Vector3.Distance(handRightFollow.localPosition, handRightStartPosition) > 0.001f ? handReturnSpeed : 0;
                if(handReturnSpeed > 0)
                    handRightFollow.localPosition = Vector3.MoveTowards(handRightFollow.localPosition, handRightStartPosition, handReturnSpeed + 0.001f);
            }

            if(leftHandSelected) {
                if(rotationSelected) {
                    handLeftFollow.Rotate(handLeftFollow.forward, mousePositionDelta.x * mouseHandRotationSpeed);
                    handLeftFollow.Rotate(handLeftFollow.right, mousePositionDelta.y * mouseHandRotationSpeed);
                    handLeftBound.transform.Rotate(handLeftFollow.forward, mousePositionDelta.x * mouseHandRotationSpeed);
                    handLeftBound.transform.Rotate(handLeftFollow.right, mousePositionDelta.y * mouseHandRotationSpeed);
                }
                else {
                    handLeftFollow.position += handLeftFollow.right * mousePositionDelta.x * mouseHandMovementSpeed;
                    handLeftFollow.position += handLeftFollow.forward * mousePositionDelta.y * mouseHandMovementSpeed;
                }

                if(handLeftBound != null) {
                    handLeftFollow.position = handLeftBound.ClosestPoint(handLeftFollow.position);
                }
            }
            else {
                var handReturnSpeed = this.handReturnSpeed * Time.deltaTime * 60;
                handReturnSpeed = handReturnSpeed * Vector3.Distance(handLeftFollow.localPosition, handLeftStartPosition) > 0.001f ? handReturnSpeed : 0;
                if(handReturnSpeed > 0)
                    handLeftFollow.localPosition = Vector3.MoveTowards(handLeftFollow.localPosition, handLeftStartPosition, handReturnSpeed + 0.001f);
            }

            if(headRoationActive) {
                if(rotationSelected) {
                    xRotation -= mouseLookRotationSpeed * 3 * mousePositionDelta.y;
                    xRotation = Mathf.Clamp(xRotation, -15f, 45f);
                    headCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                }

                trackingContainer.transform.Rotate(Vector3.up * mouseLookRotationSpeed * 10 * mousePositionDelta.x);
            }

            if(Input.GetKeyDown(grabKey)) {
                if(rightHandSelected) {
                    if(handRight.IsHolding())
                        handRight.Release();
                    else
                        handRight.Grab();
                }

                if(leftHandSelected) {
                    if(handLeft.IsHolding())
                        handLeft.Release();
                    else
                        handLeft.Grab();
                }
            }

            var fowardAxis = Input.GetAxis("Vertical");
            var rightAxis = Input.GetAxis("Horizontal");
            player.Move(new Vector2(rightAxis, fowardAxis));
        }

        void KeepPointWithinBox(Transform point, BoxCollider box) {
            if(box == null) return;

            Vector3 localPos = box.transform.InverseTransformPoint(transform.position);
            Vector3 halfExtents = box.size * 0.5f;

            localPos.x = Mathf.Clamp(localPos.x, -halfExtents.x, halfExtents.x);
            localPos.y = Mathf.Clamp(localPos.y, -halfExtents.y, halfExtents.y);
            localPos.z = Mathf.Clamp(localPos.z, -halfExtents.z, halfExtents.z);

            point.position = box.transform.TransformPoint(localPos);
        }
    }
}