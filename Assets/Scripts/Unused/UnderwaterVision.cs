using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UnderwaterVision : MonoBehaviour
{
    public Volume v;
    public Color waterColor;

    public void SetUnderwater()
    {
        var p = v?.profile;

        if (p && p.TryGet<ColorAdjustments>(out var ca) )
        {
            ColorParameter c = new ColorParameter(waterColor);
            ca.colorFilter.value = c.value;
        }
    }
}
