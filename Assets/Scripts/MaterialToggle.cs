using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialToggle : MonoBehaviour
{
    public Material firstSwap;
    public Material secondSwap;
    public MeshRenderer renderer;

    private bool toggle = false;

    void Start() {
        if (!renderer)
            renderer = GetComponent<MeshRenderer>();
    }
    
    
    public void Toggle() {
        if (!toggle) {
            toggle = true;
            renderer.material = firstSwap;
        } else {
            toggle = false;
            renderer.material = secondSwap;
        }

    }
}
