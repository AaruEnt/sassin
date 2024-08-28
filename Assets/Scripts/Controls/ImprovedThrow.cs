using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autohand;
using Valve.VR.InteractionSystem;
using UnityEngine.XR;
using System;
using Valve.VR;
using System.Diagnostics;

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

        internal VelocityTracker velocityTracker = new VelocityTracker(30);

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

        private void Update()
        {
            velocityTracker.UpdateBuffer(body, _grabbable);
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

            yield return null;

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
                        UnityEngine.Debug.LogWarning("[AaruVR System] Throwable: No Velocity Estimator component on object but release style set to short estimation. Please add one or change the release style.");

                        velocity = GetComponent<Rigidbody>().velocity;
                        angularVelocity = GetComponent<Rigidbody>().angularVelocity;
                    }
                    break;
                case ReleaseStyle.NoChange:
                    velocity = GetComponent<Rigidbody>().velocity;
                    angularVelocity = GetComponent<Rigidbody>().angularVelocity;
                    break;
                case ReleaseStyle.AdvancedEstimation:
                    velocityTracker.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
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

    public class VelocityTracker {
        public Velocities[] velocities;

        private int lastFrameUpdated = -1;
        private int index = 0;

        public VelocityTracker(int bufferSize)
        {
            velocities = new Velocities[bufferSize];
        }

        public void GetEstimatedPeakVelocities(out Vector3 velocity, out Vector3 angularVelocity)
        {
            int top = GetTopVelocity(10, 1);

            velocity = Vector3.zero;
            angularVelocity = Vector3.zero;

            Vector3 totalVelocity = Vector3.zero;
            Vector3 totalAngularVelocity = Vector3.zero;
            int forFrames = 2;
            float totalFrames = 0;
            int currentFrame = top;

            while (forFrames > 0)
            {
                forFrames--;
                currentFrame--;

                if (currentFrame < 0)
                    currentFrame = velocities.Length - 1;

                Velocities currentStep = velocities[currentFrame];

                if (IsValid(currentStep) == false)
                    break;

                totalFrames++;

                totalVelocity += currentStep.vel;
                totalAngularVelocity += currentStep.angularVel;
            }

            velocity = totalVelocity / totalFrames;
            angularVelocity = totalAngularVelocity / totalFrames;
        }

        internal int GetTopVelocity(int forFrames, int addFrames)
        {
            int topFrame = index;
            float topVelocitySqr = 0;

            int currentFrame = index;

            while (forFrames > 0)
            {
                forFrames--;
                currentFrame--;

                if (currentFrame < 0)
                    currentFrame = velocities.Length - 1;

                Velocities currentStep = velocities[currentFrame];

                if (IsValid(currentStep) == false)
                    break;

                float currentSqr = velocities[currentFrame].vel.sqrMagnitude;
                if (currentSqr > topVelocitySqr)
                {
                    topFrame = currentFrame;
                    topVelocitySqr = currentSqr;
                }
            }
            topFrame += addFrames;

            if (topFrame >= velocities.Length)
                topFrame -= velocities.Length;

            return topFrame;
        }

        internal bool IsValid(Velocities step)
        {
            return step != null && step.frameCount != -1;
        }

        public void UpdateBuffer(Rigidbody rb, Grabbable g)
        {
            int currentFrame = Time.frameCount;
            if (lastFrameUpdated != currentFrame)
            {
                var gs = g?.GetHeldBy();
                if (g && gs.Count > 0)
                {
                    velocities[index].vel = gs[0].body.velocity;
                    velocities[index].angularVel = gs[0].body.angularVelocity;
                }
                else
                {
                    velocities[index].vel = rb.velocity;
                    velocities[index].angularVel = rb.angularVelocity;
                }
                velocities[index].frameCount = currentFrame;
                index++;
                if (index >= velocities.Length)
                {
                    index = 0;
                }
                lastFrameUpdated = currentFrame;
            }
        }

        public class Velocities
        {
            public int frameCount;
            public Vector3 vel;
            public Vector3 angularVel;
        }
    }
}
