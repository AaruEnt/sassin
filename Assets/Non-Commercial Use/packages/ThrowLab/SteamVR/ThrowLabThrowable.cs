using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;


namespace CloudFine.ThrowLab.SteamVR
{
    [RequireComponent(typeof(ThrowHandle))]
    public class ThrowLabThrowable : Throwable
    {
        private ThrowHandle _handle
        {
            get
            {
                if (m_handle == null)
                {
                    m_handle = GetComponent<ThrowHandle>();
                    if (m_handle == null)
                    {
                        m_handle = gameObject.AddComponent<ThrowHandle>();
                    }
                }
                return m_handle;
            }
        }
        private ThrowHandle m_handle;

        protected override void OnAttachedToHand(Hand hand)
        {
            base.OnAttachedToHand(hand);
            //we want to make sure that the ChuckItHandle can ignore collision with the hand. If it's using HandPhysics the colliders will not be children of Hand
            GameObject handCollider = hand.gameObject;
            HandPhysics physics = hand.GetComponent<HandPhysics>();
            if (physics)
            {
                handCollider = physics.handCollider.gameObject;
            }
            _handle.OnAttach(hand.gameObject, handCollider.gameObject);
        }

        protected override void OnDetachedFromHand(Hand hand)
        {
            base.OnDetachedFromHand(hand);
            _handle.OnDetach();
        }

        public override void GetReleaseVelocities(Hand hand, out Vector3 velocity, out Vector3 angularVelocity)
        {
            velocity = _handle.GetVelocityEstimate();
            angularVelocity = _handle.GetAngularVelocityEstimate();
        }
    }
}
