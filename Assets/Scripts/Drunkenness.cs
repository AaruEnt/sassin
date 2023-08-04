using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Drunkenness : MonoBehaviour
{
    public float tipsyThreshhold = 50f;
    public float drunkThreshhold = 75f;
    public float wastedThreshhold = 100f;

    public Volume volume;

    [SerializeField]
    private float drunkenness = 0f;

    private bool addedThisFrame = false;
    private bool wasted = false;
    private bool up = false;

    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
        if (drunkenness > 0f && !addedThisFrame)
        {
            drunkenness -= 3 * Time.deltaTime;
        }
        var p = volume?.profile;
        MotionBlur mb;
        switch (drunkenness)
        {
            case var _ when drunkenness >= wastedThreshhold:
                wasted = true;
                break;
            case var _ when drunkenness >= drunkThreshhold:
                p.TryGet<MotionBlur>(out mb);
                mb.active = true;
                mb.intensity.value = 1f;
                wasted = false;
                break;
            case var _ when drunkenness >= tipsyThreshhold:
                p.TryGet<MotionBlur>(out mb);
                mb.active = true;
                mb.intensity.value = 0.5f;
                wasted = false;
                break;
            case var _ when drunkenness <= 0f:
                p.TryGet<MotionBlur>(out mb);
                mb.active = false;
                wasted = false;
                break;
            default: break;
        }

        LensDistortion ld;
        p.TryGet<LensDistortion>(out ld);
        float i = ld.intensity.value;
        if (wasted || i != 0)
        {
            if (up)
            {
                i += Time.deltaTime;
                if (!wasted && i >= 0)
                    i = 0;
                else if (i >= 0.5f)
                {
                    i = 0.5f;
                    up = false;
                }
            } else
            {
                i -= Time.deltaTime;
                if (!wasted && i <= 0)
                    i = 0;
                else if (i <= -0.5f)
                {
                    i = -0.5f;
                    up = true;
                }
            }
            ld.intensity.value = i;

        }


        addedThisFrame = false;
    }

    public void AddDrunkenness(float toAdd)
    {
        if (toAdd > 0f && !addedThisFrame)
        {
            drunkenness += toAdd;
            addedThisFrame = true;
        }
    }
}
