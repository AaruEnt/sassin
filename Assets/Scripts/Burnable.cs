using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burnable : MonoBehaviour
{
    [Tooltip("Optional variable, will use burn shader graph if not set")]
    public Shader burnShader;
    public float burnTime = 5f;
    public float burnShaderPercentCap = 1.2f;
    MeshRenderer _m;
    Grabbable _g;
    bool isBurning = false;
    float timer = 0f;
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
                _m.material.shader = burnShader;
            }
            timer += Time.deltaTime;
            float conv = timer / burnTime;
            _m.material.SetFloat("_burnAmount", conv);
        }

        if (timer >= burnTime) {
            _g.ForceHandsRelease();
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isBurning || !other.gameObject.CompareTag("Fire")) { return; }

        isBurning = true;
    }
}
