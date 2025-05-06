/*
   Copyright 2023 MSE Omnifinity AB
   The code below is part of the Omnideck Unity API

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
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Omnifinity
{
    namespace Omnideck
    {

		#region OmnideckEnums
		public enum ETreadmillStatus {
			Stopped = 1,
			Running = 2,
			Stopped_OnGameRequest = 3
		};

		enum EUserRequestOperateTreadmill {
			Request_Disable = 100,
			Request_Enable = 200
		}

		enum ESystemReplyOperateTreadmill {
			Result_NotAllowed = 1,
			Result_NoConnection = 10,
			Result_Disabled_Ok = 100,
			Result_Enabled_Ok = 200
		}
		#endregion OmnideckEnums

        public class OmnideckInterface: MonoBehaviour
        {

			public enum LogLevel {None, TerseUserMovementVector, Verbose}
			public LogLevel debugLevel = LogLevel.Verbose;

            #region OmnitrackAPIImports
            [DllImport("OmnitrackAPI")]
			private static extern UInt16 GetAPIVersionMajor ();
			[DllImport("OmnitrackAPI")]
			private static extern UInt16 GetAPIVersionMinor ();
			[DllImport("OmnitrackAPI")]
			private static extern UInt16 GetAPIVersionPatch ();

            // Initialize Omnideck API communication
            [DllImport("OmnitrackAPI")]
            private static extern int InitializeOmnitrackAPI();

            // Establish network connection with Omnideck
			[DllImport("OmnitrackAPI")]
			private static extern int EstablishOmnitrackConnection(ushort serverPort, string trackerName);

            // Close the connection with Omnideck
            [DllImport("OmnitrackAPI")]
            private static extern int CloseOmnitrackConnection();

            // Run the Omnideck mainloop each frame to properly receive new data
            [DllImport("OmnitrackAPI")]
			[return:MarshalAs(UnmanagedType.I1)]
            private static extern void UpdateOmnitrack();

			// Check if the treadmill is online and communicating with game
			[DllImport("OmnitrackAPI")]
			[return:MarshalAs(UnmanagedType.I1)]
			private static extern bool IsOmnitrackOnline ();

			// Get treadmill speed [m/s]
			[DllImport("OmnitrackAPI")]
			private static extern float GetTreadmillSpeed();

			// Get treadmill state
			[DllImport("OmnitrackAPI")]
			private static extern int GetTreadmillState();

			// Send heart beat to Omnideck and notify it which DLL version and platform this game is using
			[DllImport("OmnitrackAPI")]
			private static extern int SendHeartbeatToOmnitrack(UInt16 major, UInt16 minor, UInt16 patch, UInt16 platform);

			// Get the FPS of arriving tracking data
			[DllImport("OmnitrackAPI")]
			private static extern double getTrackingDataFPS();

            // Get X-, Y- and Z movement speed
            [DllImport("OmnitrackAPI")]
            private static extern double getVX();

            [DllImport("OmnitrackAPI")]
            private static extern double getVY();

            [DllImport("OmnitrackAPI")]
            private static extern double getVZ();
			#endregion OmnitrackAPIImports

			#region OmnitrackAPIImports_Beta
			// Handshake between this projects DLL API-version and Omnideck-version
			[DllImport("OmnitrackAPI")]
			private static extern int PerformOmnitrackHandshake();

			// Has new data arrived or not
			// ATTN: Subject to change
			[DllImport("OmnitrackAPI")]
			[return:MarshalAs(UnmanagedType.I1)]
			private static extern bool hasNewDataArrived(int sensor);

			// Timestamp of last message (second part)
			// ATTN: Subject to change
			[DllImport("OmnitrackAPI")]
			private static extern long getTimeValSecOfLastMessage();

			// Timestamp of last message (millisecond part)
			// ATTN: Subject to change
			[DllImport("OmnitrackAPI")]
			private static extern long getTimeValUSecOfLastMessage();
			private long lastMessageSec, lastMessageUSec;

			// Delta-time since last received message
			// ATTN: Subject to change
			[DllImport("OmnitrackAPI")]
			private static extern double getTimeValDurationOfLastMessage();

			// Send a request to Omnideck that you'd like to stop the Omnideck
			// ATTN: Implementation not finished.
			[DllImport("OmnitrackAPI")]
			private static extern ESystemReplyOperateTreadmill UserRequestToStopTreadmill();

			// Send a request to Omnideck that you'd like to start the Omnideck
			// ATTN: Implementation not finished.
			[DllImport("OmnitrackAPI")]
			private static extern ESystemReplyOperateTreadmill UserRequestToStartTreadmill();

			// TODO:
			// Create acknowledge events to tell the user she allowed to start / stop the Omnideck
			[DllImport("OmnitrackAPI")]
			[return:MarshalAs(UnmanagedType.I1)]
			private static extern bool IsAllowedToStartTreadmill();

			[DllImport("OmnitrackAPI")]
			[return:MarshalAs(UnmanagedType.I1)]
			private static extern bool IsAllowedToStopTreadmill();
            #endregion OmnitrackAPIImports_Beta

            #region OmnideckVariables
            // 1 == Unity. Do not edit.
            private ushort API_Platform_Type = 1;

            // How often to receive motion velocity data from Omnideck.
            // ATTN: Subject to change.
            const float desiredFps_TrackingData = 75f;
			double OmnideckFps_TrackingData = 0;

			// Keep track of current and previous velocity to be able to calculate a movement vel
			bool hasReceivedStartVelocity = false;
			static Vector3 currVelocity;
			static Vector3 currMovementVector;
			Vector3 prevVelocity;

            // Various variables during development
			// ATTN: Subject to change
            double timeValOfCurrTrackingMessage, timeValOfPrevTrackingMessage;
            uint numberOfSimilarTrackingDataMessages = 0;

			string strAPIVersion = "";
			string strOmnideckVersion = "";
			bool isHandShakeFinished = false;

			// Port and trackername. Should normally not be changed.
			public ushort port = 3889;
			public string trackerName = "AppToOmnitrackTracker0";
			#endregion OmnideckVariables


			bool _hasReceivedTrackingDataFPS = false;

			IEnumerator _trackingDataCoroutine;

			#region MonoBehaviourMethods
            // Setup Omnideck communication, SteamVR connection, Unity Character 
            // Controller component and start various coroutines
            virtual public void Start()
            {
                // Initialize communication with the Omnideck API
                InitializeOmnitrackAPI();

                // Establish the connection (uses VRPN)
                if (EstablishOmnitrackConnection(port, trackerName) == 0)
                {
                    // Periodically communicate and acquire tracking data from Omnideck
					_trackingDataCoroutine = AcquireTrackingData (1.0f / desiredFps_TrackingData);
					StartCoroutine(_trackingDataCoroutine);

                    // Periodically tell Omnideck we are alive
                    StartCoroutine(SendHeartBeat());

					// Periodically check the state of the Omnideck
					StartCoroutine(CheckOmnitrackState());

					if (debugLevel != LogLevel.None)
	                    Debug.Log("Successful setup of communication handlers with Omnideck");
                }
                else
                {
					if (debugLevel != LogLevel.None)
	                    Debug.LogError("Unable to setup communication handlers with Omnideck");
                }
            }

			// Shut down the connection to Omnideck
            void OnDestroy()
            {
                Debug.Log("OnDestroy()");

				if (CloseOmnitrackConnection() == 0)
				{
					if (debugLevel != LogLevel.None)
						Debug.Log("Closed down communication with Omnideck");
				}
				else
				{
					if (debugLevel != LogLevel.None)
						Debug.LogWarning("Unable to properly close down ommunication with Omnideck");
				}
			}
			#endregion MonoBehaviourMethods

			#region OmnitrackAPIMethods
			// Get the current velocity of the person walking on the omnideck
			public static Vector3 GetOmnitrackCharacterVelocity()
			{
				return new Vector3((float)getVX(), (float)getVY(), (float)getVZ());
			}

			// Update the omnideck users position/movement vector
			private void UpdateOmnitrackCharacterMovement() {
				// if there is no connection, set velocity to zero and escape
				if (!IsOmnitrackOnline ()) {
					currMovementVector = Vector3.zero;
					return;
				}

                currVelocity = GetOmnitrackCharacterVelocity();

				// make sure the initial starting velocity does not result in a large jump
				if (!hasReceivedStartVelocity) {
					if (debugLevel != LogLevel.None)
						Debug.Log ("Resetting start velocity for calculation of the Omnideck character movement");
                    hasReceivedStartVelocity = !hasReceivedStartVelocity;
                    // set same previous and current position
                    prevVelocity = currVelocity;
				}

				// update movement vector (if we've received which fps to run at from Omnideck)
				if (OmnideckFps_TrackingData > 0)
					currMovementVector = currVelocity;
				else
					currMovementVector = Vector3.zero;

				// cap the vector if it is very high (e.g. when Omnideck starts and headset goes from
				// lying on the centerplate to being moved
				Vector3 vectorToCheck = new Vector3 (currMovementVector.x, 0, currMovementVector.z);
				float vel = vectorToCheck.magnitude;
				if (vel > 3.0) {
					Debug.Log ("Received potential high initial movement speed, capping");
					currMovementVector = Vector3.zero;
				}

                // store current velocity for next pass
                prevVelocity = currVelocity;

				if (debugLevel == LogLevel.TerseUserMovementVector ) {
					Debug.Log ("User movementVector: " + currMovementVector);
				}
			}

            // returns the current accumulated position of the omnideck user.
            // Unit = [m]
            [System.Obsolete("This method has been deprecated.")]
            public static Vector3 GetCurrentOmnideckCharacterPosition() {
				return Vector3.zero;
			}

			// returns the current movement vector of the omnideck user.
			// Unit = [m/s]
			public Vector3 GetCurrentOmnideckCharacterMovementVector() {
				return currMovementVector;
			}

            // Acquire tracking data from Omnideck
            // ATTN: Subject to change
            IEnumerator AcquireTrackingData(float waitTime)
            {
                while (true)
                {
					// Update against Omnideck API
                    UpdateOmnitrack();

					// Update Omnideck Character position/movement data
					UpdateOmnitrackCharacterMovement ();

                    yield return new WaitForSeconds(waitTime);
                }
            }

            // Periodically send heatbeat to Omnideck to notify that game is alive
            IEnumerator SendHeartBeat()
            {
                float waitTime = 1.0f;
                while (true)
                {
					if (IsOmnitrackOnline ())
                    {
						SendHeartbeatToOmnitrack(GetAPIVersionMajor(), GetAPIVersionMinor(), GetAPIVersionPatch(), API_Platform_Type);
						if (debugLevel != LogLevel.None)
							Debug.Log("Sent heartbeat to Omnideck, using API v" + GetAPIVersionMajor().ToString () + "." + GetAPIVersionMinor().ToString () + "." + GetAPIVersionPatch().ToString ());
                    }
                    else
                    {
						if (debugLevel != LogLevel.None)
	                        Debug.LogWarning("Unable to send heartbeat to Omnideck - connection down");
                    }
                    yield return new WaitForSeconds(waitTime);
                }
            }

			// Get the state of the treadmill
			public ETreadmillStatus GetTreadmillStatus() {
				if (IsOmnitrackOnline ()) {
					return (ETreadmillStatus)GetTreadmillState ();
				} else {
					if (debugLevel != LogLevel.None)
						Debug.LogError ("Unable to check status, not connected to Omnideck");
					return ETreadmillStatus.Stopped;
				}
			}

			// Periodically check the state of the Omnideck
			IEnumerator CheckOmnitrackState()
			{
				float waitTime = 1.0f;
				while (true)
				{
					if (IsOmnitrackOnline ()) {
						ETreadmillStatus treadmillState = GetTreadmillStatus();
						switch (treadmillState) {
						case ETreadmillStatus.Stopped:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Treadmill stopped. User at center or outside of bounds.");
							break;

						case ETreadmillStatus.Stopped_OnGameRequest:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Treadmill stopped on game request. This is room scale mode.");
							break;

						case ETreadmillStatus.Running:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Treadmill running. User on active surface.");
							break;

						default:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Unsupported treadmill state");
							break;
						}

						OmnideckFps_TrackingData = getTrackingDataFPS ();
						if (OmnideckFps_TrackingData > 0) {
							if (!_hasReceivedTrackingDataFPS) {
								_hasReceivedTrackingDataFPS = true;
								if (debugLevel != LogLevel.None)
									Debug.Log ("Tracking data arrives at FPS: " + OmnideckFps_TrackingData);
								StopCoroutine (_trackingDataCoroutine);
								_trackingDataCoroutine = AcquireTrackingData (1.0f / (float)OmnideckFps_TrackingData);
								StartCoroutine (_trackingDataCoroutine);
							}
						} else {
							Debug.Log ("Have not received tracking data FPS setting from Omnideck yet");
						}
					} else {
						if (debugLevel != LogLevel.None)
							Debug.LogWarning ("Omnideck connection offline");
						currMovementVector = Vector3.zero;
					}
					yield return new WaitForSeconds(waitTime);
				}
			}
			#endregion

			#region OmnitrackAPIMethods_Beta
			// Unfinished/unverified code that is in active development
			// ATTN: Subject to change

			// Handshake version check
			// ATTN: Subject to change
			IEnumerator PerformVersionHandshake()
			{
				float waitTime = 1.0f;
				while (true && !isHandShakeFinished)
				{
					if (debugLevel != LogLevel.None)
						Debug.Log ("Handshake not finished");
					if (IsOmnitrackOnline ())
					{
						if (debugLevel != LogLevel.None)
							Debug.Log ("Connection is enabled");
						if (PerformOmnitrackHandshake () == 0) {

							UInt16 verAPIMajor, verAPIMinor, verAPIPatch;
							verAPIMajor = GetAPIVersionMajor ();
							verAPIMinor = GetAPIVersionMinor ();
							verAPIPatch = GetAPIVersionPatch ();
							strAPIVersion = verAPIMajor.ToString () + "." + verAPIMinor.ToString () + "." + verAPIPatch.ToString ();
							if (debugLevel != LogLevel.None)
								Debug.Log ("Using API Version: " + strAPIVersion);
						}
					}
					else
					{
						if (debugLevel != LogLevel.None)
							Debug.LogWarning("Unable to handshake with Omnideck - connection down");
					}
					yield return new WaitForSeconds(waitTime);
				}
				yield return null;
			}

            // Code in dev, will enable users to disable/enable omnideck upon will 
            // ("forced roomscale")
            // ATTN: Subject to change
			public void DevRequestChangeOfOmnideckOperationMode()
            {
                // escape the rest
                return;

				if (Input.GetMouseButtonDown (0)) {
					if (IsOmnitrackOnline ()) {
						ESystemReplyOperateTreadmill resOperateTreadmillRequest = UserRequestToStopTreadmill ();
						switch (resOperateTreadmillRequest) {
						case ESystemReplyOperateTreadmill.Result_NotAllowed:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Not allowed to send user request to stop the Omnideck");
							break;
						case ESystemReplyOperateTreadmill.Result_Disabled_Ok:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Sent user request to stop the Omnideck treadmill. Treadmill disabled.");
							break;
						}
					} else {
						if (debugLevel != LogLevel.None)
							Debug.LogError ("Unable to send request, not connected to Omnideck");
					}
				}

				if (Input.GetMouseButtonDown (1)) {
					if (IsOmnitrackOnline ()) {
						ESystemReplyOperateTreadmill resOperateTreadmillRequest = UserRequestToStartTreadmill ();
						switch (resOperateTreadmillRequest) {
						case ESystemReplyOperateTreadmill.Result_NotAllowed:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Not allowed to send user request to start the Omnideck");
							break;
						case ESystemReplyOperateTreadmill.Result_Enabled_Ok:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Sent user request to start the Omnideck treadmill. Treadmill enabled.");
							break;
						}
					} else {
						if (debugLevel != LogLevel.None)
							Debug.LogError ("Unable to send request, not connected to Omnideck");
					}
				}
            }

            public void DevRequestStopTreadmill()
            {
                if (IsOmnitrackOnline())
                {
                    ESystemReplyOperateTreadmill resOperateTreadmillRequest = UserRequestToStopTreadmill();
                    switch (resOperateTreadmillRequest)
                    {
                        case ESystemReplyOperateTreadmill.Result_NotAllowed:
                            if (debugLevel != LogLevel.None)
                                Debug.Log("Not allowed to send user request to stop the Omnideck");
                            break;
                        case ESystemReplyOperateTreadmill.Result_Disabled_Ok:
                            if (debugLevel != LogLevel.None)
                                Debug.Log("Sent user request to stop the Omnideck treadmill. Treadmill disabled.");
                            break;
                    }
                }
                else
                {
                    if (debugLevel != LogLevel.None)
                        Debug.LogError("Unable to send request, not connected to Omnideck");
                }
            }

            public void DevRequestStartTreadmill()
            {
                if (IsOmnitrackOnline())
                {
                    ESystemReplyOperateTreadmill resOperateTreadmillRequest = UserRequestToStartTreadmill();
                    switch (resOperateTreadmillRequest)
                    {
                        case ESystemReplyOperateTreadmill.Result_NotAllowed:
                            if (debugLevel != LogLevel.None)
                                Debug.Log("Not allowed to send user request to start the Omnideck");
                            break;
                        case ESystemReplyOperateTreadmill.Result_Enabled_Ok:
                            if (debugLevel != LogLevel.None)
                                Debug.Log("Sent user request to start the Omnideck treadmill. Treadmill enabled.");
                            break;
                    }
                }
                else
                {
                    if (debugLevel != LogLevel.None)
                        Debug.LogError("Unable to send request, not connected to Omnideck");
                }
            }

			#endregion OmnitrackAPICode_Beta
        }
    }
}
