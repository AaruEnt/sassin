using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Autohand
{
    [Serializable]
    public class UnityCanvasPointerEvent : UnityEvent<Vector3, GameObject> { }

    [HelpURL("https://app.gitbook.com/s/5zKO0EvOjzUDeT2aiFk3/auto-hand/extras/ui-interaction"), DefaultExecutionOrder(10000)]
    public class HandCanvasPointer : MonoBehaviour
    {
        [Header("References")]
        public GameObject hitPointMarker;
        private LineRenderer lineRenderer;
        public bool useSmoothing = true;
        public float forwardPointerSmoothing = 25f;


        [Header("Ray settings")]
        public float raycastLength = 8.0f;
        public bool autoShowTarget = true;
        public LayerMask UILayer;


        [Header("Events")]
        public UnityCanvasPointerEvent StartSelect;
        public UnityCanvasPointerEvent StopSelect;
        public UnityCanvasPointerEvent StartPoint;
        public UnityCanvasPointerEvent StopPoint;



        private GameObject _currTarget;
        public GameObject currTarget
        {
            get { return _currTarget; }
        }

        private float _currDistance;


        Vector3 currentSmoothForward;


        public RaycastHit lastHit { get; private set; }

        // Internal variables
        private bool hover = false;
        AutoInputModule inputModule = null;
        float lineSegements = 10f;

        bool beingDestroyed = false;
        static Camera cam = null;
        public static Camera UICamera
        {
            get
            {
                if (cam == null)
                {
                    cam = new GameObject("Camera Canvas Pointer (I AM CREATED AT RUNTIME FOR UI CANVAS INTERACTION, I AM NOT RENDERING ANYTHING, I AM NOT CREATING ADDITIONAL OVERHEAD)").AddComponent<Camera>();
                    cam.clearFlags = CameraClearFlags.Nothing;
                    cam.stereoTargetEye = StereoTargetEyeMask.None;
                    cam.orthographic = true;
                    cam.orthographicSize = 0.001f;
                    cam.cullingMask = 0;
                    cam.nearClipPlane = 0.001f;
                    cam.depth = 0f;
                    cam.allowHDR = false;
                    cam.enabled = false;
                    cam.fieldOfView = 0.00001f;
                    cam.transform.parent = AutoHandExtensions.transformParent;

#if (UNITY_2020_3_OR_NEWER)
                    var canvases = AutoHandExtensions.CanFindObjectsOfType<Canvas>(true);
#else
                    var canvases = FindObjectsOfType<Canvas>();
#endif
                    foreach(var canvas in canvases) {
                        if(canvas.renderMode == RenderMode.WorldSpace)
                            canvas.worldCamera = cam;
                    }

                }
                return cam;
            }
        }
        int pointerIndex;

        protected virtual void OnEnable()
        {

            if(lineRenderer != null)
                lineRenderer.positionCount = (int)lineSegements;
            if (inputModule.Instance != null)
                pointerIndex = inputModule.Instance.AddPointer(this);
            ShowRay(false);
        }

        protected virtual void OnDisable()
        {
            if(inputModule) inputModule.Instance?.RemovePointer(this);
        }

        protected virtual void OnDestroy() {
            beingDestroyed = true;
            if(cam != null)
                Destroy(cam.gameObject);
            cam = null;
        }

        public void SetIndex(int index)
        {
            pointerIndex = index;
        }

        protected internal virtual void Preprocess()
        {
            if(beingDestroyed) return;

            UICamera.farClipPlane = raycastLength;
            UICamera.transform.position = transform.position;
            UICamera.transform.forward = currentSmoothForward;
        }

        public virtual void Press() {
            // Handle the UI events
            if(inputModule) inputModule.ProcessPress(pointerIndex);

            // Show the ray when they attemp to press
            if(!autoShowTarget && hover) ShowRay(true);

            PointerEventData data = inputModule.GetData(pointerIndex);
            if(data != null && data.selectedObject != null) {
                StartSelect?.Invoke(data.pointerCurrentRaycast.worldPosition, data.selectedObject);
            }
        }

        public virtual void Release()
        {
            // Handle the UI events
            if(inputModule) inputModule.ProcessRelease(pointerIndex);

            PointerEventData data = inputModule.GetData(pointerIndex);
            var selectedObject = data.selectedObject;
            if(selectedObject != null) 
                StopSelect?.Invoke(data.pointerCurrentRaycast.worldPosition, selectedObject);
        }

        protected virtual void Awake()
        {
            if (lineRenderer == null)
                gameObject.CanGetComponent(out lineRenderer);

            if (inputModule == null)
            {
                if (gameObject.CanGetComponent<AutoInputModule>(out var inputMod))
                {
                    inputModule = inputMod;
                }
                else if (!(inputModule = AutoHandExtensions.CanFindObjectOfType<AutoInputModule>()))
                {
                    EventSystem system = AutoHandExtensions.CanFindObjectOfType<EventSystem>();
                    if(system == null) {
                        system = new GameObject().AddComponent<EventSystem>();
                        system.name = "UI Input Event System";
                    }
                    inputModule = system.gameObject.AddComponent<AutoInputModule>();
                    inputModule.transform.parent = AutoHandExtensions.transformParent;
                }
            }
        }

        protected virtual void LateUpdate()
        {
            if(useSmoothing) {
                var currentAngleDistance = Vector3.Angle(currentSmoothForward, transform.forward);
                currentSmoothForward = Vector3.RotateTowards(currentSmoothForward, transform.forward, Time.deltaTime * forwardPointerSmoothing + Time.deltaTime * forwardPointerSmoothing * currentAngleDistance, 1000f);
                currentSmoothForward.Normalize();
            }
            else
                currentSmoothForward = transform.forward;

            UpdateLine();
        }

        protected virtual void UpdateLine()
        {

            PointerEventData data = inputModule.GetData(pointerIndex);
            float targetLength = data.pointerCurrentRaycast.gameObject == null ? raycastLength : data.pointerCurrentRaycast.distance;

            if(targetLength > 0) {
                _currTarget = data.pointerCurrentRaycast.gameObject;
                _currDistance = targetLength;
            }
            else {
                _currTarget = null;
            }

            if (data.pointerCurrentRaycast.gameObject != null && !hover){
                lastHit = CreateRaycast(targetLength);
                Vector3 endPosition = transform.position + (currentSmoothForward * targetLength);
                if (lastHit.collider) endPosition = lastHit.point;


                if(lastHit.collider != null) {
                    currentSmoothForward = transform.forward;
                    StartPoint?.Invoke(lastHit.point, lastHit.transform.gameObject);
                }
                else {
                    currentSmoothForward = transform.forward;
                    StartPoint?.Invoke(endPosition, null);
                }


                // Show the ray if autoShowTarget is on when they enter the canvas
                if (autoShowTarget) ShowRay(true);

                hover = true;
            }
            else if (data.pointerCurrentRaycast.gameObject == null && hover){
                lastHit = CreateRaycast(targetLength);
                Vector3 endPosition = transform.position + (currentSmoothForward * targetLength);
                if (lastHit.collider) endPosition = lastHit.point;

                if (lastHit.collider != null)
                    StopPoint?.Invoke(lastHit.point, lastHit.transform.gameObject);
                else
                    StopPoint?.Invoke(endPosition, null);

                // Hide the ray when they leave the canvas
                ShowRay(false);
                hover = false;
            }

            if(hover) {
                lastHit = CreateRaycast(targetLength);

                Vector3 endPosition = transform.position + (currentSmoothForward * targetLength);

                //Handle the hitmarker
                hitPointMarker.transform.position = endPosition;
                hitPointMarker.transform.forward = data.pointerCurrentRaycast.worldNormal;

                if(lastHit.collider) {
                    endPosition = lastHit.point;
                    hitPointMarker.transform.forward = lastHit.collider.transform.forward;
                    hitPointMarker.transform.position = endPosition + hitPointMarker.transform.forward * 0.002f;
                }

                //Handle the line renderer
                for(int i = 0; i < lineSegements; i++) {
                    lineRenderer.SetPosition(i, Vector3.Lerp(transform.position, endPosition, i/ lineSegements));
                }
            }



        }

        protected virtual RaycastHit CreateRaycast(float dist){
            RaycastHit hit;
            Ray ray = new Ray(transform.position, currentSmoothForward);
            Physics.Raycast(ray, out hit, dist, UILayer);

            return hit;
        }

        protected virtual void ShowRay(bool show) {
            if(hitPointMarker != null)
                hitPointMarker.SetActive(show);
            if(lineRenderer != null)
                lineRenderer.enabled = show;
        }

    }
}