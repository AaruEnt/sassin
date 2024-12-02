//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Provides a haptic bump when colliding with balloons
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class BalloonHapticBump : MonoBehaviour
    {
#if !UNITY_ANDROID
		public GameObject physParent;

		//-------------------------------------------------
		void OnCollisionEnter( Collision other )
		{
			Balloon contactBalloon = other.collider.GetComponentInParent<Balloon>();
			if ( contactBalloon != null )
			{
				Hand hand = physParent.GetComponentInParent<Hand>();
				if ( hand != null )
				{
					hand.TriggerHapticPulse( 500 );
				}
			}
		}
#endif
	}
}
