using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class BF_SquareBanding : MonoBehaviour
{
    public Material cinematicBandsFX = null;
    
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, cinematicBandsFX);
    }
}
