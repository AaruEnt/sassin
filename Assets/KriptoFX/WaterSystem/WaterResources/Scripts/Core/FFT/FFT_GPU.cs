using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace KWS
{
    public class FFT_GPU
    {
        private WaterSystem _waterInstance;
        private int _lodIdx;
        private int _currentSize;

        public enum SizeSetting
        {
            Size_32 = 32,
            Size_64 = 64,
            Size_128 = 128,
            Size_256 = 256,
            Size_512 = 512,
        }


        public RenderTexture DisplaceTexture;
        public RenderTexture NormalTexture;
        //public RenderTexture LeanTexture;

        RenderTexture spectrumInitRenderTexture;
        RenderTexture spectrumHeight;
        RenderTexture spectrumDisplaceX;
        RenderTexture spectrumDisplaceZ;
        RenderTexture fftTemp1;
        RenderTexture fftTemp2;
        RenderTexture fftTemp3;

        Material normalComputeMaterial;


        ComputeShader spectrumShader;
        ComputeShader shaderFFT;


        Texture2D texButterfly;

        float prevWindTurbulence;
        float prevWindSpeed;
        private float prevWindRotation;

        int kernelSpectrumInit;
        int kernelSpectrumUpdate;
        Color[] butterflyColors;

        private bool _isWaterHandlerInitialized;

        private void OnWaterSettingsChanged(WaterSystem waterInstance, WaterSystem.WaterTab changedTab)
        {
            if (!changedTab.HasFlag(WaterSystem.WaterTab.Waves)) return;

            if(DisplaceTexture == null || DisplaceTexture.width != GetCurrentSizeRelativeToLod()) InitializeResources();
        }

        public void Initialize(WaterSystem waterInstance, int lodIdx)
        {
            _waterInstance                     =  waterInstance;
            _lodIdx                            =  lodIdx;
            InitializeResources();
        }

        public void Release()
        {
            if(_isWaterHandlerInitialized) WaterSystem.OnWaterSettingsChanged -= OnWaterSettingsChanged;
            _isWaterHandlerInitialized = false;

            KW_Extensions.SafeDestroy(spectrumShader, shaderFFT, texButterfly, normalComputeMaterial);
            KWS_CoreUtils.ReleaseRenderTextures(spectrumInitRenderTexture, spectrumHeight, spectrumDisplaceX, spectrumDisplaceZ, fftTemp1, fftTemp2, fftTemp3, DisplaceTexture, NormalTexture);

            prevWindTurbulence = -1;
            prevWindSpeed = -1;
            prevWindRotation = -1;
        }

        int GetCurrentSizeRelativeToLod()
        {
            if (_lodIdx > 0) return _waterInstance.Settings.FFT_SimulationSize == SizeSetting.Size_512 ? 128 : 64;
            
            return (int) _waterInstance.Settings.FFT_SimulationSize;
        }


        void InitializeResources()
        {
            Release();

            spectrumShader = Object.Instantiate(Resources.Load<ComputeShader>("Common/FFT/Spectrum_GPU"));
            kernelSpectrumInit = spectrumShader.FindKernel("SpectrumInitalize");
            kernelSpectrumUpdate = spectrumShader.FindKernel("SpectrumUpdate");
            shaderFFT = Object.Instantiate(Resources.Load<ComputeShader>("Common/FFT/ComputeFFT_GPU"));
            normalComputeMaterial = new Material(Shader.Find("KriptoFX/Water/ComputeNormal"));
            _currentSize = GetCurrentSizeRelativeToLod();
            var anisoLevel = _waterInstance.Settings.UseAnisotropicFiltering ? KWS_Settings.Water.MaxNormalsAnisoLevel : 0;

            var mipLevels = Mathf.RoundToInt(Mathf.Log(_currentSize, 2));
            texButterfly = new Texture2D(_currentSize, mipLevels, TextureFormat.RGBAFloat, false, true);
            spectrumInitRenderTexture = new RenderTexture(_currentSize, _currentSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            spectrumInitRenderTexture.enableRandomWrite = true;
            spectrumInitRenderTexture.Create();

            spectrumHeight = new RenderTexture(_currentSize, _currentSize, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
            spectrumHeight.enableRandomWrite = true;
            spectrumHeight.Create();

            spectrumDisplaceX = new RenderTexture(_currentSize, _currentSize, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
            spectrumDisplaceX.enableRandomWrite = true;
            spectrumDisplaceX.Create();

            spectrumDisplaceZ = new RenderTexture(_currentSize, _currentSize, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
            spectrumDisplaceZ.enableRandomWrite = true;
            spectrumDisplaceZ.Create();

            fftTemp1 = new RenderTexture(_currentSize, _currentSize, 0, RenderTextureFormat.RGFloat);
            fftTemp1.enableRandomWrite = true;
            fftTemp1.Create();

            fftTemp2 = new RenderTexture(_currentSize, _currentSize, 0, RenderTextureFormat.RGFloat);
            fftTemp2.enableRandomWrite = true;
            fftTemp2.Create();

            fftTemp3 = new RenderTexture(_currentSize, _currentSize, 0, RenderTextureFormat.RGFloat);
            fftTemp3.enableRandomWrite = true;
            fftTemp3.Create();

            DisplaceTexture = new RenderTexture(_currentSize, _currentSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            DisplaceTexture.enableRandomWrite = true;
            DisplaceTexture.Create();

            NormalTexture = new RenderTexture(_currentSize, _currentSize, 0, GraphicsFormat.R16G16B16A16_SFloat, mipLevels - 1);
            NormalTexture.filterMode = FilterMode.Trilinear;
            NormalTexture.wrapMode = TextureWrapMode.Repeat;
            NormalTexture.useMipMap = true;
            NormalTexture.anisoLevel = anisoLevel;

            _waterInstance.SharedData.FftDisplacements[_lodIdx] = DisplaceTexture;
            _waterInstance.SharedData.FftNormals[_lodIdx]       = NormalTexture;
            _waterInstance.SharedData.DomainSizes[_lodIdx] = KWS_Settings.Water.FftDomainSize[_lodIdx];
            _waterInstance.SharedData.FFTActiveLods = _waterInstance.UseMultipleSimulations ? 3 : 1;
            InitializeButterfly(_currentSize);
            
            //KW_Extensions.WaterLog(this, DisplaceTexture, NormalTexture);
        }

        public void ComputeFFT(float windTurbulence, float windSpeed, float windRotation, float timeOffset)
        {
            if (!_isWaterHandlerInitialized)
            {
                _isWaterHandlerInitialized         =  true;
                WaterSystem.OnWaterSettingsChanged += OnWaterSettingsChanged;
            }
            if (Mathf.Abs(prevWindTurbulence - windTurbulence) > 0.001f || Mathf.Abs(prevWindSpeed - windSpeed) > 0.01f || Mathf.Abs(prevWindRotation - windRotation) > 0.01f)
            {
                prevWindTurbulence = windTurbulence;
                prevWindSpeed = windSpeed;
                prevWindRotation = windRotation;
                InitializeSpectrum(windSpeed, windTurbulence, windRotation);
            }

            UpdateSpectrum(timeOffset);
            DispatchFFT(windSpeed);

            _waterInstance.SharedData.FftDisplacements[_lodIdx] = DisplaceTexture;
            _waterInstance.SharedData.FftNormals[_lodIdx] = NormalTexture;
            _waterInstance.SharedData.DomainSizes[_lodIdx] = KWS_Settings.Water.FftDomainSize[_lodIdx];
            _waterInstance.SharedData.FFTActiveLods = _waterInstance.UseMultipleSimulations ? 3 : 1;
        }

        void InitializeButterfly(int size)
        {
            var log2Size = Mathf.RoundToInt(Mathf.Log(size, 2));
            butterflyColors = new Color[size * log2Size];

            int offset = 1, numIterations = size >> 1;
            for (int rowIndex = 0; rowIndex < log2Size; rowIndex++)
            {
                int rowOffset = rowIndex * size;
                {
                    int start = 0, end = 2 * offset;
                    for (int iteration = 0; iteration < numIterations; iteration++)
                    {
                        var bigK = 0.0f;
                        for (int K = start; K < end; K += 2)
                        {
                            var phase = 2.0f * Mathf.PI * bigK * numIterations / size;
                            var cos = Mathf.Cos(phase);
                            var sin = Mathf.Sin(phase);
                            butterflyColors[rowOffset + K / 2] = new Color(cos, -sin, 0, 1);
                            butterflyColors[rowOffset + K / 2 + offset] = new Color(-cos, sin, 0, 1);

                            bigK += 1.0f;
                        }
                        start += 4 * offset;
                        end = start + 2 * offset;
                    }
                }
                numIterations >>= 1;
                offset <<= 1;
            }

            texButterfly.SetPixels(butterflyColors);
            texButterfly.Apply();
        }

        void InitializeSpectrum(float windSpeed, float windTurbulence, float windRotation)
        {
            spectrumShader.SetInt("size", _currentSize);

            spectrumShader.SetFloat("domainSize", KWS_Settings.Water.FftDomainSize[_lodIdx]);
            spectrumShader.SetFloats("turbulence", windTurbulence);
            spectrumShader.SetFloat("windSpeed", windSpeed);
            spectrumShader.SetFloat("windRotation", windRotation);
            spectrumShader.SetTexture(kernelSpectrumInit, "resultInit", spectrumInitRenderTexture);
            spectrumShader.Dispatch(kernelSpectrumInit, _currentSize / 8, _currentSize / 8, 1);
        }

        void UpdateSpectrum(float timeOffset)
        {
            spectrumShader.SetFloat("time", timeOffset);
            spectrumShader.SetTexture(kernelSpectrumUpdate, "init0", spectrumInitRenderTexture);
            spectrumShader.SetTexture(kernelSpectrumUpdate, "resultHeight", spectrumHeight);
            spectrumShader.SetTexture(kernelSpectrumUpdate, "resultDisplaceX", spectrumDisplaceX);
            spectrumShader.SetTexture(kernelSpectrumUpdate, "resultDisplaceZ", spectrumDisplaceZ);

            spectrumShader.Dispatch(kernelSpectrumUpdate, _currentSize / 8, _currentSize / 8, 1);
        }

        void DispatchFFT(float windSpeed)
        {
            var temp = RenderTexture.active;

            var kernelOffset = 0;
            switch (_currentSize)
            {
                case (int)SizeSetting.Size_32:
                    kernelOffset = 0;
                    break;
                case (int)SizeSetting.Size_64:
                    kernelOffset = 2;
                    break;
                case (int)SizeSetting.Size_128:
                    kernelOffset = 4;
                    break;
                case (int)SizeSetting.Size_256:
                    kernelOffset = 6;
                    break;
                case (int)SizeSetting.Size_512:
                    kernelOffset = 8;
                    break;
            }

            shaderFFT.SetTexture(kernelOffset, "inputH", spectrumHeight);
            shaderFFT.SetTexture(kernelOffset, "inputX", spectrumDisplaceX);
            shaderFFT.SetTexture(kernelOffset, "inputZ", spectrumDisplaceZ);
            shaderFFT.SetTexture(kernelOffset, "inputButterfly", texButterfly);
            shaderFFT.SetTexture(kernelOffset, "output1", fftTemp1);
            shaderFFT.SetTexture(kernelOffset, "output2", fftTemp2);
            shaderFFT.SetTexture(kernelOffset, "output3", fftTemp3);
            shaderFFT.Dispatch(kernelOffset, 1, _currentSize, 1);

            shaderFFT.SetTexture(kernelOffset + 1, "inputH", fftTemp1);
            shaderFFT.SetTexture(kernelOffset + 1, "inputX", fftTemp2);
            shaderFFT.SetTexture(kernelOffset + 1, "inputZ", fftTemp3);
            shaderFFT.SetTexture(kernelOffset + 1, "inputButterfly", texButterfly);
            shaderFFT.SetTexture(kernelOffset + 1, "output", DisplaceTexture);
            shaderFFT.Dispatch(kernelOffset + 1, _currentSize, 1, 1);


            normalComputeMaterial.SetFloat("KW_FFTDomainSize", KWS_Settings.Water.FftDomainSize[_lodIdx]);
            normalComputeMaterial.SetTexture("_DispTex", DisplaceTexture);
            var sizeLog = Mathf.RoundToInt(Mathf.Log(_currentSize, 2)) - 4;
            normalComputeMaterial.SetFloat("_SizeLog", sizeLog);
            normalComputeMaterial.SetFloat("_WindSpeed", windSpeed);


            Graphics.SetRenderTarget(NormalTexture);
            Graphics.Blit(null, NormalTexture, normalComputeMaterial, 0);
            RenderTexture.active = temp;
        }

    }
}

