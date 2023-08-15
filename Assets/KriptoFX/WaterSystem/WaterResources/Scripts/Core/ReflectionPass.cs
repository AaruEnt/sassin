﻿using System;
using UnityEngine;

namespace KWS
{
    public abstract class ReflectionPass
    {
        public Action<RenderTexture> OnReflectionRendered;

        public abstract void RenderReflection(WaterSystem waterInstance, Camera currentCamera);

        public abstract void OnEnable();
        public abstract void Release();

    }
}