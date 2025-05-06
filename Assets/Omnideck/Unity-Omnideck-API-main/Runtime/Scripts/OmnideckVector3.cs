/*
   Copyright 2017-2023 MSE Omnifinity AB
   The code below is a simple example of moving a transform based on 
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
using System.Collections;

namespace Omnifinity
{
	namespace Omnideck
	{
		public class OmnideckVector3 : MonoBehaviour
		{

			// debugging stuff
			public enum LogLevel { None, Terse, Verbose }
			public LogLevel debugLevel = LogLevel.Verbose;

			// our interface of interest
			[SerializeField]
			OmnideckInterface _omnideckInterface;

			// Camera eye transform for positioning of head collider
			Transform cameraTransform = null;

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
			}

			// move the object
			void Update()
			{
				// escape if we have not gotten hold of the interface component
				if (!_omnideckInterface)
					return;

				// calculate movement vector since last pass [m/s]
				Vector3 currMovementVector = _omnideckInterface.GetCurrentOmnideckCharacterMovementVector();

				// disregard height changes
				Vector3 bodyMovementVector = new Vector3(currMovementVector.x, 0, currMovementVector.z);

				// Simply translate the transform ([m/s] * [s] = [m])
				// (in a normal use case you'd have some code/raycasting for ground/object collision)
				transform.Translate(bodyMovementVector * Time.deltaTime);

				// Call some prototype code
				// ATTN: this can change anytime
				//PrototypeCodeSubjectToChange();
			}
			#endregion MonoBehaviorMethods

			#region OmnideckMethods_Beta
			// various prototype code in development below
			void PrototypeCodeSubjectToChange()
			{
				_omnideckInterface.DevRequestChangeOfOmnideckOperationMode();
			}
			#endregion OmnideckMethods_Beta
		}
	}
}