using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookahHose : MonoBehaviour
{
    public LineRenderer line;
    public Transform bodyConnectPoint;
    public Transform mouthConnectPoint;
    public Transform curveHint;
    public Transform curveHint2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DrawCubicBezierCurve(bodyConnectPoint.position, curveHint.position, curveHint2.position, mouthConnectPoint.position);
    }

    void DrawQuadraticBezierCurve(Vector3 point0, Vector3 point1, Vector3 point2)
    {
        line.positionCount = 200;
        float t = 0f;
        Vector3 B = new Vector3(0, 0, 0);
        for (int i = 0; i < line.positionCount; i++)
        {
            B = (1 - t) * (1 - t) * point0 + 2 * (1 - t) * t * point1 + t * t * point2;
            line.SetPosition(i, B);
            t += (1 / (float)line.positionCount);
        }
    }

    void DrawCubicBezierCurve(Vector3 point0, Vector3 point1, Vector3 point2, Vector3 point3)
    {

        line.positionCount = 200;
        float t = 0f;
        Vector3 B = new Vector3(0, 0, 0);
        for (int i = 0; i < line.positionCount; i++)
        {
            B = (1 - t) * (1 - t) * (1 - t) * point0 + 3 * (1 - t) * (1 - t) *
                t * point1 + 3 * (1 - t) * t * t * point2 + t * t * t * point3;

            line.SetPosition(i, B);
            t += (1 / (float)line.positionCount);
        }
    }
}
