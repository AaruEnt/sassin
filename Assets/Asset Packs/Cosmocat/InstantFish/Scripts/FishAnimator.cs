using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmocat.InstantFish
{
    [RequireComponent(typeof(BoneAttacher))]
    public class FishAnimator : MonoBehaviour
    {

        [Header("References")]
        public Transform jawTransform;
        public Transform tailBone;
        public List<Transform> finBones = new List<Transform>();
        private List<Vector3> finAngles = new List<Vector3>();
        private Vector3 armatureRot;
        private Transform armature;

        [Header("Behaviour - General")]
        [Range(0f, 2f)] public float speed = 0.5f;

        [Header("Behaviour - Jaw")]
        public bool animateJaw = true;
        public float jawCloseAngle = 0f;
        public float jawOpenAngle = 30f;
        public float jawOpenFrequency = 2f;

        private float jawTime = 0f;
        private float jawOpen = 0f;
        private float jawTgt = 1f;

        [Header("Behaviour - Tail")]
        public bool animateTail = true;
        public float tailRange = 20f;
        public float tailRangeTurn = 30f;
        public float tailSpeed = 0.1f;
        private float tailTime = 0f;
        [ReadOnly] public float tailOverride;
        private float prevTail;
        private Coroutine turnTween;

        [Header("Behaviour - Fins")]
        public bool animateFins = true;
        public float finMoveFrequency = 0.5f;
        public float finMinAngle = 20f;
        public float finRange = 45f;
        public Vector3 finRotations = new Vector3(0f, 0f, 20f);
        private float finTime = 0f;

        public void Awake()
        {
            BoneAttacher boneAttacher = GetComponent<BoneAttacher>();
            boneAttacher.Init();
            armature = boneAttacher.armature;
            armatureRot = armature.localEulerAngles;
            PrepFins();
        }

        public void Update()
        {
            if (tailBone != null && animateTail)
            {
                TailMove(speed);
            }
            if (jawTransform != null && animateJaw)
            {
                jawTime += jawOpenFrequency * Time.deltaTime;
                MouthOpen(Mathf.Sin(jawTime), speed);
            }
            if (animateFins)
            {
                FinsMove(speed);
            }
        }

        private void PrepFins()
        {
            for (int i = 0; i < finBones.Count; i++)
            {
                finAngles.Add(finBones[i].localEulerAngles);
            }
        }
        private void FinsMove(float sp = 1)
        {
            finTime += finMoveFrequency * sp * Time.deltaTime;
            Vector3 v = new Vector3();

            for (int i = 0; i < finBones.Count; i++)
            {
                v.x = finAngles[i].x + finRotations.x * Mathf.Sin(finTime);
                v.y = finAngles[i].y + finRotations.y * Mathf.Sin(finTime);
                v.z = finAngles[i].z + finRotations.z * Mathf.Sin(finTime);
                finBones[i].localRotation = Quaternion.Euler(v);
            }

        }

        private static float CubicEaseOut(float t, float b, float c, float d)
        {
            return c * ((t = t / d - 1) * t * t + 1) + b;
        }

        public void Turn(int dir, float duration)
        {
            if (dir != -1 && dir != 1)
            {
                return;
            }
            if (turnTween == null)
            {
                turnTween = StartCoroutine(TurnCo(dir, duration));
            }
        }

        private IEnumerator TurnCo(int dir, float duration)
        {
            var t = 0f;
            while (t <= duration * 0.5f)
            {
                tailOverride = Mathf.Lerp(prevTail, tailRangeTurn * dir,
                    CubicEaseOut(t, 0, 1, duration));
                t += Time.deltaTime;
                yield return null;
            }
            t = 0f;
            while (t <= duration * 0.5f)
            {
                tailOverride = Mathf.Lerp(tailRangeTurn * dir, prevTail,
                    CubicEaseOut(t, 0, 1, duration));
                t += Time.deltaTime;
                yield return null;
            }
            turnTween = null;
        }

        private void TailMove(float sp = 1)
        {
            // Override when turning
            if (turnTween != null)
            {
                tailBone.localEulerAngles = new Vector3(0, 0, tailOverride);
                armature.localEulerAngles = armatureRot + new Vector3(0, 0, -tailOverride * 0.25f);
                return;
            }

            float cyclePct = Mathf.Sin(tailTime);
            float a = cyclePct * tailRange;
            prevTail = a;

            tailTime += tailSpeed * sp * Time.deltaTime;

            float angleChange = Mathf.DeltaAngle(tailBone.localEulerAngles.z, a);

            tailBone.localEulerAngles = new Vector3(0, 0, a);
            armature.localEulerAngles = armatureRot + new Vector3(0, 0, -a * 0.25f);
        }

        private void MouthOpen(float pct = 1, float jawSpeed = 1)
        {
            jawTgt = Mathf.LerpAngle(jawCloseAngle, jawOpenAngle, pct);

            jawOpen = Mathf.LerpAngle(jawTransform.localEulerAngles.x, jawTgt, jawSpeed);

            Vector3 curAngles = jawTransform.localEulerAngles;
            curAngles.x = jawOpen;

            jawTransform.localEulerAngles = curAngles;
        }

    }
}