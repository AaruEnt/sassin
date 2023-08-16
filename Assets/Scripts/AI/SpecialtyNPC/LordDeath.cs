using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class LordDeath : MonoBehaviour
{
    public GameObject fireParticle;
    public GameObject explosionParticle;
    public GameObject smokeParticle;
    public MeshRenderer mesh;
    public UnityEvent OnActivateExplosion;
    public float blackenTime = 6f;

    private Material mat;
    private bool isRunning = false;
    private bool isSetup = false;
    private float cd = 0f;
    private Color startColor;

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            if (!isSetup)
            {
                mat = mesh.material;
                startColor = mat.color;
                fireParticle.SetActive(true);
                isSetup = true;
            }
            cd += Time.deltaTime;
            float diff = cd / blackenTime;
            Color tmp = Color.Lerp(startColor, Color.black, diff);
            if (tmp == Color.black)
            {
                UnityEngine.Debug.Log(tmp.ToString());
                isRunning = false;
                StartCoroutine(Pop());
            }
            mat.color = tmp;
        }
    }

    public void StartDeath()
    {
        isRunning = true;
    }

    IEnumerator Pop()
    {
        yield return new WaitForSeconds(1.5f);
        OnActivateExplosion.Invoke();
        explosionParticle.SetActive(true);
        smokeParticle.SetActive(true);
    }
}
