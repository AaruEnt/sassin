using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Autohand
{
    public class LineAnimation : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public GameObject lineReticle;
        public float activateTime = 0.5f;
        public bool useColorCurve = true;
        public AnimationCurve colorCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve widthCurve = AnimationCurve.Linear(0, 0, 1, 1); 

        float startWidth;
        float currentWidth;
        bool activated;
        float state;
        bool wasLineEnabled;

        Vector3 startReticleScale;
        Vector3 lastPosition;


        GradientAlphaKey[] startAlphaKeys;
        Coroutine animationCoroutine;

        private void Start() {
            startWidth = lineRenderer.widthMultiplier;
            lineRenderer.widthMultiplier = 0;

            wasLineEnabled = lineRenderer.enabled;
            if(lineReticle != null) {
                startReticleScale = lineReticle.transform.localScale;
                lineReticle.transform.localScale = Vector3.zero;
            }
        }

        private void OnDisable() {
            if(animationCoroutine != null)
                StopCoroutine(animationCoroutine);
            animationCoroutine = null;
            state = 0;
        }

        private void LateUpdate() {
            if(!lineRenderer.enabled && wasLineEnabled) {
                lineRenderer.enabled = true;
                wasLineEnabled = false;
                Deactivate();
            }

            else if(lineRenderer.enabled && !wasLineEnabled && lineRenderer.positionCount > 0) {
                //Need to do additional check to see if the line has moved just in case someone enables the line while its being disabled
                if(animationCoroutine != null && lastPosition != lineRenderer.GetPosition(lineRenderer.positionCount-1)) {
                    StopCoroutine(animationCoroutine);
                    animationCoroutine = null;
                }

                if(animationCoroutine == null) {
                    //Initializing this here instead of start because it needs to fill when the line is enabled
                    if(state == 0 && startAlphaKeys == null) {
                        startAlphaKeys = new GradientAlphaKey[lineRenderer.colorGradient.alphaKeys.Length];
                        lineRenderer.colorGradient.alphaKeys.CopyTo(startAlphaKeys, 0);
                    }
                    wasLineEnabled = true;
                    Activate();

                }
            }
            if(lineRenderer.positionCount > 0)
                lastPosition = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
            else
                lastPosition = Vector3.zero;
        }

        public void Activate() {
            activated = true;
            if(animationCoroutine == null)
                animationCoroutine = StartCoroutine(Animate());
        }

        public void Deactivate() {
            activated = false;
            if(animationCoroutine == null)
                animationCoroutine = StartCoroutine(Animate());
        }

         
        IEnumerator Animate() {
            while ((activated && state < 1) || (!activated && state > 0)) {
                state += Time.deltaTime / activateTime * (activated ? 1 : -1);
                state = Mathf.Clamp01(state);
                if(lineReticle != null)
                    lineReticle.transform.localScale = Vector3.Lerp(Vector3.zero, startReticleScale, state);
                lineRenderer.widthMultiplier = Mathf.Lerp(0, startWidth, state);

                if(useColorCurve)
                    for(int i = 0; i < startAlphaKeys.Length; i++) {
                        lineRenderer.colorGradient.alphaKeys[i] = new GradientAlphaKey(Mathf.Lerp(0, startAlphaKeys[i].alpha, colorCurve.Evaluate(state)), startAlphaKeys[i].time);
                    }
                yield return null;
            }

            lineRenderer.enabled = wasLineEnabled;
            animationCoroutine = null;
        }
    }
}
