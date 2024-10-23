/*
   Copyright 2023 MSE Omnifinity AB
   The code below is a simple example of using a standard Unity CharacterController
   attached to the SteamVR CameraRig for moving the Omnideck user around based on
   position data arriving from Omnideck.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 */

using UnityEngine;

namespace Omnifinity
{
	namespace Omnideck
	{

		public class OmnideckCharacterController : MonoBehaviour
		{

			// debugging stuff
			public enum LogLevel { None, Terse, Verbose }
			public LogLevel debugLevel = LogLevel.Verbose;

			// The XR Origin (XR Rig)
			public GameObject xrRig = null;

			// The XR camera
			public GameObject xrCamera = null;

            // The interface to Omnideck
            [SerializeField]
            OmnideckInterface _omnideckInterface;

			// Camera eye transform for positioning of head collider
			Transform _cameraTransform = null;

			// The standard Unity CharacterController
			CharacterController _characterController = null;

			#region MonoBehaviorMethods
			// setup various things
			void Start()
			{
				// get hold of the Omnideck interface component
				if (_omnideckInterface)
				{
					if (debugLevel != LogLevel.None)
						Debug.Log("OmnideckInterface object: " + _omnideckInterface);
				}
				else
				{
					if (debugLevel != LogLevel.None)
						Debug.Log("Unable to find OmnideckInterface component on object. Please add an OmnideckInterface component.", gameObject);
					return;
				}

				// get hold of the xr camera and its transform
				if (xrCamera)
				{
					if (debugLevel != LogLevel.None)
						Debug.Log("XR Camera: " + xrCamera, xrCamera);
					_cameraTransform = xrCamera.transform;
				}
				else
				{
					if (debugLevel != LogLevel.None)
						Debug.LogError("Unable to find XR Camera object");
					return;
				}

				// Get hold of the Unity Character Controller. This object is what we move.
				_characterController = transform.GetComponent<CharacterController>();
				if (_characterController)
				{
					if (debugLevel != LogLevel.None)
						Debug.Log("Unity Character Controller: ", _characterController);
				}
				else
				{
					if (debugLevel != LogLevel.None)
						Debug.LogError("Unable to find Character Controller object");
				}
			}


			// move the object
			void Update()
			{
				// escape if we have not gotten hold of the interface component
				if (!_omnideckInterface)
					return;

				if (_characterController == null)
				{
					if (debugLevel != LogLevel.None)
						Debug.LogError("Unable to move charactercontroller");
					return;
				}

				// calculate movement vector since last pass [m/s]
				Vector3 newMovementVector = _omnideckInterface.GetCurrentOmnideckCharacterMovementVector();

				// disregard height changes
				Vector3 currMovementVector = new Vector3(newMovementVector.x, 0, newMovementVector.z);

				// first move the character controller based on the movement vector [m/s] ...
				_characterController.SimpleMove(currMovementVector);

				// ...and secondly move the center of the capsule collider along with the head
				// so that the user cannot move through walls
				if (_cameraTransform != null)
					_characterController.center = new Vector3(_cameraTransform.localPosition.x, 0, _cameraTransform.localPosition.z);

				// ... and thirdly make scale the height of the character collider to match head position
				// TBD. Please check how this is done in the XR Interaction toolkit for inspiration on your own.
			}
			#endregion MonoBehaviorMethods
		}
	}
}