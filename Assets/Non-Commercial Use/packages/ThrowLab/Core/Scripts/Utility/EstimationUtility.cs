using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EstimationUtility
{

    public static Vector3 SimpleAverage(Vector3[] inputs, out float[] componentWeights)
    {
        componentWeights = new float[inputs.Length];
        Vector3 output = Vector3.zero;
        for(int i=0; i<inputs.Length; i++)
        {
            output += inputs[i];
            componentWeights[i] = 1;
        }
        output /= inputs.Length;
        return output;
    }

    public static Vector3 WeightedMovingAverage(Vector3[] inputs, out float[] componentWeights)
    {
        componentWeights = new float[inputs.Length];
        Vector3 output = Vector3.zero;
        float totalWeight = 0;
        for(int i=0; i<inputs.Length; i++)
        {
            float weight = (i + 1f) / inputs.Length;
            componentWeights[i] = weight;
            totalWeight += weight;
            output += weight * inputs[i];
        }
        output /= totalWeight;
        return output;
    }

    public static Vector3 ExponentialMovingAverage(Vector3[] inputs, out float[] componentWeights)
    {
        float weightCoefficient = Mathf.Pow(((float)inputs.Length) / (inputs.Length + 1f), 4f);
        componentWeights = new float[inputs.Length];
        Vector3 output = Vector3.zero;
        float totalWeight = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            float weight = Mathf.Pow(weightCoefficient, (inputs.Length - 1f) - i);
            componentWeights[i] = weight;
            totalWeight += weight;
            output += weight * inputs[i];
        }
        output /= totalWeight;
        return output;
    }

    public static Vector3 CustomCurveAverage(Vector3[] inputs, AnimationCurve curve, out float[] componentWeights)
    {
        componentWeights = new float[inputs.Length];
        Vector3 output = Vector3.zero;
        float totalWeight = 0;
        for (int i = 0; i < inputs.Length; i++)
        {
            float t = inputs.Length>1? (float)i / (inputs.Length - 1) : 0;
            float weight = curve.Evaluate(t);
            componentWeights[i] = weight;
            output += inputs[i] * weight;
            totalWeight += weight;
        }
        output /= totalWeight;
        return output;
    }
}
