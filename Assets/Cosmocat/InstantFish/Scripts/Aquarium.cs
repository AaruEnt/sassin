using System;
using System.Collections;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using UnityEngine.SceneManagement;

namespace Cosmocat.InstantFish
{
    public class Aquarium : MonoBehaviour
    {
        [Header("References")]
        public FishSpawner spawner;
        public TMP_Text dayNightText;
        public MeshRenderer[] waterRenderers;
        public CanvasGroup canvasGroup;

        [Header("Day Settings")]
        public Material dayWaterMat;
        public Material daySkyboxMat;
#if UNITY_POST_PROCESSING_STACK_V2
        public PostProcessProfile dayProfile;
#endif
        public Color dayFogColor;

        [Header("Night Settings")]
        public Material nightWaterMat;
#if UNITY_POST_PROCESSING_STACK_V2
        public PostProcessProfile nightProfile;
#endif
        public Color nightFogColor;

        private bool night = true;

        void Start()
        {

        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (canvasGroup.interactable)
                {
                    canvasGroup.interactable = false;
                    canvasGroup.alpha = 0f;
                }
                else
                {
                    canvasGroup.interactable = true;
                    canvasGroup.alpha = 1f;
                }
            }
        }

        public void OnFishViewerClick()
        {
#if UNITY_EDITOR
            EditorSceneManager.LoadScene("FishViewer");
#else
            SceneManager.LoadScene("FishViewer");
#endif
        }

        private IEnumerator RespawnCo()
        {
            // Wait one frame for destroyed objects
            yield return null;
            spawner.Spawn();
        }

        public void OnDayNightClick()
        {
            night = !night;

            if (night)
            {
                dayNightText.text = "DAY";
                Camera.main.backgroundColor = nightFogColor;
                Camera.main.clearFlags = CameraClearFlags.Color;
                foreach (MeshRenderer water in waterRenderers)
                {
                    water.material = nightWaterMat;
                }
                RenderSettings.skybox = null;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                DynamicGI.UpdateEnvironment();
                RenderSettings.fogColor = nightFogColor;
                RenderSettings.fogDensity = 0.1f;
#if UNITY_POST_PROCESSING_STACK_V2
                Camera.main.GetComponent<PostProcessVolume>().profile = nightProfile;
#endif
            }
            else
            {
                dayNightText.text = "NIGHT";
                Camera.main.clearFlags = CameraClearFlags.Skybox;
                foreach (MeshRenderer water in waterRenderers)
                {
                    water.material = dayWaterMat;
                }
                RenderSettings.skybox = daySkyboxMat;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                DynamicGI.UpdateEnvironment();
                RenderSettings.fogColor = dayFogColor;
                RenderSettings.fogDensity = 0.05f;
#if UNITY_POST_PROCESSING_STACK_V2
                Camera.main.GetComponent<PostProcessVolume>().profile = dayProfile;
#endif
            }
        }

        public void OnRespawnClick()
        {
            foreach (Fish fish in FindObjectsOfType<Fish>())
            {
                Destroy(fish.gameObject);
            }
            StartCoroutine(RespawnCo());
        }

    }
}

