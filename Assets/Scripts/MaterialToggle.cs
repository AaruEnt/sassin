using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField, Tooltip("Material to be enabled after odd-numbered swaps (1, 3, 5, etc)")]
    private Material firstSwap;

    [SerializeField, Tooltip("Material to be enabled after even-numbered swaps (2, 4 , 6, etc)")]
    private Material secondSwap;

    [SerializeField, Tooltip("The mesh renderer to pull the material from. If not set, program will try to find it on the object.")]
    private MeshRenderer mRenderer;

    // toggle state
    private bool toggle = false;

    void Start() {
        if (!mRenderer)
            mRenderer = GetComponent<MeshRenderer>();
    }
    
    
    public void Toggle() {
        if (!toggle) {
            toggle = true;
            mRenderer.material = firstSwap;
        } else {
            toggle = false;
            mRenderer.material = secondSwap;
        }

    }
}
