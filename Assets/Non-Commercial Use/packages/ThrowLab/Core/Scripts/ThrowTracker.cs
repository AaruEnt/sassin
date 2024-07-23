using CloudFine.ThrowLab.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CloudFine.ThrowLab
{
    public class ThrowTracker : MonoBehaviour
    {
        public LineRenderer _trajectoryLine;
        public ParticleSystem _sampleParticleSystem;
        private ParticleSystemRenderer _particleRenderer;
        public UIThrowTracker _trackerUIPrefab;
        public GameObject _collisionMarkerPrefab;

        private Vector3 _origin;
        private Vector3 _releaseVelocity;

        private ThrowHandle _handle;
        private Rigidbody _rigidbody;        

        private Vector3 _groundHitPoint;
        private bool _tracking = false;
        private List<Vector3> _positions = new List<Vector3>();
        private const int _positionsCap = 500;

        private UIThrowTracker _ui;

        private bool _show = true;
        private bool _showLine;
        private bool _showSamples;

        private Color _color = Color.white;

        private Vector3 particleMaxSize = new Vector3(.1f, .1f, .2f);
        private List<ThrowHandle.VelocitySample> visSamples;
        private float[] visWeights;
        private ParticleSystem.Particle[] _smoothingSampleSet;
        private List<ParticleSystem.Particle> _postReleaseSampleSet = new List<ParticleSystem.Particle>();


        public float GroundDistance
        {
            get
            {
                Vector2 posFlat = new Vector2(_handle.transform.position.x, _handle.transform.position.z);
                Vector2 originFlat = new Vector2(_origin.x, _origin.z);
                return Vector2.Distance(posFlat, originFlat);
            }
        }

        private void Awake()
        {
            _particleRenderer = _sampleParticleSystem.GetComponent<ParticleSystemRenderer>();
        }


        private void Update()
        {
            if (_tracking)
            {
                if (_positions.Count < _positionsCap)
                {
                    _positions.Add(_rigidbody.position);
                    if (_trajectoryLine)
                    {
                        _trajectoryLine.positionCount = _positions.Count;
                        _trajectoryLine.SetPositions(_positions.ToArray());
                    }
                }

                if (_ui)
                {
                    _ui.UpdateDistance(GroundDistance);
                }
            }
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        public void TrackThrowable(ThrowHandle throwable)
        {
            _handle = throwable;

            _handle.onDetachFromHand+=(OnDetach);
            _handle.onPickUp+=(OnAttach);
            _handle.OnSampleRecorded += VisualizeSmoothingSample;
            _handle.onFrictionApplied += VisualizeEstimatedVelocity;
            _handle.onFinalTrajectory+= (RecordFinalTrajectory);
            _rigidbody = _handle.GetComponentInChildren<Rigidbody>();
            CollisionListener listener = _rigidbody.gameObject.AddComponent<CollisionListener>();
            listener.CollisionEnter += OnCollisionEnter;
            throwable.OnDestroyHandle += OnHandleDestroyed;
            CreateOutline(throwable.gameObject);
        }

        public void SetLineAppearance(Texture lineTex, Color lineColor)
        {
            _trajectoryLine.material.mainTexture = lineTex;
            _trajectoryLine.material.color = lineColor;
            _trajectoryLine.useWorldSpace = true;
        }

        public void ShowHideLine(bool show)
        {
            _showLine = show;
            _trajectoryLine.enabled = _show && _showLine;

        }

        public void ShowHideSamples(bool show)
        {
            _showSamples = show;
            if (_particleRenderer)
            {
                _particleRenderer.enabled = (show && _showSamples);

            }

        }

        public void SetColor(Color color)
        {
            _color = color;
        }

        public void AttachUIToRoot(Transform root)
        {
            _ui = GameObject.Instantiate(_trackerUIPrefab, root);
            _ui.transform.SetAsLastSibling();
            _ui.gameObject.SetActive(false);
            _ui.clearButton.onClick.AddListener(Cleanup);
            _ui.showHideButton.onClick.AddListener(ToggleVisible);

            foreach(Graphic text in _ui.GetComponentsInChildren<Graphic>())
            {
                if (text.GetComponent<UIColorMeTag>() != null)
                {
                    text.color = _color;
                }
            }
        }


        public void OnDetach()
        {
            _releaseVelocity = _rigidbody.velocity;
            _origin = _rigidbody.position;
            if (_trajectoryLine)
            {
                _trajectoryLine.useWorldSpace = true;
            }
            _positions.Clear();
            _positions.Add(_rigidbody.position);
            _tracking = true;
            if (_ui)
            {
                _ui.gameObject.SetActive(true);
                _ui.SetSpeed(_releaseVelocity.magnitude);
                float pitch = Mathf.Asin(_releaseVelocity.normalized.y) * Mathf.Rad2Deg;
                if (_releaseVelocity.magnitude < .1f) pitch = 0;
                _ui.SetAngle(pitch);
            }

            _postReleaseSampleSet.Clear();
            VisualizeEstimatedVelocity();
            RefreshParticles();

            ShowHideOutline(true);
        }

        public void EndTracking()
        {
            _tracking = false;
        }

        public void OnAttach(GameObject hand, GameObject collisionRoot)
        {
            EndTracking();
        }

        private void RecordFinalTrajectory(Vector3 launch)
        {
            if (_ui)
            {
                _ui.SetSpeed(launch.magnitude);
                float pitch = Mathf.Asin(launch.normalized.y) * Mathf.Rad2Deg;
                if (launch.magnitude < .1f) pitch = 0;
                _ui.SetAngle(pitch);
            }
        }

        //creates a white cone particle
        private void VisualizeSmoothingSample(ThrowHandle.VelocitySample sample)
        {
            if (_handle.Settings.smoothingEnabled)
            {
                if (_handle._attached)
                {
                    visSamples = _handle.GetSampleWeights(out visWeights);
                    VisualizeVelocitySmoothingData(visSamples, visWeights);
                }
                RefreshParticles();
            }
        }

        //creates a colored cone particle
        private void VisualizeEstimatedVelocity()
        {
            float alpha = _handle.GetHandFriction();
            if (alpha <= 0) return;

            ParticleSystem.Particle newParticle = CreateParticleForCurrentEstimatedVelocity();

            Color color = _color;
            color.a = alpha;
            newParticle.startColor = color;
            _postReleaseSampleSet.Add(newParticle);
        }


        private void RefreshParticles()
        {
            List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();
            if (_smoothingSampleSet != null)
            {
                particles.AddRange(_smoothingSampleSet); //add all particles that were present on release
            }
            particles.AddRange(_postReleaseSampleSet);

            if (_sampleParticleSystem)
            {
                _sampleParticleSystem.SetParticles(particles.ToArray(), particles.Count);
            }
        }

        private ParticleSystem.Particle CreateParticleForCurrentEstimatedVelocity()
        {
            Vector3 position = _handle.GetSampleSource().position;
            Vector3 velocity = _handle.GetVelocityEstimate();
            Quaternion rotation = Quaternion.identity;
            if (velocity.magnitude > 0)
            {
                rotation = Quaternion.LookRotation(velocity);
            }

            ParticleSystem.Particle newParticle = new ParticleSystem.Particle();

            Color color = _color;
            color.a = 1;

            newParticle.position = position;
            newParticle.rotation3D = rotation.eulerAngles;
            newParticle.startSize3D = particleMaxSize;
            newParticle.startColor = color;

            return newParticle;
        }

        private void VisualizeVelocitySmoothingData(List<ThrowHandle.VelocitySample> samples, float[] weights)
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[samples.Count];
            Vector3 look = Vector3.zero;

            
            for(int i=0; i<particles.Length; i++)
            {
                particles[i].position = samples[i].position;
                
                if (samples[i].velocity.magnitude < 0.1f)
                {
                    particles[i].startSize3D = Vector3.zero;
                }
                else
                {
                    particles[i].startSize3D = particleMaxSize * weights[i];
                    look = Quaternion.LookRotation(samples[i].velocity).eulerAngles;
                }

                particles[i].rotation3D = look;
                Color color = Color.white;
                color.a = .3f;
                particles[i].startColor = color;
            }
            _smoothingSampleSet = particles;
        }


        public void ToggleVisible()
        {
            _show = !_show;
            ShowHide(_show);
        }



        public void ShowHide(bool show)
        {
            _show = show;
            _trajectoryLine.enabled = show && _showLine;
            if (_particleRenderer)
            {
                _particleRenderer.enabled = (show && _showSamples);
            }

            if (_ui)
            {
                _ui.RefreshVisibilityButton(_show);
            }

            if (_collisionMarker)
            {
                _collisionMarker.SetActive(show);
            }

        }


        void OnHandleDestroyed(ThrowHandle handle)
        {
            Cleanup();
        }

        public void Cleanup()
        {
            if (_handle)
            {
                GameObject.Destroy(_handle.gameObject);
            }
            if (_ui)
            {
                GameObject.Destroy(_ui.gameObject);
            }

            if (this)
            {
                GameObject.Destroy(this.gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_tracking)
            {
                if (Time.time-_handle._timeOfRelease>.1f)
                {
                    if (collision.contacts.Length > 0)
                    {
                        ContactPoint point = collision.contacts[0];
                        PlaceCollisionMarker(point.point, point.normal);
                    }
                    EndTracking();
                }
            }
        }

        private GameObject _collisionMarker;
        private void PlaceCollisionMarker(Vector3 position, Vector3 normal)
        {
            if (_collisionMarker == null)
            {
                _collisionMarker = GameObject.Instantiate(_collisionMarkerPrefab, position, Quaternion.LookRotation(normal), this.transform);
                MeshRenderer render = _collisionMarker.GetComponentInChildren<MeshRenderer>();
                if (render)
                {
                    if (render.material)
                    {
                        render.material.color = _color;
                    }
                }
            }

            _collisionMarker.transform.position = position;
            _collisionMarker.transform.rotation = Quaternion.LookRotation(normal);
            _collisionMarker.SetActive(_show);




        }

        private List<MeshRenderer> _outlineRenderers = new List<MeshRenderer>();
        private void CreateOutline(GameObject original)
        {
            _outlineRenderers = new List<MeshRenderer>();
            foreach(MeshFilter renderer in original.GetComponentsInChildren<MeshFilter>())
            {
                GameObject outlineObj = new GameObject("_Outline");
                outlineObj.transform.SetParent(renderer.transform);
                outlineObj.transform.localPosition = Vector3.zero;
                outlineObj.transform.localRotation = Quaternion.identity;
                outlineObj.transform.localScale = Vector3.one;

                MeshRenderer outline = outlineObj.AddComponent<MeshRenderer>();
                MeshFilter filter = outlineObj.AddComponent<MeshFilter>();
                filter.mesh = renderer.mesh;
                outline.material = new Material(Shader.Find("Custom/OutlineOnly"));
                outline.material.color = _color;
                outline.enabled = false;
                _outlineRenderers.Add(outline);

            }

        }

        private void ShowHideOutline(bool show)
        {
            foreach(MeshRenderer renderer in _outlineRenderers)
            {
                renderer.enabled = show;
            }
        }
    }
}