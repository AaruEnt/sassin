using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BF_InteractiveEffectsAdditional : MonoBehaviour
{
    public Transform mainCamera;
    public RenderTexture rt;
    public string GlobalTexName = "_GlobalEffectRTAdditional";
    public string GlobalOrthoName = "_OrthographicCamSizeAdditional";
    public bool isPaced = false;

    private float orthoMem = 0;
    private Vector3 camDir;
    private Coroutine waitPace;
    private bool paceRunning = false;
    private void Awake()
    {
        orthoMem = GetComponent<Camera>().orthographicSize;
        Shader.SetGlobalFloat(GlobalOrthoName, orthoMem);
        Shader.SetGlobalTexture(GlobalTexName, rt);
    }
    private void OnEnable()
    {
        orthoMem = GetComponent<Camera>().orthographicSize;
        Shader.SetGlobalFloat(GlobalOrthoName, orthoMem);
        Shader.SetGlobalTexture(GlobalTexName, rt);
    }

    private void MoveCamera()
    {
        if (mainCamera != null)
        {
            camDir = Vector3.ProjectOnPlane(mainCamera.forward, Vector3.up).normalized;
            camDir.y = 0f;

            if (mainCamera != null)
            {
                float YView = Vector3.Angle(Vector3.down, mainCamera.forward);
                transform.position = new Vector3(mainCamera.position.x, mainCamera.position.y + 20, mainCamera.position.z) + camDir.normalized * Mathf.Max(0f, orthoMem - 20f) * Mathf.Clamp01(((YView - 35) * 3) / 35);
            }
        }
        Shader.SetGlobalVector("_PositionAdd", transform.position);
        transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
    }

    private void Update()
    {
        if (isPaced)
        {
            if (!paceRunning)
            {
                waitPace = StartCoroutine(WaitPace());
            }
        }
        else
        {
            if (paceRunning)
            {
                paceRunning = false;
                StopCoroutine(WaitPace());
            }

            MoveCamera();
        }
    }
    private IEnumerator WaitPace()
    {
        for (; ; )
        {
            paceRunning = true;

            MoveCamera();

            yield return new WaitForSeconds(1f);
        }
    }
}