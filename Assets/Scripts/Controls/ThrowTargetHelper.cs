using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AaruThrowVR
{
    public class ThrowTargetHelper : MonoBehaviour
    {

        public bool _showReticle = true;

        private GameObject reticle;
        private List<ImprovedThrow> targettingHandles = new List<ImprovedThrow>();
        public static List<ThrowTargetHelper> AllTargets
        {
            get
            {
                if (_allTargets == null) _allTargets = new List<ThrowTargetHelper>();
                return _allTargets;
            }
            protected set
            {
                _allTargets = value;
            }
        }
        private static List<ThrowTargetHelper> _allTargets;

        private void Awake()
        {
            reticle = GameObject.CreatePrimitive(PrimitiveType.Quad);
            reticle.GetComponent<Collider>().enabled = false;
            reticle.GetComponent<Renderer>().material = Resources.Load<Material>("TargetReticle");
            reticle.transform.localPosition = Vector3.zero;
            reticle.name = "Reticle";
            ShowHideReticle(false);
        }

        private void Update()
        {
            targettingHandles.RemoveAll(x => x == null);
            ShowHideReticle(_showReticle && targettingHandles.Count > 0);
        }

        private void LateUpdate()
        {
            MaintainReticleSize();
        }

        private void OnEnable()
        {
            AllTargets.Add(this);
        }

        private void OnDisable()
        {
            AllTargets.Remove(this);
        }

        public void AddTargettingHandle(ImprovedThrow handle)
        {
            if (!targettingHandles.Contains(handle)) targettingHandles.Add(handle);
        }

        public void RemoveTargettingHandle(ImprovedThrow handle)
        {
            if (targettingHandles.Contains(handle)) targettingHandles.Remove(handle);
        }
        private void ShowHideReticle(bool show)
        {
            reticle.SetActive(show);
        }

        private void MaintainReticleSize()
        {
            Camera cam = Camera.main;
            reticle.transform.position = this.transform.position;
            reticle.transform.LookAt(cam.transform, cam.transform.up);
            reticle.transform.localScale = (Vector3.one * Vector3.Distance(cam.transform.position, this.transform.position) * .1f);
        }

        public Vector3 GetTargetPosition()
        {
            return transform.position;
        }
    }
}