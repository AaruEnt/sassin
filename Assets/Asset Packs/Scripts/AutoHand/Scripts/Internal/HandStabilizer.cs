using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Autohand{
    //This script is used to hide rigidbody physics instabilitites by
    //putting the hand where it visually should be on prerender
    //and putting it where it physically should be on post render
    [DefaultExecutionOrder(int.MaxValue)]
    public class HandStabilizer : MonoBehaviour{
        public HandBase hand = null;

        void Start(){
            if (!GetComponent<Camera>().enabled || hand == null)
                enabled = false;
        }

        void OnEnable(){
            if(GraphicsSettings.defaultRenderPipeline != null){
                RenderPipelineManager.beginCameraRendering += OnPreRenderEvent;
                RenderPipelineManager.endCameraRendering += OnPostRenderEvent;
            }
        }

        void OnDisable(){
            if(GraphicsSettings.defaultRenderPipeline != null){
                RenderPipelineManager.beginCameraRendering -= OnPreRenderEvent;
                RenderPipelineManager.endCameraRendering -= OnPostRenderEvent;
            }
        }

        private void Update() {
            if(hand == null)
                Destroy(this);
        }

        private void OnWillRenderObject() {
            if(hand != null && hand.gameObject.activeInHierarchy)
                hand.OnWillRenderObject();
        }

        private void OnPreRender() {
            if(hand != null && hand.gameObject.activeInHierarchy)
                hand.OnWillRenderObject();
        }

        private void OnPostRender() {
            if(hand != null && hand.gameObject.activeInHierarchy)
                hand.OnPostRender();
        }



        private void OnPreRenderEvent(ScriptableRenderContext context, List<Camera> cameras) {
            if(hand != null && hand.gameObject.activeInHierarchy)
                hand.OnWillRenderObject();
        }
        private void OnPreRenderEvent(ScriptableRenderContext context, Camera cam) {
            if(hand != null && hand.gameObject.activeInHierarchy)
                hand.OnWillRenderObject();
        }
        private void OnPostRenderEvent(ScriptableRenderContext context, Camera cam) {
            if(hand != null && hand.gameObject.activeInHierarchy)
                hand.OnPostRender();
        }

        private void OnPostRenderEvent(ScriptableRenderContext context, List<Camera> cameras) {
            if(hand != null && hand.gameObject.activeInHierarchy)
                hand.OnPostRender();
        }
        
    }
}
