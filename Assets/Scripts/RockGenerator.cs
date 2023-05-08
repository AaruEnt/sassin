using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RockTools
{
    public class RockGenerator : MonoBehaviour
    {
        #region Generator Settings

        private const int COUNT_MIN = 1;
        private const int COUNT_MAX = 150;
        private const float ROTATION_MIN = -45f;
        private const float ROTATION_MAX = 45;
        private const float MIN_SCALE_VISIBLE = 0.01f;
        private const float CHANGE_SENSITIVITY = 0.0001f;

        [Header("Appearance")] [SerializeField]
        private ERockType type;

        [SerializeField] private Material material;

        [Header("Distribution")] [Range(COUNT_MIN, COUNT_MAX)] [SerializeField]
        private int density = 120;

        [Range(1f, 5f)] [SerializeField] private float radius = 5f;
        [Range(-1f, 1f)] [SerializeField] private float asymmetry;
        [Range(0f, 1f)] [SerializeField] private float wave;
        [Range(0f, 1f)] [SerializeField] private float decentralize = 0.5f;

        [Header("Scale"), Range(0f, 2f)] [SerializeField]
        private float scaleLocal = 2f;

        [SerializeField] private AnimationCurve scaleByDistance = new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 2, 2));

        [Range(0f, 1f), Tooltip("Increase Height of the Rocks")] [SerializeField]
        private float tallness = 0.6f;

        [Range(0f, 1f)] [SerializeField] private float flatness;
        [Range(0f, 1f)] [SerializeField] private float wideness;

        [Header("Rotation")] [Range(ROTATION_MIN, ROTATION_MAX)] [SerializeField]
        private float rotation;

        [Range(0f, 359f)] [SerializeField] private float rotationLocal;
        [Range(0f, 1f)] [SerializeField] private float rotationRnd = 0.1f;

        #endregion

        #region Properties

        public ERockType pType
        {
            get => type;
            set
            {
                if (value != type)
                {
                    type = value;
                    UpdateMeshes();
                }
            }
        }

        public Material pMaterial
        {
            get => material;
            set
            {
                if (value != material)
                {
                    material = value;
                    UpdateMaterials();
                }
            }
        }

        public int pDensity
        {
            get => density;
            set
            {
                if (value != density)
                {
                    density = value;
                    UpdateDensities();
                }
            }
        }

        public float pRadius
        {
            get => radius;
            set
            {
                if (Math.Abs(value - radius) > CHANGE_SENSITIVITY)
                {
                    radius = value;
                    UpdateRadius();
                }
            }
        }

        public float pAsymmetry
        {
            get => asymmetry;
            set
            {
                if (Math.Abs(value - asymmetry) > CHANGE_SENSITIVITY)
                {
                    asymmetry = value;
                    UpdateScales();
                    UpdateRotations();
                }
            }
        }

        public float pWave
        {
            get => wave;
            set
            {
                if (Math.Abs(value - wave) > CHANGE_SENSITIVITY)
                {
                    wave = value;
                    UpdateScales();
                }
            }
        }

        public float pDecentralize
        {
            get => decentralize;
            set
            {
                if (Math.Abs(value - decentralize) > CHANGE_SENSITIVITY)
                {
                    decentralize = value;
                    UpdatePositions();
                    UpdateRotations();
                }
            }
        }

        public float pScaleLocal
        {
            get => scaleLocal;
            set
            {
                if (Math.Abs(value - scaleLocal) > CHANGE_SENSITIVITY)
                {
                    scaleLocal = value;
                    UpdateScales();
                }
            }
        }

        public AnimationCurve pScaleByDistance
        {
            get => scaleByDistance;
            set
            {
                if (!value.Equals(scaleByDistance))
                {
                    scaleByDistance = value;
                    UpdateScales();
                }
            }
        }

        public float pTallness
        {
            get => tallness;
            set
            {
                if (Math.Abs(value - tallness) > CHANGE_SENSITIVITY)
                {
                    tallness = value;
                    UpdateScales();
                }
            }
        }

        public float pFlatness
        {
            get => flatness;
            set
            {
                if (Math.Abs(value - flatness) > CHANGE_SENSITIVITY)
                {
                    flatness = value;
                    UpdateScales();
                }
            }
        }

        public float pWideness
        {
            get => wideness;
            set
            {
                if (Math.Abs(value - wideness) > CHANGE_SENSITIVITY)
                {
                    wideness = value;
                    UpdateScales();
                }
            }
        }

        public float pRotation
        {
            get => rotation;
            set
            {
                if (Math.Abs(value - rotation) > CHANGE_SENSITIVITY)
                {
                    rotation = value;
                    UpdateRotations();
                }
            }
        }

        public float pRotationLocal
        {
            get => rotationLocal;
            set
            {
                if (Math.Abs(value - rotationLocal) > CHANGE_SENSITIVITY)
                {
                    rotationLocal = value;
                    UpdateRotations();
                }
            }
        }

        public float pRotationRnd
        {
            get => rotationRnd;
            set
            {
                if (Math.Abs(value - rotationRnd) > CHANGE_SENSITIVITY)
                {
                    rotationRnd = value;
                    UpdateRotations();
                }
            }
        }

        #endregion

        public Transform pRoot => root;
        public List<MeshCollider> pColliders => meshColliders;

        [HideInInspector] public Mesh[] rockMeshesType1;
        [HideInInspector] public Mesh[] rockMeshesType2;
        [HideInInspector] public List<float> distances;
        [HideInInspector] public List<float> randomDs;
        [HideInInspector] public List<float> randomThetas;
        [HideInInspector] public List<float> randomScales;
        [HideInInspector] public List<Vector3> randomRotations;
        [HideInInspector] public List<bool> activeByDensity;
        [HideInInspector] public List<bool> activeByScale;
        [HideInInspector] public List<MeshCollider> meshColliders;
        [HideInInspector] public List<MeshRenderer> meshRenderers;
        [HideInInspector] public List<MeshFilter> meshFilters;
        [HideInInspector] public Transform root;
        [HideInInspector] public GameObject plane;

        private float largestScale = 2f;

        public static RockGenerator GetInstance()
        {
            return new GameObject("Rock Generator").AddComponent<RockGenerator>();
        }

        private void Reset()
        {
            Initialize();
        }

        public void Initialize()
        {
            transform.ClearChildren();

            // create the rocks root
            root = new GameObject("root").transform;
            root.SetParent(transform);
            root.localPosition = Vector3.zero;

            // create the plane
            plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.SetParent(transform);
            plane.transform.localPosition = Vector3.zero;

            // get default material
            var diffuse = plane.GetComponent<MeshRenderer>().sharedMaterial;
            material = diffuse;

            // get default rock meshes
            if (!LoadMeshes())
            {
                Debug.LogError("Meshes not found!");
                return;
            }

            Prepare();
            Randomize();
        }

        private void Clear()
        {
            root.ClearChildren();

            distances = new List<float>(COUNT_MAX);
            randomDs = new List<float>(COUNT_MAX);
            randomThetas = new List<float>(COUNT_MAX);
            randomScales = new List<float>(COUNT_MAX);
            randomRotations = new List<Vector3>(COUNT_MAX);
            activeByDensity = new List<bool>(COUNT_MAX);
            activeByScale = new List<bool>(COUNT_MAX);
            meshColliders = new List<MeshCollider>(COUNT_MAX);
            meshRenderers = new List<MeshRenderer>(COUNT_MAX);
            meshFilters = new List<MeshFilter>(COUNT_MAX);
        }

        public void Randomize()
        {
            for (var i = 0; i < COUNT_MAX; i++)
            {
                distances[i] = 0f;
                randomDs[i] = Random.value;
                randomThetas[i] = Random.value;
                randomScales[i] = Random.Range(-1f, 1f);
                randomRotations[i] = new Vector3(Random.value, Random.value, Random.value);
                activeByDensity[i] = true;
                activeByScale[i] = true;
            }

            UpdateAll();
        }

        private void Prepare()
        {
            Clear();

            for (var i = 0; i < COUNT_MAX; i++)
            {
                var mGameObject = new GameObject($"Rock-{i:000}");
                var mRenderer = mGameObject.AddComponent<MeshRenderer>();
                meshRenderers.Add(mRenderer);
                var mFilter = mGameObject.AddComponent<MeshFilter>();
                meshFilters.Add(mFilter);
                var mCollider = new GameObject($"{mGameObject}-collider").AddComponent<MeshCollider>();
                mCollider.convex = true;
                mCollider.transform.SetParent(mGameObject.transform);
                meshColliders.Add(mCollider);
                mGameObject.transform.position = root.position;
                mGameObject.transform.SetParent(root);
                distances.Add(0f);
                randomDs.Add(Random.value);
                randomThetas.Add(Random.value);
                randomScales.Add(Random.Range(-1f, 1f));
                randomRotations.Add(new Vector3(Random.value, Random.value, Random.value));
                activeByDensity.Add(true);
                activeByScale.Add(true);
            }
        }

        private bool LoadMeshes()
        {
            rockMeshesType1 = Resources.LoadAll<Mesh>("Meshes/Type01");
            rockMeshesType2 = Resources.LoadAll<Mesh>("Meshes/Type02");

            return rockMeshesType1.Length != 0 && rockMeshesType2.Length != 0;
        }

        public void UpdateAll()
        {
            UpdateMeshes();
            UpdatePositions();
            UpdateRotations();
            UpdateDensities();
            UpdateMaterials();
        }

        public void UpdateMeshes()
        {
            var meshes = type == ERockType.Cubic ? rockMeshesType1 : rockMeshesType2;
            for (var i = 0; i < meshFilters.Count; i++)
            {
                var mesh = meshes[i % meshes.Length];
                meshFilters[i].sharedMesh = mesh;
                meshColliders[i].sharedMesh = mesh;
            }

            RefreshColliders();
        }

        /// <summary>
        /// force mesh colliders to update their transforms, when rocks or their parent has a non-uniform scale. 
        /// </summary>
        private void RefreshColliders()
        {
            for (var i = 0; i < meshFilters.Count; i++)
            {
                meshColliders[i].sharedMesh = null;
                meshColliders[i].sharedMesh = meshFilters[i].sharedMesh;
            }
        }

        public void UpdatePositions()
        {
            for (var i = 0; i < COUNT_MAX; i++)
            {
                var pow = Mathf.Lerp(0.66f, 0.25f, decentralize);
                var d = Mathf.Pow(randomDs[i], pow);
                var r = d * radius;
                var theta = randomThetas[i] * 2 * Mathf.PI;
                var localPos = new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta));
                root.GetChild(i).localPosition = localPos;
                distances[i] = d;
            }

            UpdateScales();
        }

        public void UpdateRotations()
        {
            for (var i = 0; i < COUNT_MAX; i++)
            {
                var child = root.GetChild(i);
                var rot = new Vector3(0f, 0f, rotation);

                if (rotationRnd > 0f)
                {
                    var localCenter = new Vector3(asymmetry * radius, 0f, 0f);
                    var distance = (child.localPosition - localCenter).magnitude;
                    distance /= radius;

                    if (distance > 0.5f && rotationRnd > 0f)
                    {
                        var rndRot = rotationRnd * 360 * distance;
                        rot.x += randomRotations[i].x * rndRot;
                        rot.y += randomRotations[i].y * rndRot;
                        rot.z += randomRotations[i].z * rndRot;
                    }
                }

                child.rotation = Quaternion.Euler(rot + root.rotation.eulerAngles);
                child.RotateAround(child.position, child.up, rotationLocal);
            }

            RefreshColliders();
        }

        public void UpdateScales()
        {
            var tmpLargestScale = 0f;
            for (var i = 0; i < COUNT_MAX; i++)
            {
                var child = root.GetChild(i);

                var localCenter = new Vector3(asymmetry * radius, 0f, 0f);
                var distance = (child.localPosition - localCenter).magnitude;
                distance /= radius;
                var distCurve = scaleByDistance.Evaluate(1 - distance);
                var distCurveReverse = scaleByDistance.Evaluate(distance);
                var localScale = distCurve;
                localScale *= scaleLocal;

                var waveFactor = 0f;
                if (distance > 0.3f)
                {
                    var wavePosition = Mathf.Lerp(radius, -radius, wave);
                    var waveDist = Mathf.Abs(child.localPosition.x - wavePosition);
                    waveFactor = Mathf.InverseLerp(radius / 4f, 0, waveDist) * (distCurveReverse * 4);
                }

                localScale += localScale * waveFactor;

                if (localScale > tmpLargestScale)
                    tmpLargestScale = localScale;

                var _tallness = Mathf.Lerp(1f, 3f, distCurve * tallness);
                var _flatness = Mathf.Lerp(0f, 3f, distCurveReverse * flatness);
                var _wideness = Mathf.Lerp(0f, 3f, distCurveReverse * wideness);
                var height = Mathf.Clamp(_tallness - _flatness, 0.1f, 4f);
                var finalScale = new Vector3(1 + _wideness, height, 1 + _wideness) * localScale;
                child.localScale = finalScale;
                activeByScale[i] = localScale > MIN_SCALE_VISIBLE * largestScale;
                child.gameObject.SetActive(activeByScale[i] && activeByDensity[i]);
            }

            largestScale = tmpLargestScale;
        }

        public void UpdateRadius()
        {
            for (var i = 0; i < COUNT_MAX; i++)
            {
                var child = root.GetChild(i);
                var direction = child.localPosition.normalized;
                var newPos = direction * (distances[i] * radius);
                child.localPosition = newPos;
            }
        }

        public void UpdateDensities()
        {
            for (var i = 0; i < COUNT_MAX; i++)
            {
                activeByDensity[i] = i < density;
                root.GetChild(i).gameObject.SetActive(activeByScale[i] && activeByDensity[i]);
            }
        }

        public void UpdateMaterials()
        {
            for (var i = 0; i < meshRenderers.Count; i++)
                meshRenderers[i].sharedMaterial = material;
        }
    }
}