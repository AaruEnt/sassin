using System;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// Event data associated with the event when XR Hand joints are updated.
    /// </summary>
    public class XRHandJointsUpdatedEventArgs
    {
        /// <summary>
        /// The data for the XR Hand.
        /// </summary>
        public XRHand hand;

        /// <summary>
        /// The subsystem that is the source of the hand tracking data.
        /// </summary>
        public XRHandSubsystem subsystem { get; internal set; }
    }
}

