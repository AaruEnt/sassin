using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;
using Valve.VR.InteractionSystem;
using UnityEngine.XR;

namespace AaruThrowVR
{
    [RequireComponent(typeof(Grabbable))]
    [RequireComponent(typeof(VelocityEstimator))]
    public class ImprovedThrow : MonoBehaviour
    {
        public ReleaseStyle releaseVelocityStyle = ReleaseStyle.GetFromHand;

        [Tooltip("The time offset used when releasing the object with the RawFromHand option")]
        public float releaseVelocityTimeOffset = -0.011f;

        public float scaleReleaseVelocity = 1.1f;

        [Tooltip("The release velocity magnitude representing the end of the scale release velocity curve. (-1 to disable)")]
        public float scaleReleaseVelocityThreshold = -1.0f;
        [Tooltip("Use this curve to ease into the scaled release velocity based on the magnitude of the measured release velocity. This allows greater differentiation between a drop, toss, and throw.")]
        public AnimationCurve scaleReleaseVelocityCurve = AnimationCurve.EaseInOut(0.0f, 0.1f, 1.0f, 1.0f);

        public Rigidbody body;

        private Grabbable _grabbable;
        protected VelocityEstimator velocityEstimator;

        private void Awake()
        {
            _grabbable = GetComponent<Grabbable>();
            velocityEstimator = GetComponent<VelocityEstimator>();
        }
        private void OnEnable()
        {
            _grabbable.OnReleaseEvent += OnThrow;
        }

        private void OnDisable()
        {
            _grabbable.OnReleaseEvent -= OnThrow;
        }

        public void OnThrow(Autohand.Hand hand, Grabbable g)
        {
            

            StartCoroutine(GetSetVelocity(hand));

            
        }

        private IEnumerator GetSetVelocity(Autohand.Hand hand)
        {
            Vector3 velocity;
            Vector3 angularVelocity;
            GetReleaseVelocities(hand, out velocity, out angularVelocity);
            velocityEstimator.BeginEstimatingVelocity();

            yield return new WaitForEndOfFrame();

            body.velocity = velocity;
            body.angularVelocity = angularVelocity;
        }

        public virtual void GetReleaseVelocities(Autohand.Hand hand, out Vector3 velocity, out Vector3 angularVelocity)
        {
            if (releaseVelocityStyle != ReleaseStyle.NoChange)
            {
                releaseVelocityStyle = ReleaseStyle.ShortEstimation;
            }
            switch (releaseVelocityStyle)
            {
                case ReleaseStyle.ShortEstimation:
                    if (velocityEstimator != null)
                    {
                        velocityEstimator.FinishEstimatingVelocity();
                        velocity = velocityEstimator.GetVelocityEstimate();
                        angularVelocity = velocityEstimator.GetAngularVelocityEstimate();
                        UnityEngine.Debug.Log(string.Format("vel: {0}, angVel: {1}", velocity, angularVelocity));
                    }
                    else
                    {
                        Debug.LogWarning("[SteamVR Interaction System] Throwable: No Velocity Estimator component on object but release style set to short estimation. Please add one or change the release style.");

                        velocity = GetComponent<Rigidbody>().velocity;
                        angularVelocity = GetComponent<Rigidbody>().angularVelocity;
                    }
                    break;
                case ReleaseStyle.NoChange:
                    velocity = GetComponent<Rigidbody>().velocity;
                    angularVelocity = GetComponent<Rigidbody>().angularVelocity;
                    break;
                default:
                    velocity = GetComponent<Rigidbody>().velocity;
                    angularVelocity = GetComponent<Rigidbody>().angularVelocity;
                    break;

            }

            if (releaseVelocityStyle != ReleaseStyle.NoChange)
            {
                float scaleFactor = 1.0f;
                if (scaleReleaseVelocityThreshold > 0)
                {
                    scaleFactor = Mathf.Clamp01(scaleReleaseVelocityCurve.Evaluate(velocity.magnitude / scaleReleaseVelocityThreshold));
                }

                velocity *= (scaleFactor * scaleReleaseVelocity);
            }
        }
    }

    public enum ReleaseStyle
    {
        NoChange,
        GetFromHand,
        ShortEstimation,
        AdvancedEstimation,
    }
}
