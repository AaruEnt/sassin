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
    bool isBurning = false;
    float timer = 0f;
    Shader tmpShader = default(Shader);

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
        }

        if ((timer >= burnTime || (earlyCutTime > 0 && timer >= earlyCutTime)) && destroyOnFinish) {
            if (_g)
                _g.ForceHandsRelease();
            Destroy(this.gameObject);
        } else if ((timer >= burnTime || (earlyCutTime > 0 && timer >= earlyCutTime)) && !destroyOnFinish)
        {
            _m.material.shader = tmpShader;
            isBurning = false;
            timer = 0f;
        }
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
