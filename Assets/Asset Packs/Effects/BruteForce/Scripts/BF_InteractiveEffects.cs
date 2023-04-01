using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BF_InteractiveEffects : MonoBehaviour
{
    public Transform transformToFollow;
    public RenderTexture rt;
    public string GlobalTexName = "_GlobalEffectRT";
    public string GlobalOrthoName = "_OrthographicCamSize";
    public bool isPaced = false;

    private float orthoMem = 0;
    private Coroutine waitPace;
    private bool paceRunning = false;

    private void Awake()
    {
        orthoMem = GetComponent<Camera>().orthographicSize;
        Shader.SetGlobalFloat(GlobalOrthoName, orthoMem);
        Shader.SetGlobalTexture(GlobalTexName, rt);
        Shader.SetGlobalFloat("_HasRT", 1);
    }
    private void OnEnable()
    {
        orthoMem = GetComponent<Camera>().orthographicSize;
        Shader.SetGlobalFloat(GlobalOrthoName, orthoMem);
        Shader.SetGlobalTexture(GlobalTexName, rt);
        Shader.SetGlobalFloat("_HasRT", 1);
    }
    private void MoveCamera()
    {
        if (transformToFollow != null)
        {
            transform.position = new Vector3(transformToFollow.position.x, transformToFollow.position.y + 20, transformToFollow.position.z);
        }
        Shader.SetGlobalVector("_Position", transform.position);
        transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
    }
    private void Update()
    {
        if(isPaced)
        {
            if(!paceRunning)
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
        for(; ;)
        {
            paceRunning = true;

            MoveCamera();

            yield return new WaitForSeconds(1f);
        }
    }
}