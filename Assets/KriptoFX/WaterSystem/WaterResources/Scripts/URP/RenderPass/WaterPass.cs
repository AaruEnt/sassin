using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KWS
{
    public abstract class WaterPass : ScriptableRenderPass
    {
        public   WaterSystem WaterInstance;
        internal bool        IsInitialized;

        public abstract void Release();
    }
}