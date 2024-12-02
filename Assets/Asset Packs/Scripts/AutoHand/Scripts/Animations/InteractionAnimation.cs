using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Autohand {

    [System.Serializable]
    public struct AnimationTarget {
        public MeshRenderer renderer;
        public SpriteRenderer spriteRenderer;

        public bool ignoreColor;
        public bool ignorePosition;
        public bool ignoreScale;
        public bool ignoreRotation;

        public float colorDampener;
        public float positionDampener;
        public float scaleDampener;
        public float rotationDampener;
        public float waveOffset;

        private Vector3 startPosition;
        private Vector3 startRotation;
        private Vector3 startScale;
        [HideInInspector]
        public Transform transform;

        public Vector3 StartPosition => startPosition;
        public Vector3 StartRotation => startRotation;
        public Vector3 StartScale => startScale;

        public void SetStartValues() {
            if(renderer != null) {
                startPosition = renderer.transform.localPosition;
                startScale = renderer.transform.localScale;
                startRotation = renderer.transform.localEulerAngles;
                transform = renderer.transform;
            }
            else if(spriteRenderer != null) {
                startPosition = spriteRenderer.transform.localPosition;
                startScale = spriteRenderer.transform.localScale;
                startRotation = spriteRenderer.transform.localEulerAngles;
                transform = spriteRenderer.transform;
            }

        }

        public void SetColor(Color color) {
            if(renderer != null) {
                renderer.material.color = color;
            }
            else if(spriteRenderer != null) {
                spriteRenderer.color = color;
            }
        }
    }



    public class InteractionAnimations : MonoBehaviour {
        public AnimationTarget[] animationTargets;

        [Header("On Enable")]
        public bool onEnableTransition = false;
        public float onEnableTransitionTime = 0.15f;
        public AnimationCurve onEnableTransitionCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Color")]
        public bool updateColor = true;
        public float highlightAnimationColorTime = 0.5f;
        public float unhighlightAnimationColorTime = 0.5f;
        public Color unhighlightColor = Color.grey;
        public Color highlightColor = Color.white;
        public Color activateColor = Color.white;
        public AnimationCurve highlightColorCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float colorWaveFrequency = 1;
        public float colorWaveAmplitude = 0f;
        public float colorWaveOffset = 0.5f;

        [Header("Position")]
        public bool updatePosition = true;
        public float highlightAnimationPositionTime = 0.5f;
        public float unhighlightAnimationPositionTime = 0.5f;
        public Vector3 highlightPosition;
        public Vector3 activatePosition;
        public AnimationCurve positionAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float positionWaveFrequency = 1;
        public float positionWaveAmplitude = 0f;
        public float positionWaveOffset = 0.5f;

        [Header("Scale")]
        public bool updateScale = true;
        public float highlightAnimationScaleTime = 0.5f;
        public float unhighlightAnimationScaleTime = 0.5f;
        public float highlightScaleOffset = 0.1f;
        public float activateScaleOffset = 0.1f;
        public AnimationCurve scaleAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float scaleWaveFrequency = 1;
        public float scaleWaveAmplitude = 0f;
        public float scaleWaveOffset = 0.5f;

        [Header("Rotation")]
        public bool updateRotation = true;
        public float highlightAnimationRotationTime = 0.5f;
        public float unhighlightAnimationRotationTime = 0.5f;
        public Vector3 highlightRotationOffset = Vector3.zero;
        public Vector3 activateRotationOffset = Vector3.zero;
        public AnimationCurve rotationAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float rotationWaveFrequency = 1;
        public float rotationWaveAmplitude = 0f;
        public float rotationWaveOffset = 0.5f;

        protected float enableState;

        protected bool highlighting;
        protected float highlightStartTime;
        protected float highlightStopTime;
        protected float highlightColorState;
        protected float highlightPositionState;
        protected float highlightRotationState;
        protected float highlightScaleState;

        protected bool activating;
        protected float activateStartTime;
        protected float activateStopTime;
        protected float activateColorState;
        protected float activatePositionState;
        protected float activateRotationState;
        protected float activateScaleState;

        Color currentHighlightColor;
        Color currentUnhighlightColor;
        Color currentActivateColor;

        bool finishedAnimation;
        bool useWave;
        bool startedValueSet = false;

        Coroutine enableAnimationRountine;

        protected virtual void OnEnable() {
            if(!startedValueSet) {
                for(int i = 0; i < animationTargets.Length; i++)
                    animationTargets[i].SetStartValues();
                startedValueSet = true;
            }

            useWave = colorWaveAmplitude > 0 || positionWaveAmplitude > 0 || scaleWaveAmplitude > 0 || rotationWaveAmplitude > 0;

            if(!onEnableTransition) {
                currentActivateColor = activateColor;
                currentHighlightColor = highlightColor;
                currentUnhighlightColor = unhighlightColor;
            }

            SetAnimation();
        }

        protected virtual void OnDisable() {
            enableState = 0;
            highlightColorState = 0;
            highlightPositionState = 0;
            highlightScaleState = 0;
            highlightRotationState = 0;
            activateColorState = 0;
            activatePositionState = 0;
            activateScaleState = 0;
            activateRotationState = 0;
            activateScaleState = 0;
            highlighting = false;
            activating = false;
            SetAnimation();
        }


        protected virtual void LateUpdate() {
            if(!enabled)
                return;

            UpdateAnimationState();

            if(!finishedAnimation)
                SetAnimation();

            finishedAnimation = !highlighting && !activating &&
                (highlightPositionState == 0) &&
                (highlightColorState == 0) &&
                (highlightRotationState == 0) &&
                (highlightScaleState == 0) &&
                (activatePositionState == 0) &&
                (activateColorState == 0) &&
                (activateRotationState == 0) &&
                (activateScaleState == 0);

            if(onEnableTransition)
                finishedAnimation = finishedAnimation && enableState == 1;
        }


        [ContextMenu("HIGHLIGHT")]
        public void Highlight() {
            highlighting = true;
            highlightStartTime = Time.time;
        }

        [ContextMenu("UNHIGHLIGHT")]
        public void Unhighlight() {
            highlighting = false;
            highlightStopTime = Time.time;
        }

        [ContextMenu("ACTIVATE")]
        public void Activate() {
            activating = true;
            activateStartTime = Time.time;
        }

        [ContextMenu("DEACTIVATE")]
        public void Deactivate() {
            activating = false;
            activateStopTime = Time.time;
        }


        protected virtual void UpdateAnimationState() {
            var deltaTime = Time.deltaTime;
            var time = Time.time;
            var offseTime = time - deltaTime;


            //ENABLED TRANSITION
            if(onEnableTransition && enableState < 1) {
                enableState += deltaTime / onEnableTransitionTime;
                enableState = Mathf.Clamp01(enableState);
            }

            if(updatePosition) {
                //UPDATE POSITON STATES
                if(highlighting && highlightStartTime + highlightAnimationPositionTime > offseTime) {
                    highlightPositionState += deltaTime / highlightAnimationPositionTime;
                    highlightPositionState = Mathf.Clamp01(highlightPositionState);
                }
                else if(!highlighting && highlightStopTime + unhighlightAnimationPositionTime > offseTime) {
                    highlightPositionState -= deltaTime / unhighlightAnimationPositionTime;
                    highlightPositionState = Mathf.Clamp01(highlightPositionState);
                }

                if(activating && activateStartTime + highlightAnimationPositionTime > offseTime) {
                    activatePositionState += deltaTime/ highlightAnimationPositionTime;
                    activatePositionState = Mathf.Clamp01(activatePositionState);
                }
                else if(!activating && activateStopTime + unhighlightAnimationPositionTime > offseTime) {
                    activatePositionState -= deltaTime / unhighlightAnimationPositionTime;
                    activatePositionState = Mathf.Clamp01(activatePositionState);
                }
            }

            //UPDATE COLOR STATES
            if(updateColor) {
                if(highlighting && highlightStartTime + highlightAnimationColorTime > offseTime) {
                    highlightColorState += deltaTime / highlightAnimationColorTime;
                    highlightColorState = Mathf.Clamp01(highlightColorState);
                }
                else if(!highlighting && highlightStopTime + unhighlightAnimationColorTime > offseTime) {
                    highlightColorState -= deltaTime / unhighlightAnimationColorTime;
                    highlightColorState = Mathf.Clamp01(highlightColorState);
                }

                if(activating && activateStartTime + highlightAnimationColorTime > offseTime) {
                    activateColorState += deltaTime / highlightAnimationColorTime;
                    activateColorState = Mathf.Clamp01(activateColorState);
                }
                else if(!activating && activateStopTime + unhighlightAnimationColorTime > offseTime) {
                    activateColorState -= deltaTime / unhighlightAnimationColorTime;
                    activateColorState = Mathf.Clamp01(activateColorState);
                }
            }


            //UPDATE SCALE STATES
            if(updateScale) {
                if(highlighting && highlightStartTime + highlightAnimationScaleTime > offseTime) {
                    highlightScaleState += deltaTime / highlightAnimationScaleTime;
                    highlightScaleState = Mathf.Clamp01(highlightScaleState);
                }
                else if(!highlighting && highlightStopTime + unhighlightAnimationScaleTime > offseTime) {
                    highlightScaleState -= deltaTime / unhighlightAnimationScaleTime;
                    highlightScaleState = Mathf.Clamp01(highlightScaleState);
                }

                if(activating && activateStartTime + highlightAnimationScaleTime > offseTime) {
                    activateScaleState += deltaTime / highlightAnimationScaleTime;
                    activateScaleState = Mathf.Clamp01(activateScaleState);
                }
                else if(!activating && activateStopTime + unhighlightAnimationScaleTime > offseTime) {
                    activateScaleState -= deltaTime / unhighlightAnimationScaleTime;
                    activateScaleState = Mathf.Clamp01(activateScaleState);
                }
            }


            //UPDATE ROTATION STATES
            if(updateRotation) {
                if(highlighting && highlightStartTime + highlightAnimationRotationTime > offseTime) {
                    highlightRotationState += deltaTime / highlightAnimationRotationTime;
                    highlightRotationState = Mathf.Clamp01(highlightRotationState);
                }
                else if(!highlighting && highlightStopTime + unhighlightAnimationRotationTime > offseTime) {
                    highlightRotationState -= deltaTime / unhighlightAnimationRotationTime;
                    highlightRotationState = Mathf.Clamp01(highlightRotationState);
                }

                if(activating && activateStartTime + highlightAnimationRotationTime > offseTime) {
                    activateRotationState += deltaTime / highlightAnimationRotationTime;
                    activateRotationState = Mathf.Clamp01(activateRotationState);
                }
                else if(!activating && activateStopTime + unhighlightAnimationRotationTime > offseTime) {
                    activateRotationState -= deltaTime / unhighlightAnimationRotationTime;
                    activateRotationState = Mathf.Clamp01(activateRotationState);
                }
            }
        }


        protected virtual void SetAnimation() {
            if(onEnableTransition) {
                var transparentUnhighlight = unhighlightColor;
                transparentUnhighlight.a = 0;
                var state = onEnableTransitionCurve.Evaluate(enableState);
                currentUnhighlightColor = Color.Lerp(transparentUnhighlight, unhighlightColor, state);

                var transparentHighlight = highlightColor;
                transparentHighlight.a = 0;
                currentHighlightColor = Color.Lerp(transparentHighlight, highlightColor, state);

                var transparentActivate = activateColor;
                transparentActivate.a = 0;
                currentActivateColor = Color.Lerp(transparentActivate, activateColor, state);
            }

            for(int i = 0; i < animationTargets.Length; i++) {
                var animationTarget = animationTargets[i];
                if(animationTarget.transform == null)
                    continue;

                var waveTime = Time.time - highlightStartTime;
                //var animationOffset = useWave && !animationTarget.ignoreWave ? ((Mathf.Sin(waveTime*Mathf.PI * waveFrequency + index*waveOffset*Mathf.PI)) + 1)/2f * waveAmplitude : 0;

                //UPDATE COLOR
                if(!animationTarget.ignoreColor) {
                    float waveOffset = 0;
                    if(!(colorWaveFrequency == 0) && !(colorWaveAmplitude == 0))
                        waveOffset = ((Mathf.Sin(waveTime*Mathf.PI * colorWaveFrequency + Mathf.PI*colorWaveOffset + animationTarget.waveOffset*Mathf.PI)) + 1)/2f * colorWaveAmplitude;

                    var targetColor = Color.Lerp(currentUnhighlightColor, currentHighlightColor, highlightColorCurve.Evaluate(highlightColorState * Mathf.Clamp01(1-animationTarget.colorDampener - waveOffset)));
                    targetColor = Color.Lerp(targetColor, currentActivateColor, highlightColorCurve.Evaluate(activateColorState * Mathf.Clamp01(1-animationTarget.colorDampener - waveOffset)));
                    animationTarget.SetColor(targetColor);
                }

                //UPDATE POSITIONS
                if(!animationTarget.ignorePosition) {
                    float waveOffset = 0;
                    if(!(positionWaveFrequency == 0) && !(positionWaveAmplitude == 0))
                        waveOffset = ((Mathf.Sin(waveTime*Mathf.PI * positionWaveFrequency + Mathf.PI*positionWaveOffset + animationTarget.waveOffset*Mathf.PI)) + 1)/2f * positionWaveAmplitude;

                    var targetPos = Vector3.Lerp(animationTarget.StartPosition, animationTarget.StartPosition + highlightPosition, positionAnimationCurve.Evaluate(highlightPositionState * Mathf.Clamp01(1 - animationTarget.positionDampener - waveOffset)));
                    targetPos = Vector3.Lerp(targetPos, activatePosition, positionAnimationCurve.Evaluate(activatePositionState* Mathf.Clamp01(1 - animationTarget.positionDampener - waveOffset)));
                    animationTarget.transform.localPosition = targetPos;
                }

                //UPDATE ROTATION
                if(!animationTarget.ignoreRotation) {
                    float waveOffset = 0;
                    if(!(rotationWaveFrequency == 0) && !(rotationWaveAmplitude == 0))
                        waveOffset = ((Mathf.Sin(waveTime*Mathf.PI * rotationWaveFrequency + Mathf.PI*rotationWaveOffset + animationTarget.waveOffset*Mathf.PI)) + 1)/2f * rotationWaveAmplitude;

                    var targetRotation = Vector3.Lerp(animationTarget.StartRotation, animationTarget.StartRotation + highlightRotationOffset, rotationAnimationCurve.Evaluate(highlightRotationState*Mathf.Clamp01(1-animationTarget.rotationDampener - waveOffset)));
                    targetRotation = Vector3.Lerp(targetRotation, activateRotationOffset, rotationAnimationCurve.Evaluate(activateRotationState * Mathf.Clamp01(1 - animationTarget.rotationDampener - waveOffset)));
                    animationTarget.transform.localEulerAngles =  targetRotation;
                }

                //UPDATE SCALE
                if(!animationTarget.ignoreScale) {
                    float waveOffset = 0;
                    if(!(scaleWaveFrequency == 0) && !(scaleWaveAmplitude == 0) && !activating)
                        waveOffset = ((Mathf.Sin(waveTime*Mathf.PI * scaleWaveFrequency + Mathf.PI*scaleWaveOffset + animationTarget.waveOffset*Mathf.PI)) + 1)/2f * scaleWaveAmplitude;

                    var targetScale = Vector3.Lerp(animationTarget.StartScale, animationTarget.StartScale * (1 + highlightScaleOffset), scaleAnimationCurve.Evaluate(highlightScaleState)*Mathf.Clamp01(1-animationTarget.scaleDampener - waveOffset));
                    targetScale = Vector3.Lerp(targetScale, animationTarget.StartScale * (1 + activateScaleOffset), scaleAnimationCurve.Evaluate(activateScaleState * Mathf.Clamp01(1 - animationTarget.scaleDampener - waveOffset)));
                    animationTarget.transform.localScale = targetScale;
                }
            }

        }
    }
}
