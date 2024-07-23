using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CloudFine;
using System;

namespace CloudFine.ThrowLab.UI
{
    [RequireComponent(typeof(LineRenderer))]
    public class UICurveLine : MonoBehaviour
    {
        private Func<float, float> curveFunction;
        private LineRenderer line;
        public int numPositions = 100;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
        }


        public void SetCurveFunc(Func<float,float> curveFunc)
        {
            curveFunction = curveFunc;
            RefreshCurve();
        }

        public void RefreshCurve()
        {
            if (curveFunction==null) return;

            Vector3[] positions = new Vector3[numPositions + 1];
            line.positionCount = numPositions + 1;

            for (int i = 0; i < numPositions; i++)
            {
                float t = ((float)i / numPositions);
                positions[i] = new Vector3(t, curveFunction(t), 0);
            }
            positions[numPositions] = new Vector3(1, curveFunction(1), 0);
            line.SetPositions(positions);
        }
    }

     
}
