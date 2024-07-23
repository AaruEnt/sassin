using CloudFine.ThrowLab.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CloudFine.ThrowLab
{
    public class LabManager : MonoBehaviour
    {
        [Header("Spawn")]
        public List<ThrowHandle> _throwablePrefabs;
        public Transform _spawnPoint;
        public ParticleSystem _spawnEffect;
        private ThrowHandle _throwablePrefab;
        public ThrowTracker _trackerPrefab;

        [Header("UI")]
        public UIThrowConfiguration _configurationUI;
        public DeviceDetectorUI _deviceDetector;
        public RectTransform _trackerUIListRoot;
        public Text _throwableLabel;

        [Header("Lines")]
        public Texture2D[] _lineTextures;
        public Color[] _lineColors = new Color[]
        {
            Color.cyan,
            Color.magenta,
            Color.yellow
        };

        [Header("Variants")]
        public GameObject variantPanelRoot;
        public Image[] tabFills;
        public Button variantResetButton;
        public Button variantSaveButton;
        public GameObject warningNoConfigs;
        public Toggle variantEnabledToggle;
        public Toggle variantLineEnabledToggle;
        public Toggle variantSamplesEnabledToggle;

        private int _throwableIndex = -1;
        private Device _device = Device.UNSPECIFIED;
        private List<ThrowTracker> _trackers = new List<ThrowTracker>();
        private Dictionary<ThrowConfiguration, ThrowConfiguration[]> _tempConfigVariants = new Dictionary<ThrowConfiguration, ThrowConfiguration[]>();

        private ThrowHandle _currentSpawn;

        private int currentConfigIndex;
        private ThrowConfiguration[] configSet;
        private ThrowConfiguration original;
        private Color[] colorSet = new Color[]
        {
            Color.cyan,
            Color.magenta,
            Color.yellow,
        };
        private bool[] configEnabled = new bool[3]        {
            true,false,false
        };
        private bool[] showSamples = new bool[3]
        {
            true,false,false
        };
        private bool[] showLine = new bool[3]
        {
            true,true,true
        };

        private void Awake()
        {
            _deviceDetector.OnDeviceDetected += SetDevice; 
        }

        private void Start()
        {
            if (_throwablePrefabs.Count > 0)
            {
                SelectThrowable(0);
            }
            else
            {
                if (_throwableLabel)
                {
                    _throwableLabel.text = "(none)";

                }
            }

            for(int i=0; i<configEnabled.Length; i++)
            {
                SetConfigEnabled(i, configEnabled[i]);
            }
        }

        private void Update()
        {
            if (_currentSpawn) {
                if (Vector3.Distance(_currentSpawn.transform.position, _spawnPoint.position) > .3f)
                {
                    SpawnTrackedThrowable();
                }
            }
            else
            {
                SpawnTrackedThrowable();
            }
        }


        private void SetDevice(Device device)
        {
            _device = device;
            SelectThrowable(_throwableIndex);
        }

        public void SpawnTrackedThrowable()
        {
            if (_throwablePrefab)
            {
                List<ThrowHandle> throwableSet = new List<ThrowHandle>();
                ThrowHandle primaryHandle = null;

                ThrowConfiguration[] configVariants = _tempConfigVariants[_throwablePrefab.GetConfigForDevice(_device)];


                for (int i=0; i<3; i++)
                {
                    if (!configEnabled[i]) continue;

                   
                    ThrowHandle handle = GameObject.Instantiate(_throwablePrefab);
                    throwableSet.Add(handle);

                    ThrowTracker tracker = GameObject.Instantiate(_trackerPrefab) as ThrowTracker;

                    handle.SetConfigForDevice(_device, configVariants[i]);

                    if (primaryHandle == null)
                    {
                        primaryHandle = handle;
                        primaryHandle.transform.position = _spawnPoint.position;
                    }
                    else
                    {
                        handle.transform.SetParent(primaryHandle.transform);
                        handle.transform.localPosition = Vector3.zero;
                        handle.transform.localRotation = Quaternion.identity;
                        handle.name = handle.name + "_" + i;

                        handle.SetPhysicsEnabled(false);

                        primaryHandle.onDetachFromHand+=(handle.OnDetach);
                        primaryHandle.onPickUp+=(handle.OnAttach);
                        
                    }

                    tracker.SetColor(colorSet[i]);
                    tracker.TrackThrowable(handle);
                    tracker.SetLineAppearance(_lineTextures[i], _lineColors[i]);
                    tracker.ShowHideLine(showLine[i]);
                    tracker.ShowHideSamples(showSamples[i]);
                    tracker.AttachUIToRoot(_trackerUIListRoot);
                    _trackers.Add(tracker);

                }

                //Each of these throwables will have a rigidbody, so make sure they will ignore eachother.
                for(int i =0; i<throwableSet.Count; i++)
                {
                    for(int j =0; j<throwableSet.Count; j++)
                    {
                        if (i == j) continue;
                        if (throwableSet[i] == null || throwableSet[j] == null) continue;

                        throwableSet[i].IgnoreCollisionWithOther(throwableSet[j].gameObject, true);
                    }
                }     



                if (_spawnEffect)
                {
                    _spawnEffect.Play();
                }

                _currentSpawn = primaryHandle;
            }
        }

        void RespawnThrowable()
        {
            if(_currentSpawn != null)
            {
                GameObject.Destroy(_currentSpawn.gameObject);
            }
            SpawnTrackedThrowable();
        }

        public void SetCurrentConfigEnabled(bool enabled)
        {
            SetConfigEnabled(currentConfigIndex, enabled);
            variantLineEnabledToggle.interactable = enabled;
            variantSamplesEnabledToggle.interactable = enabled;
            ReloadCurrentConfig();
            RespawnThrowable();
        }

        public void SetConfigEnabled(int i, bool enabled)
        {
            configEnabled[i] = enabled;
            tabFills[i].enabled = enabled;

            bool activeConfig = false;
            for (int j = 0; j < configEnabled.Length; j++)
            {
                activeConfig = activeConfig || configEnabled[j];
            }

            warningNoConfigs.SetActive(!activeConfig);
        }

        public void SetCurrentLineEnabled(bool enabled)
        {
            SetLineEnabled(currentConfigIndex, enabled);
        }

        public void SetLineEnabled(int i, bool enabled)
        {
            showLine[i] = enabled;
        }

        public void SetCurrentSampleVisEnabled(bool enabled)
        {
            SetSampleVisualizationEnabled(currentConfigIndex, enabled);
        }

        public void SetSampleVisualizationEnabled(int i, bool enabled)
        {
            showSamples[i] = enabled;
        }

        public void SaveCurrentConfig()
        {
            configSet[currentConfigIndex].CopyTo(original);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(original);
#else
            original.SaveToJSON();
#endif
        }

        public void ResetCurrentConfig()
        {
            original.CopyTo(configSet[currentConfigIndex]);
            LoadConfig(currentConfigIndex);
        }

        public void ClearAll()
        {
            foreach (ThrowTracker tracker in _trackers)
            {
                tracker.Cleanup();
            }
            _trackers.Clear();
        }

        public void Reset()
        {
            SpawnTrackedThrowable();
        }

        public void CycleThrowableRight()
        {
            if (_throwableIndex < 0) return;
            _throwableIndex++;
            if (_throwableIndex >= _throwablePrefabs.Count)
            {
                _throwableIndex = 0;
            }
            SelectThrowable(_throwableIndex);
        }

        public void CycleThrowableLeft()
        {
            if (_throwableIndex < 0) return;
            _throwableIndex--;
            if (_throwableIndex < 0)
            {
                _throwableIndex = _throwablePrefabs.Count - 1;
            }
            SelectThrowable(_throwableIndex);
        }

        void SelectThrowable(int i)
        {
            if (i < 0 || i >= _throwablePrefabs.Count) return;

            _throwableIndex = i;
            _throwablePrefab = _throwablePrefabs[i];

            original = _throwablePrefab.GetConfigForDevice(_device);
            ThrowConfiguration[] variants;
            if (_tempConfigVariants.ContainsKey(original))
            {
                variants = _tempConfigVariants[original];
            }
            else
            {
                variants = new ThrowConfiguration[3]
                {
                    original.Clone(),
                    original.Clone(),
                    original.Clone()
                };
                variants[0].name = original.name + " A";
                variants[1].name = original.name + " B";
                variants[2].name = original.name + " C";

                _tempConfigVariants.Add(original, variants);
            }
            configSet = variants;
            LoadConfig(currentConfigIndex);

            if (_throwableLabel)
            {
                _throwableLabel.text = _throwablePrefab.name;

            }
            RespawnThrowable();
        }

        public void LoadConfig(int i)
        {
            currentConfigIndex = i;
            ReloadCurrentConfig();
            variantEnabledToggle.isOn = configEnabled[i];
            SetCurrentConfigEnabled(configEnabled[i]);
            variantLineEnabledToggle.isOn = showLine[i];
            SetCurrentLineEnabled(showLine[i]);
            variantSamplesEnabledToggle.isOn = (showSamples[i]);
            SetCurrentSampleVisEnabled(showSamples[i]);
        }

        public void ReloadCurrentConfig()
        {
            _configurationUI.LoadConfig(configSet[currentConfigIndex], colorSet[currentConfigIndex], configEnabled[currentConfigIndex]);

        }

    }
}