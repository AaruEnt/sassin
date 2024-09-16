using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class Burnable : MonoBehaviour
{
    [Tooltip("Optional variable, will use burn shader graph if not set")]
    public Shader burnShader;
    public float burnTime = 5f;
    public float burnShaderPercentCap = 1.2f;
    public float earlyCutTime = -1;
    public bool reverse = false;
    public bool destroyOnFinish = true;
    MeshRenderer _m;
    Grabbable _g;
    public bool isBurning = false;
    float timer = 0f;
    Shader tmpShader = default(Shader);
    public bool useFlashColor = false;
    [ShowIf("useFlashColor")]
    public Material flashMat;

    public Texture baseTex;
    public Color baseColor = Color.white;


    [Button]
    public void editorStartButn() { StartBurnManual(); }
    // Start is called before the first frame update
    void Start()
    {
        if (!burnShader)
            burnShader = Shader.Find("Shader Graphs/Burn");
        _m = GetComponent<MeshRenderer>();
        _m.material = new Material(_m.material);
        _g = GetComponent<Grabbable>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isBurning)
        {
            if (_m.material.shader != burnShader)
            {
                tmpShader = _m.material.shader;
                _m.material.shader = burnShader;
            }
            gameObject.tag = "Fire";
            timer += Time.deltaTime;
            float conv = (timer / burnTime) * burnShaderPercentCap;
            if (reverse)
                conv = burnShaderPercentCap - conv;
            _m.material.SetFloat("_burnAmount", conv);
            _m.material.SetColor("_baseColor", baseColor);
            if (baseTex)
                _m.material.SetTexture("_baseTexture", baseTex);
           //_m.material.SetColor("_hotColor", hotColor);
        }

        if ((timer >= burnTime || (earlyCutTime > 0 && timer >= earlyCutTime)) && destroyOnFinish) {
            if (_g)
                _g.ForceHandsRelease();
            Destroy(this.gameObject);
        } else if ((timer >= burnTime || (earlyCutTime > 0 && timer >= earlyCutTime)) && !destroyOnFinish)
        {
            if (useFlashColor)
            {
                _m.material.shader = flashMat.shader;
                _m.material.SetColor("_baseColor", flashMat.color);
                _m.material.EnableKeyword("_EMISSION");
                _m.material.SetColor("_EmissionColor", Color.white);
                isBurning = false;
                timer = 0f;
                StartCoroutine(ResetMaterialToBase());
            }
            else
            {
                _m.material.shader = tmpShader;
                isBurning = false;
                timer = 0f;
            }
        }
    }

    private IEnumerator ResetMaterialToBase()
    {
        yield return new WaitForSeconds(0.08f);
        _m.material.shader = tmpShader;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isBurning || !other.gameObject.CompareTag("Fire")) { return; }

        isBurning = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isBurning || !collision.gameObject.CompareTag("Fire")) { return; }

        isBurning = true;
    }

    public void StartBurnManual()
    {
        isBurning = true;
    }
}
