using System.Collections;
using UnityEngine;

namespace Cosmocat.InstantFish
{
    public class Fish : MonoBehaviour
    {
        [Header("References")]
        public Transform eyes;
        public GameObject mesh;
        public Renderer eyeRenderer;
        public Renderer bodyRenderer;
        public SkinnedMeshRenderer headRenderer;
        public Renderer jawRenderer;
        public FishFin[] pectoralFins;
        public FishFin[] dorsalFins;
        public FishFin tailFin;

        [Header("Behaviour")]
        public float speed = 2f;
        public float verticalRange = 4.5f;
        public bool move = true;

        [Header("Appearance")]
        public FishProfile profile;

        /// <summary>
        /// Should fish cast shadows?
        /// </summary>
        public bool castShadows;

        /// <summary>
        /// Should fish recieve shadows?
        /// </summary>
        public bool receiveShadows;

        private FishProfile lastProfile;

        [Header("Debug")]
        [ReadOnly] public Vector3 randomBias;
        [ReadOnly] public Vector3 biasOverride;
        [ReadOnly] public string debug;

        private Vector3 lastBias;
        private Vector3 biasVel;
        private Vector3 turnBias;
        private new Rigidbody rigidbody;
        private Vector3 startPos;
        private Coroutine tween;
        private Coroutine recoveryTween;
        private MeshRenderer bakedHead;
        private WaterLevel waterLevel;
        private FishAnimator animator;

        private float WaterLevel
        {
            get
            {
                if (waterLevel)
                {
                    return waterLevel.transform.position.y;
                }
                else
                {
                    return 0f;
                }
            }
        }

        void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            waterLevel = FindObjectOfType<WaterLevel>();
            animator = GetComponent<FishAnimator>();
            startPos = transform.position;
            InitShadows();
            RefreshProfile();
            BakeHead();

        }

        private void BakeHead()
        {
            // Bake head mesh renderer (since blend shapes are not modified at runtime)
            var ho = headRenderer.gameObject;
            var mr = ho.AddComponent<MeshRenderer>();
            var mf = ho.AddComponent<MeshFilter>();
            headRenderer.BakeMesh(mf.mesh);
            mf.mesh.RecalculateBounds();
            mr.material = headRenderer.sharedMaterial;
            mr.receiveShadows = headRenderer.receiveShadows;
            mr.shadowCastingMode = headRenderer.shadowCastingMode;
            Destroy(headRenderer);

            bakedHead = mr;

        }

        public void RefreshProfile()
        {
            // Initialize eye material
            var eyeMat = profile.eyeMats[Random.Range(0, profile.eyeMats.Length)];
            eyeRenderer.material = eyeMat;

            // Initialize body and head material
            var bodyMat = profile.bodyMats[Random.Range(0, profile.bodyMats.Length)];
            bodyRenderer.material = bodyMat;

            Renderer hr = bakedHead ? bakedHead : (Renderer)headRenderer;
            if (profile.useHeadMat)
            {
                hr.material = profile.headMats[
                    Random.Range(0, profile.headMats.Length)];
            }
            else
            {
                hr.material = bodyMat;
            }

            // Initialize fin materials and meshes
            var finMat = profile.finMats[Random.Range(0, profile.finMats.Length)];
            foreach (FishFin fin in pectoralFins)
            {
                fin.Refresh();
                fin.meshFilter.mesh = profile.pectoralFinMeshs[
                    Random.Range(0, profile.pectoralFinMeshs.Length)];
                fin.meshRenderer.material = finMat;
            }
            foreach (FishFin fin in dorsalFins)
            {
                fin.Refresh();
                fin.meshFilter.mesh = profile.dorsalFinMeshes[
                    Random.Range(0, profile.dorsalFinMeshes.Length)];
                fin.meshRenderer.material = finMat;

            }

            tailFin.Refresh();
            tailFin.meshFilter.mesh = profile.tailFinMeshes[
                    Random.Range(0, profile.tailFinMeshes.Length)];
            if (profile.useTailFinMat)
            {
                tailFin.meshRenderer.material = profile.tailFinMats[
                    Random.Range(0, profile.tailFinMats.Length)];
            }
            else
            {
                tailFin.meshRenderer.material = finMat;
            }

            if (profile.useJawMats)
            {
                jawRenderer.material = profile.jawMats[
                    Random.Range(0, profile.jawMats.Length)];
            }
            else
            {
                jawRenderer.material = finMat;
            }

            lastProfile = profile;
        }

        public void InitShadows()
        {
            if (!castShadows || !receiveShadows)
            {
                foreach (Renderer r in GetComponentsInChildren<Renderer>())
                {
                    r.receiveShadows = receiveShadows;
                    if (!castShadows)
                    {
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    }
                }
            }
        }

        void Start()
        {
            if (move)
            {
                StartCoroutine(UpdateBias());
            }
        }

        void Update()
        {
            if (profile != lastProfile)
            {
                RefreshProfile();
            }

            if (!move)
            {
                return;
            }

            if (IsTweening)
            {
                return;
            }

            RaycastHit hitUp, hitDown, hitForward;
            bool didHitUp = Physics.Raycast(eyes.transform.position + (transform.forward * 0.5f), transform.up, out hitUp, 0.5f);
            bool didHitDown = Physics.Raycast(eyes.transform.position + (transform.forward * 0.5f), -transform.up, out hitDown, 0.5f);
            bool didHitForward = Physics.Raycast(eyes.transform.position, transform.forward, out hitForward, 1f);

            if (!didHitForward)
            {
                if (didHitUp && !didHitDown)
                {
                    // Push down
                    biasOverride = new Vector3(randomBias.x, -0.5f, 0f);
                }

                if (didHitDown && !didHitUp)
                {
                    debug = hitDown.collider.name;
                    // Push up
                    biasOverride = new Vector3(randomBias.x, 0.5f, 0f);
                }
            }
            else
            {
                if (turnBias == Vector3.left)
                {
                    tween = StartCoroutine(RotateTowardsCo(-transform.right, 0.5f));
                }
                else
                {
                    tween = StartCoroutine(RotateTowardsCo(transform.right, 0.5f));
                }
            }
        }

        private void FixedUpdate()
        {
            if (!move)
            {
                return;
            }

            if (transform.position.y > WaterLevel)
            {
                rigidbody.AddForce(Physics.gravity);
            }

            Vector3 biasVect;
            if (biasOverride != Vector3.zero)
            {
                biasVect = Vector3.SmoothDamp(lastBias, biasOverride, ref biasVel, 1f);
                lastBias = biasVect;
            }
            else
            {
                biasVect = Vector3.SmoothDamp(lastBias, randomBias, ref biasVel, 1f);
                lastBias = biasVect;
            }
            Vector3 target, forward;

            float xBias = biasVect.x;
            biasVect.x = 0f;
            forward = (transform.forward + (transform.right * xBias) + biasVect).normalized;
            target = transform.position + forward * speed * Time.fixedDeltaTime;

            // FIXME causes fish to slow down near water level
            target.y = Mathf.Clamp(target.y, -Mathf.Infinity, WaterLevel);

            if (!IsTweening)
            {
                rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(
                    new Vector3(forward.x, 0f, forward.z)), Time.fixedDeltaTime * 180f));
            }
            rigidbody.MovePosition(target);
            biasOverride = Vector3.zero;
        }

        private bool IsTweening
        {
            get
            {
                if (tween != null)
                {
                    return true;
                }
                if (recoveryTween != null)
                {
                    return true;
                }
                return false;
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!move)
            {
                return;
            }

            debug = "OnCollisionStay";
            if (tween != null)
            {
                StopCoroutine(tween);
                tween = null;
            }
            if (recoveryTween != null)
            {
                return;
            }

            if (turnBias == Vector3.left)
            {
                recoveryTween = StartCoroutine(RotateTowardsCo(-transform.right, 0.5f, true));
            }
            else
            {
                recoveryTween = StartCoroutine(RotateTowardsCo(transform.right, 0.5f, true));
            }
        }

        IEnumerator UpdateBias()
        {
            while (true)
            {
                randomBias = new Vector3(Random.Range(-0.025f, 0.025f), Random.Range(-0.5f, 0.5f), 0f);
                if (Random.Range(0, 2) == 0)
                {
                    turnBias = Vector2.left;
                }
                else
                {
                    turnBias = Vector2.right;
                }
                yield return new WaitForSeconds(Random.Range(1f, 2f));
            }
        }

        private static float CubicEaseOut(float t, float b, float c, float d)
        {
            return c * ((t = t / d - 1) * t * t + 1) + b;
        }

        private IEnumerator RotateTowardsCo(Vector3 dir, float duration, bool recovery = false)
        {
            var targetRot = Quaternion.LookRotation(dir);
            var startRot = rigidbody.rotation;
            var t = 0f;

            if (dir == -transform.right)
            {                
                animator?.Turn(-1, duration);
            }
            else if (dir == transform.right)
            {                
                animator?.Turn(1, duration);
            }

            while (t <= duration)
            {
                rigidbody.MoveRotation(Quaternion.Lerp(startRot, targetRot,
                    CubicEaseOut(t, 0, 1, duration)));
                t += Time.deltaTime;
                yield return null;
            }
            rigidbody.MoveRotation(targetRot);
            if (recovery)
            {
                recoveryTween = null;
            }
            else
            {
                tween = null;
            }
        }

    }
}