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
    public class ImprovedThrow : MonoBehaviour
    {
        [Tooltip("The time offset used when releasing the object with the RawFromHand option")]
        public float releaseVelocityTimeOffset = -0.011f;

        public float scaleReleaseVelocity = 1.1f;

        [Tooltip("The release velocity magnitude representing the end of the scale release velocity curve. (-1 to disable)")]
        public float scaleReleaseVelocityThreshold = -1.0f;
        [Tooltip("Use this curve to ease into the scaled release velocity based on the magnitude of the measured release velocity. This allows greater differentiation between a drop, toss, and throw.")]
        public AnimationCurve scaleReleaseVelocityCurve = AnimationCurve.EaseInOut(0.0f, 0.1f, 1.0f, 1.0f);

        public Rigidbody body;

        public bool assistEnabled;
        public bool addDelay = false;

        public float minDist = 0f;
        public float maxDist = 30f;

        internal VelocityTracker velocityTracker = new VelocityTracker(30);

        private Grabbable _grabbable;
        private ThrowTargetHelper currentTarget;

        private AnimationCurve assistRampCustomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private void Awake()
        {
            _grabbable = GetComponent<Grabbable>();
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
            if (_grabbable)
                velocityTracker.UpdateBuffer(body, _grabbable);

            if (_grabbable != null && _grabbable.IsHeld())
            {
                ThrowTargetHelper best = FindBestGazeBasedThrowTarget(ThrowTargetHelper.AllTargets);
                if (currentTarget)
                {
                    if (currentTarget != best)
                    {
                        currentTarget.RemoveTargettingHandle(this);
                        currentTarget = best;
                        if (currentTarget)
                        {
                            currentTarget.AddTargettingHandle(this);
                        }
                    }
                }
                else if (best)
                {
                    currentTarget = best;
                    currentTarget.AddTargettingHandle(this);
                }
            }
        }

        public void OnThrow(Autohand.Hand hand, Grabbable g)
        {
            StartCoroutine(GetSetVelocity(hand));
            if (currentTarget)
                currentTarget.RemoveTargettingHandle(this);
            currentTarget = null;
        }

        private IEnumerator GetSetVelocity(Autohand.Hand hand)
        {
            Vector3 velocity;
            Vector3 angularVelocity;
            GetReleaseVelocities(hand, out velocity, out angularVelocity);

            if (addDelay)
                yield return null;

            body.velocity = velocity;
            body.angularVelocity = angularVelocity;
            yield return null;
        }

        public virtual void GetReleaseVelocities(Autohand.Hand hand, out Vector3 velocity, out Vector3 angularVelocity)
        {
            velocityTracker.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
            
            float scaleFactor = 2.0f;
            if (scaleReleaseVelocityThreshold > 0)
            {
                scaleFactor = Mathf.Clamp01(scaleReleaseVelocityCurve.Evaluate(velocity.magnitude / scaleReleaseVelocityThreshold));
            }

            velocity *= (scaleFactor * scaleReleaseVelocity);

            //UnityEngine.Debug.Log(string.Format("Pre assist: {0}", velocity));
            //UnityEngine.Debug.Log(angularVelocity);
            var pa = velocity;
            if (currentTarget)
            {
                velocity = ApplyAssist(velocity, transform.position, currentTarget.GetTargetPosition());
                //angularVelocity = ApplyAssist(angularVelocity, transform.position, currentTarget.GetTargetPosition());
            }
            velocity.y = pa.y;
            //UnityEngine.Debug.Log(string.Format("Post assist: {0}", velocity));
            //UnityEngine.Debug.Log(angularVelocity);
        }

        private ThrowTargetHelper FindBestGazeBasedThrowTarget(List<ThrowTargetHelper> targets)
        {
            Camera cam = Camera.main;
            ThrowTargetHelper best = null;
            float minAngle = float.MaxValue;
            for (int i = 0; i < targets.Count; i++)
            {
                float angle = Vector3.Angle(cam.transform.forward, (targets[i].transform.position - cam.transform.position));
                if (angle < minAngle)
                {
                    minAngle = angle;
                    best = targets[i];
                }
            }
            float dist = 0f;
            if (best)
                dist = Vector3.Distance(cam.transform.position, best.transform.position);
            UnityEngine.Debug.LogFormat("Distance: {0}", dist);
            if (dist >= minDist && dist <= maxDist)
                return best;
            return null;
        }

        private Vector3 ApplyAssist(Vector3 rawVelocity, Vector3 origin, Vector3 targetPosition)
        {
            Vector3 ideal1, ideal2;
            //UnityEngine.Debug.LogFormat("Origin: {0}\nVelocity: {1}\nTarget Pos: {2}\n", origin, rawVelocity.magnitude, targetPosition);
            int numSolutions = BallisticsUtility.solve_ballistic_arc(origin, rawVelocity.magnitude, targetPosition, -Physics.gravity.y, out ideal1, out ideal2);

            //cannot apply assist
            if (numSolutions == 0) return rawVelocity * scaleReleaseVelocity;

            //find which potential solution is closer to actual throw
            Vector3 idealVelocity = Vector3.Angle(rawVelocity, ideal1) < Vector3.Angle(rawVelocity, ideal2) ? ideal1 : ideal2;

            //1 is perfect accuracy, 0 is the edge of assistable range
            float rawAccuracy = (1 - Vector3.Angle(rawVelocity, idealVelocity) / 45); // magic number is assistrangedegrees, 0-180
            if (rawAccuracy < 0) return rawVelocity;

            float ramp = assistRampCustomCurve.Evaluate(rawAccuracy);
            ramp *= 0.8f; // range 0-1

            return Vector3.Lerp(rawVelocity, idealVelocity, ramp);
        }
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

            //UnityEngine.Debug.LogFormat("Top: {0}", top);

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
                {
                    UnityEngine.Debug.Log("Broke");
                    break;
                }

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
                velocities[index] = new Velocities();
                velocities[index].vel = rb.velocity;
                velocities[index].angularVel = g.GetAngularVelocity();
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
