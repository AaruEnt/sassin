using System;
using System.Runtime.InteropServices;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// Represents a joint of an <see cref="XRHand"/>.
    /// </summary>
    /// <remarks>
    /// The term "joint" should be taken loosely in this context. In addition to the
    /// anatomical finger joints, the list of joints includes the fingertips, a point on the palm
    /// and a point on the wrist. See <see cref="XRHandJointID"/> for the full list of
    /// joints.
    ///
    /// Refer to [Hand data model](xref:xrhands-data-model) for a description of the joint locations
    /// and the data they contain.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct XRHandJoint
    {
        /// <summary>
        /// The ID of this joint.
        /// </summary>
        /// <value>The joint ID.</value>
        public XRHandJointID id => (XRHandJointID)(m_IdAndHandedness & ~k_IsRightHandBit);

        /// <summary>
        /// Denotes which hand this joint is on.
        /// </summary>
        /// <value>Right or left.</value>
        public Handedness handedness => (m_IdAndHandedness & k_IsRightHandBit) != 0 ? Handedness.Right : Handedness.Left;

        /// <summary>
        /// Represents which tracking data is valid.
        /// </summary>
        /// <value>A flag is set for each valid type of data. If the
        /// The <see cref="XRHandJointTrackingState.WillNeverBeValid"/> flag is set
        /// when this joind ID isn't supported by the hand data provider.</value>
        public XRHandJointTrackingState trackingState => m_TrackingState;

        /// <summary>
        /// Retrieves the joint's radius, if available.
        /// </summary>
        /// <param name="radius">
        /// Assigned the tracked radius of this joint, if successful.
        /// Set to zero, if unsuccessful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and the radius was
        /// filled out with valid tracking data, returns <see langword="false"/>
        /// otherwise.
        /// </returns>
        public bool TryGetRadius(out float radius)
        {
            if ((m_TrackingState & XRHandJointTrackingState.Radius) == XRHandJointTrackingState.None)
            {
                radius = 0.0f;
                return false;
            }

            radius = m_Radius;
            return true;
        }

        /// <summary>
        /// Retrieves the joint's pose, if available.
        /// </summary>
        /// <param name="pose">
        /// Assigned the tracked pose of this joint, if successful.
        /// Set to <see cref="Pose.identity"/>, if unsuccessful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and the joint pose was
        /// filled out with valid tracking data, returns <see langword="false"/>
        /// otherwise.
        /// </returns>
        /// <remarks>
        /// Joint poses are relative to the real-world point chosen by the user's device.
        /// 
        /// To transform to world space so that the joint appears in the correct location
        /// relative to the user, transform the pose based on the
        /// [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </remarks>
        /// <example>
        /// The following example illustrates how to transform a pose into world space using the
        /// transform from the XROrigin object in a scene.
        /// <code>
        /// public Pose ToWorldPose(XRHandJoint joint, Transform origin)
        /// {
        ///     Pose xrOriginPose = new Pose(origin.position, origin.rotation);
        ///     if (joint.TryGetPose(out Pose jointPose))
        ///     {
        ///         return jointPose.GetTransformedBy(xrOriginPose);
        ///     }
        ///     else
        ///         return Pose.identity;
        ///     }
        /// </code>
        /// </example>
        public bool TryGetPose(out Pose pose)
        {
            if ((m_TrackingState & XRHandJointTrackingState.Pose) == XRHandJointTrackingState.None)
            {
                pose = Pose.identity;
                return false;
            }

            pose = m_Pose;
            return true;
        }

        /// <summary>
        /// Retrieves the joint's linear velocity vector, if available.
        /// </summary>
        /// <param name="linearVelocity">
        /// Assigned the tracked linear velocity of this joint, if successful.
        /// Set to <see cref="Vector3.zero"/>, if unsuccessful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and the velocity was
        /// filled out with valid tracking data, returns <see langword="false"/>
        /// otherwise.
        /// </returns>
        /// <remarks>
        /// To transform to world space so that the vector has the correct direction
        /// relative to the user, rotate this by the rotation of the
        /// [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </remarks>
        public bool TryGetLinearVelocity(out Vector3 linearVelocity)
        {
            if ((m_TrackingState & XRHandJointTrackingState.LinearVelocity) == XRHandJointTrackingState.None)
            {
                linearVelocity = Vector3.zero;
                return false;
            }

            linearVelocity = m_LinearVelocity;
            return true;
        }

        /// <summary>
        /// Retrieves the joint's angular velocity vector, if available
        /// </summary>
        /// <param name="angularVelocity">
        /// Assigned the tracked angular velocity of this joint, if successful.
        /// Set to <see cref="Vector3.zero"/>, if unsuccessful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and the angular
        /// velocity filled out with valid tracking data, returns
        /// <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// To transform to world space so that the vector has the correct direction
        /// relative to the user, rotate this by the rotation of the
        /// [XROrigin](xref:Unity.XR.CoreUtils.XROrigin).
        /// </remarks>
        public bool TryGetAngularVelocity(out Vector3 angularVelocity)
        {
            if ((m_TrackingState & XRHandJointTrackingState.AngularVelocity) == XRHandJointTrackingState.None)
            {
                angularVelocity = Vector3.zero;
                return false;
            }

            angularVelocity = m_AngularVelocity;
            return true;
        }

        /// <summary>
        /// Returns a string representation of the XRHandJoint.
        /// </summary>
        /// <returns>
        /// String representation of the value.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                "[{0} {1}] Pose: {2} | Radius: {3} | Linear Velocity: {4} | Angular Velocity: {5} | Tracking State: {6}",
                handedness, id, m_Pose.ToString("F4"), m_Radius.ToString("F4"), m_LinearVelocity.ToString("F4"), m_AngularVelocity.ToString("F4"), m_TrackingState);
        }

        internal int m_IdAndHandedness;
        internal Pose m_Pose;
        internal float m_Radius;
        internal Vector3 m_LinearVelocity;
        internal Vector3 m_AngularVelocity;
        internal XRHandJointTrackingState m_TrackingState;

        internal const int k_IsRightHandBit = 1 << 31;
    }
}
