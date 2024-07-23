using System;
using UnityEngine;
using UnityEngine.UI;

namespace CloudFine.ThrowLab.UI
{
    public class UISmoothingVisual : MonoBehaviour {

        public Image[] bars;
        private ThrowConfiguration.EstimationAlgorithm algorithm;
        private Vector3[] dummyData;
        private float[] weights;
        private Func<Vector3[],float[]> _func;

        private void Awake()
        {
            dummyData = new Vector3[bars.Length];
            for(int i =0; i < dummyData.Length; i++)
            {
                dummyData[i] = UnityEngine.Random.insideUnitSphere;
            }
        }

        public void SetFunc(Func<Vector3[],float[]> func)
        {
            _func = func;
        }

        public void Refresh()
        {
            if (_func == null) return;

            weights = _func(dummyData);
            for (int i = 0; i < bars.Length; i++)
            {
                bars[i].fillAmount = weights[i];
            }

        }
        
    }
}