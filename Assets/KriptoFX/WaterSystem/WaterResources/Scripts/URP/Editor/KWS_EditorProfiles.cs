using System;
using System.Collections.Generic;
using static KWS.WaterSystem;

namespace KWS
{
    public static class KWS_EditorProfiles
    {
        private const float tolerance = 0.001f;

        public interface IWaterPerfomanceProfile
        {
            WaterProfileEnum GetProfile(WaterSystem                          water);
            void             SetProfile(WaterProfileEnum                     profile, WaterSystem water);
            void             ReadDataFromProfile(WaterSystem                 water);
            void             CheckDataChangesAnsSetCustomProfile(WaterSystem water);
        }

        public static class PerfomanceProfiles
        {
            public struct Reflection : IWaterPerfomanceProfile
            {
                static readonly Dictionary<WaterProfileEnum, PlanarReflectionResolutionQualityEnum> PlanarReflectionResolutionQuality = new Dictionary<WaterProfileEnum, PlanarReflectionResolutionQualityEnum>()
                {
                    {WaterProfileEnum.Ultra, PlanarReflectionResolutionQualityEnum.Ultra},
                    {WaterProfileEnum.High, PlanarReflectionResolutionQualityEnum.High},
                    {WaterProfileEnum.Medium, PlanarReflectionResolutionQualityEnum.Medium},
                    {WaterProfileEnum.Low, PlanarReflectionResolutionQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, PlanarReflectionResolutionQualityEnum.VeryLow},
                };

                static readonly Dictionary<WaterProfileEnum, ScreenSpaceReflectionResolutionQualityEnum> ScreenSpaceReflectionResolutionQuality = new Dictionary<WaterProfileEnum, ScreenSpaceReflectionResolutionQualityEnum>()
                {
                    {WaterProfileEnum.Ultra, ScreenSpaceReflectionResolutionQualityEnum.Ultra},
                    {WaterProfileEnum.High, ScreenSpaceReflectionResolutionQualityEnum.High},
                    {WaterProfileEnum.Medium, ScreenSpaceReflectionResolutionQualityEnum.Medium},
                    {WaterProfileEnum.Low, ScreenSpaceReflectionResolutionQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, ScreenSpaceReflectionResolutionQualityEnum.VeryLow},
                };

                static readonly Dictionary<WaterProfileEnum, CubemapReflectionResolutionQualityEnum> CubemapReflectionResolutionQuality = new Dictionary<WaterProfileEnum, CubemapReflectionResolutionQualityEnum>()
                {
                    {WaterProfileEnum.Ultra, CubemapReflectionResolutionQualityEnum.High},
                    {WaterProfileEnum.High, CubemapReflectionResolutionQualityEnum.High},
                    {WaterProfileEnum.Medium, CubemapReflectionResolutionQualityEnum.Medium},
                    {WaterProfileEnum.Low, CubemapReflectionResolutionQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, CubemapReflectionResolutionQualityEnum.Low},
                };

                static readonly Dictionary<WaterProfileEnum, float> CubemapUpdateInterval = new Dictionary<WaterProfileEnum, float>()
                {
                    {WaterProfileEnum.Ultra, 10},
                    {WaterProfileEnum.High, 10},
                    {WaterProfileEnum.Medium, 10},
                    {WaterProfileEnum.Low, 30},
                    {WaterProfileEnum.PotatoPC, 60},
                };

                static readonly Dictionary<WaterProfileEnum, bool> UseAnisotropicReflections = new Dictionary<WaterProfileEnum, bool>()
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, true},
                    {WaterProfileEnum.Medium, true},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };

                static readonly Dictionary<WaterProfileEnum, bool> AnisotropicReflectionsHighQuality = new Dictionary<WaterProfileEnum, bool>()
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, false},
                    {WaterProfileEnum.Medium, false},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };

                static readonly Dictionary<WaterProfileEnum, float> AnisotropicReflectionsScale = new Dictionary<WaterProfileEnum, float>()
                {
                    {WaterProfileEnum.Ultra, 0.85f},
                    {WaterProfileEnum.High, 0.55f},
                    {WaterProfileEnum.Medium, 0.4f},
                    {WaterProfileEnum.Low, 0.4f},
                    {WaterProfileEnum.PotatoPC, 0.4f},
                };

                static readonly Dictionary<WaterProfileEnum, float> ReflectionClipPlaneOffset = new Dictionary<WaterProfileEnum, float>()
                {
                    {WaterProfileEnum.Ultra, 0.01f},
                    {WaterProfileEnum.High, 0.0085f},
                    {WaterProfileEnum.Medium, 0.0065f},
                    {WaterProfileEnum.Low, 0.0065f},
                    {WaterProfileEnum.PotatoPC, 0.0065f},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.ReflectionProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.ReflectionProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.ReflectionProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.PlanarReflectionResolutionQuality      = PlanarReflectionResolutionQuality[currentProfile];
                        water.Settings.ScreenSpaceReflectionResolutionQuality = ScreenSpaceReflectionResolutionQuality[currentProfile];
                        water.Settings.CubemapReflectionResolutionQuality     = CubemapReflectionResolutionQuality[currentProfile];
                        water.Settings.CubemapUpdateInterval                  = CubemapUpdateInterval[currentProfile];
                        water.Settings.UseAnisotropicReflections              = UseAnisotropicReflections[currentProfile];
                        water.Settings.AnisotropicReflectionsHighQuality      = AnisotropicReflectionsHighQuality[currentProfile];
                        water.Settings.AnisotropicReflectionsScale            = AnisotropicReflectionsScale[currentProfile];
                        water.Settings.ReflectionClipPlaneOffset              = ReflectionClipPlaneOffset[currentProfile];
                    }
                }


                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.ReflectionProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.PlanarReflectionResolutionQuality                                                        != PlanarReflectionResolutionQuality[currentProfile]) isChanged      = true;
                        else if (water.Settings.ScreenSpaceReflectionResolutionQuality                                              != ScreenSpaceReflectionResolutionQuality[currentProfile]) isChanged = true;
                        else if (water.Settings.CubemapReflectionResolutionQuality                                                  != CubemapReflectionResolutionQuality[currentProfile]) isChanged     = true;
                        else if (Math.Abs(water.Settings.CubemapUpdateInterval - CubemapUpdateInterval[currentProfile])             > tolerance) isChanged                                               = true;
                        else if (water.Settings.UseAnisotropicReflections                                                           != UseAnisotropicReflections[currentProfile]) isChanged              = true;
                        else if (water.Settings.AnisotropicReflectionsHighQuality                                                   != AnisotropicReflectionsHighQuality[currentProfile]) isChanged      = true;
                        else if (Math.Abs(water.Settings.AnisotropicReflectionsScale - AnisotropicReflectionsScale[currentProfile]) > tolerance) isChanged                                               = true;
                        else if (Math.Abs(water.Settings.ReflectionClipPlaneOffset   - ReflectionClipPlaneOffset[currentProfile])   > tolerance) isChanged                                               = true;

                        if (isChanged) water.Settings.ReflectionProfile = WaterProfileEnum.Custom;
                    }
                }

            }

            public struct ColorRerfraction : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, RefractionModeEnum> RefractionMode = new Dictionary<WaterProfileEnum, RefractionModeEnum>
                {
                    {WaterProfileEnum.Ultra, RefractionModeEnum.PhysicalAproximationIOR},
                    {WaterProfileEnum.High, RefractionModeEnum.PhysicalAproximationIOR},
                    {WaterProfileEnum.Medium, RefractionModeEnum.PhysicalAproximationIOR},
                    {WaterProfileEnum.Low, RefractionModeEnum.Simple},
                    {WaterProfileEnum.PotatoPC, RefractionModeEnum.Simple}
                };

                public static readonly Dictionary<WaterProfileEnum, bool> UseRefractionDispersion = new Dictionary<WaterProfileEnum, bool>()
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, true},
                    {WaterProfileEnum.Medium, false},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.RefractionProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.RefractionProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.RefractionProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.RefractionMode          = RefractionMode[currentProfile];
                        water.Settings.UseRefractionDispersion = UseRefractionDispersion[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.RefractionProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.RefractionMode               != RefractionMode[currentProfile]) isChanged          = true;
                        else if (water.Settings.UseRefractionDispersion != UseRefractionDispersion[currentProfile]) isChanged = true;

                        if (isChanged) water.Settings.RefractionProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct Flowing : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, FlowmapTextureResolutionEnum> FlowMapTextureResolution = new Dictionary<WaterProfileEnum, FlowmapTextureResolutionEnum>()
                {
                    {WaterProfileEnum.Ultra, FlowmapTextureResolutionEnum._4096},
                    {WaterProfileEnum.High, FlowmapTextureResolutionEnum._4096},
                    {WaterProfileEnum.Medium, FlowmapTextureResolutionEnum._2048},
                    {WaterProfileEnum.Low, FlowmapTextureResolutionEnum._1024},
                    {WaterProfileEnum.PotatoPC, FlowmapTextureResolutionEnum._512},
                };

                public static readonly Dictionary<WaterProfileEnum, int> FluidsSimulationIterrations = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 3},
                    {WaterProfileEnum.High, 2},
                    {WaterProfileEnum.Medium, 2},
                    {WaterProfileEnum.Low, 2},
                    {WaterProfileEnum.PotatoPC, 2},
                };

                public static readonly Dictionary<WaterProfileEnum, int> FluidsTextureSize = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 2048},
                    {WaterProfileEnum.High, 1536},
                    {WaterProfileEnum.Medium, 1024},
                    {WaterProfileEnum.Low, 768},
                    {WaterProfileEnum.PotatoPC, 512},
                };

                public static readonly Dictionary<WaterProfileEnum, int> FluidsAreaSize = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 45},
                    {WaterProfileEnum.High, 35},
                    {WaterProfileEnum.Medium, 25},
                    {WaterProfileEnum.Low, 20},
                    {WaterProfileEnum.PotatoPC, 15},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.FlowmapProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.FlowmapProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.FlowmapProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.FlowMapTextureResolution = FlowMapTextureResolution[currentProfile];
                        water.Settings.FluidsSimulationIterrations = FluidsSimulationIterrations[currentProfile];
                        water.Settings.FluidsTextureSize           = FluidsTextureSize[currentProfile];
                        water.Settings.FluidsAreaSize              = FluidsAreaSize[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.FlowmapProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.FlowMapTextureResolution                  != FlowMapTextureResolution[currentProfile]) isChanged    = true;
                        else if (water.Settings.FluidsSimulationIterrations != FluidsSimulationIterrations[currentProfile]) isChanged = true;
                        else if (water.Settings.FluidsTextureSize           != FluidsTextureSize[currentProfile]) isChanged           = true;
                        else if (water.Settings.FluidsAreaSize              != FluidsAreaSize[currentProfile]) isChanged              = true;

                        if (isChanged) water.Settings.FlowmapProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct DynamicWaves : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, int> DynamicWavesAreaSize = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 60},
                    {WaterProfileEnum.High, 50},
                    {WaterProfileEnum.Medium, 40},
                    {WaterProfileEnum.Low, 30},
                    {WaterProfileEnum.PotatoPC, 20},
                };

                public static readonly Dictionary<WaterProfileEnum, int> DynamicWavesResolutionPerMeter = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 34},
                    {WaterProfileEnum.High, 34},
                    {WaterProfileEnum.Medium, 34},
                    {WaterProfileEnum.Low, 25},
                    {WaterProfileEnum.PotatoPC, 20},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.DynamicWavesProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.DynamicWavesProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.DynamicWavesProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.DynamicWavesAreaSize           = DynamicWavesAreaSize[currentProfile];
                        water.Settings.DynamicWavesResolutionPerMeter = DynamicWavesResolutionPerMeter[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.DynamicWavesProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.DynamicWavesAreaSize                != DynamicWavesAreaSize[currentProfile]) isChanged           = true;
                        else if (water.Settings.DynamicWavesResolutionPerMeter != DynamicWavesResolutionPerMeter[currentProfile]) isChanged = true;

                        if (isChanged) water.Settings.DynamicWavesProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct Shoreline : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, ShorelineFoamQualityEnum> FoamLodQuality = new Dictionary<WaterProfileEnum, ShorelineFoamQualityEnum>
                {
                    {WaterProfileEnum.Ultra, ShorelineFoamQualityEnum.High},
                    {WaterProfileEnum.High, ShorelineFoamQualityEnum.High},
                    {WaterProfileEnum.Medium, ShorelineFoamQualityEnum.Medium},
                    {WaterProfileEnum.Low, ShorelineFoamQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, ShorelineFoamQualityEnum.VeryLow},
                };

                public static readonly Dictionary<WaterProfileEnum, bool> UseFastMode = new Dictionary<WaterProfileEnum, bool>
                {
                    {WaterProfileEnum.Ultra, false},
                    {WaterProfileEnum.High, false},
                    {WaterProfileEnum.Medium, false},
                    {WaterProfileEnum.Low, true},
                    {WaterProfileEnum.PotatoPC, true},
                };

                public static readonly Dictionary<WaterProfileEnum, bool> FoamReceiveShadows = new Dictionary<WaterProfileEnum, bool>
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, true},
                    {WaterProfileEnum.Medium, true},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.ShorelineProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.ShorelineProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.ShorelineProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.ShorelineFoamLodQuality        = FoamLodQuality[currentProfile];
                        water.Settings.UseShorelineFoamFastMode       = UseFastMode[currentProfile];
                        water.Settings.ShorelineFoamReceiveDirShadows = FoamReceiveShadows[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.ShorelineProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.ShorelineFoamLodQuality             != FoamLodQuality[currentProfile]) isChanged     = true;
                        else if (water.Settings.UseShorelineFoamFastMode       != UseFastMode[currentProfile]) isChanged        = true;
                        else if (water.Settings.ShorelineFoamReceiveDirShadows != FoamReceiveShadows[currentProfile]) isChanged = true;

                        if (isChanged) water.Settings.ShorelineProfile = WaterProfileEnum.Custom;
                    }
                }
            }
            public struct Foam : IWaterPerfomanceProfile
            {
                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.ShorelineProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.ShorelineProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.ShorelineProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {

                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.ShorelineProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        //var isChanged = false;


                        //if (isChanged) water.Settings.ShorelineProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct VolumetricLight : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, VolumetricLightResolutionQualityEnum> VolumetricLightResolutionQuality = new Dictionary<WaterProfileEnum, VolumetricLightResolutionQualityEnum>()
                {
                    {WaterProfileEnum.Ultra, VolumetricLightResolutionQualityEnum.Ultra},
                    {WaterProfileEnum.High, VolumetricLightResolutionQualityEnum.High},
                    {WaterProfileEnum.Medium, VolumetricLightResolutionQualityEnum.Medium},
                    {WaterProfileEnum.Low, VolumetricLightResolutionQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, VolumetricLightResolutionQualityEnum.VeryLow},
                };

                public static readonly Dictionary<WaterProfileEnum, int> VolumetricLightIteration = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 8},
                    {WaterProfileEnum.High, 6},
                    {WaterProfileEnum.Medium, 4},
                    {WaterProfileEnum.Low, 3},
                    {WaterProfileEnum.PotatoPC, 2},
                };

                public static readonly Dictionary<WaterProfileEnum, VolumetricLightFilterEnum> VolumetricLightFilter = new Dictionary<WaterProfileEnum, VolumetricLightFilterEnum>()
                {
                    {WaterProfileEnum.Ultra, VolumetricLightFilterEnum.Bilateral},
                    {WaterProfileEnum.High, VolumetricLightFilterEnum.Bilateral},
                    {WaterProfileEnum.Medium, VolumetricLightFilterEnum.Bilateral},
                    {WaterProfileEnum.Low, VolumetricLightFilterEnum.Gaussian},
                    {WaterProfileEnum.PotatoPC, VolumetricLightFilterEnum.Gaussian},
                };

                public static readonly Dictionary<WaterProfileEnum, float> VolumetricLightBlurRadius = new Dictionary<WaterProfileEnum, float>()
                {
                    {WaterProfileEnum.Ultra, 1f},
                    {WaterProfileEnum.High, 1f},
                    {WaterProfileEnum.Medium, 1f},
                    {WaterProfileEnum.Low, 2.5f},
                    {WaterProfileEnum.PotatoPC, 3.5f},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.VolumetricLightProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.VolumetricLightProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.VolumetricLightProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.VolumetricLightResolutionQuality = VolumetricLightResolutionQuality[currentProfile];
                        water.Settings.VolumetricLightIteration         = VolumetricLightIteration[currentProfile];
                        water.Settings.VolumetricLightFilter            = VolumetricLightFilter[currentProfile];
                        water.Settings.VolumetricLightBlurRadius        = VolumetricLightBlurRadius[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.VolumetricLightProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.VolumetricLightResolutionQuality                                                     != VolumetricLightResolutionQuality[currentProfile]) isChanged = true;
                        else if (water.Settings.VolumetricLightIteration                                                        != VolumetricLightIteration[currentProfile]) isChanged         = true;
                        else if (water.Settings.VolumetricLightFilter                                                           != VolumetricLightFilter[currentProfile]) isChanged            = true;
                        else if (Math.Abs(water.Settings.VolumetricLightBlurRadius - VolumetricLightBlurRadius[currentProfile]) > tolerance) isChanged                                         = true;

                        if (isChanged) water.Settings.VolumetricLightProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct Caustic : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, bool> UseCausticBicubicInterpolation = new Dictionary<WaterProfileEnum, bool>()
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, false},
                    {WaterProfileEnum.Medium, false},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };

                public static readonly Dictionary<WaterProfileEnum, bool> UseCausticDispersion = new Dictionary<WaterProfileEnum, bool>()
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, true},
                    {WaterProfileEnum.Medium, false},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };

                public static readonly Dictionary<WaterProfileEnum, int> CausticTextureSize = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 1024},
                    {WaterProfileEnum.High, 768},
                    {WaterProfileEnum.Medium, 512},
                    {WaterProfileEnum.Low, 384},
                    {WaterProfileEnum.PotatoPC, 256},
                };

                public static readonly Dictionary<WaterProfileEnum, int> CausticMeshResolution = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 384},
                    {WaterProfileEnum.High, 320},
                    {WaterProfileEnum.Medium, 256},
                    {WaterProfileEnum.Low, 192},
                    {WaterProfileEnum.PotatoPC, 128},
                };

                public static readonly Dictionary<WaterProfileEnum, int> CausticActiveLods = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 4},
                    {WaterProfileEnum.High, 4},
                    {WaterProfileEnum.Medium, 3},
                    {WaterProfileEnum.Low, 2},
                    {WaterProfileEnum.PotatoPC, 1},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.CausticProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.CausticProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.CausticProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.UseCausticBicubicInterpolation = UseCausticBicubicInterpolation[currentProfile];
                        water.Settings.UseCausticDispersion           = UseCausticDispersion[currentProfile];
                        water.Settings.CausticTextureSize             = CausticTextureSize[currentProfile];
                        water.Settings.CausticMeshResolution          = CausticMeshResolution[currentProfile];
                        water.Settings.CausticActiveLods              = CausticActiveLods[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.CausticProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.UseCausticBicubicInterpolation != UseCausticBicubicInterpolation[currentProfile]) isChanged = true;
                        else if (water.Settings.UseCausticDispersion      != UseCausticDispersion[currentProfile]) isChanged           = true;
                        else if (water.Settings.CausticTextureSize        != CausticTextureSize[currentProfile]) isChanged             = true;
                        else if (water.Settings.CausticMeshResolution     != CausticMeshResolution[currentProfile]) isChanged          = true;
                        else if (water.Settings.CausticActiveLods         != CausticActiveLods[currentProfile]) isChanged              = true;

                        if (isChanged) water.Settings.CausticProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct Mesh : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, WaterMeshQualityEnum> MeshQualityInfinite = new Dictionary<WaterProfileEnum, WaterMeshQualityEnum>()
                {
                    {WaterProfileEnum.Ultra, WaterMeshQualityEnum.Ultra},
                    {WaterProfileEnum.High, WaterMeshQualityEnum.High},
                    {WaterProfileEnum.Medium, WaterMeshQualityEnum.Medium},
                    {WaterProfileEnum.Low, WaterMeshQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, WaterMeshQualityEnum.VeryLow},
                };

                public static readonly Dictionary<WaterProfileEnum, int> OceanDetailingFarDistance = new Dictionary<WaterProfileEnum, int>()
                {
                    {WaterProfileEnum.Ultra, 8000},
                    {WaterProfileEnum.High, 6000},
                    {WaterProfileEnum.Medium, 4000},
                    {WaterProfileEnum.Low, 2000},
                    {WaterProfileEnum.PotatoPC, 1000},
                };

                public static readonly Dictionary<WaterProfileEnum, WaterMeshQualityEnum> MeshQualityFinite = new Dictionary<WaterProfileEnum, WaterMeshQualityEnum>()
                {
                    {WaterProfileEnum.Ultra, WaterMeshQualityEnum.Ultra},
                    {WaterProfileEnum.High, WaterMeshQualityEnum.High},
                    {WaterProfileEnum.Medium, WaterMeshQualityEnum.Medium},
                    {WaterProfileEnum.Low, WaterMeshQualityEnum.Low},
                    {WaterProfileEnum.PotatoPC, WaterMeshQualityEnum.VeryLow},
                };

                public static readonly Dictionary<WaterProfileEnum, float> TesselationFactor = new Dictionary<WaterProfileEnum, float>()
                {
                    {WaterProfileEnum.Ultra, 1.0f},
                    {WaterProfileEnum.High, 0.75f},
                    {WaterProfileEnum.Medium, 0.5f},
                    {WaterProfileEnum.Low, 0.25f},
                    {WaterProfileEnum.PotatoPC, 0.15f},
                };

                public static readonly Dictionary<WaterProfileEnum, float> TesselationInfiniteMeshMaxDistance = new Dictionary<WaterProfileEnum, float>()
                {
                    {WaterProfileEnum.Ultra, 5000},
                    {WaterProfileEnum.High, 2000},
                    {WaterProfileEnum.Medium, 1000},
                    {WaterProfileEnum.Low, 500},
                    {WaterProfileEnum.PotatoPC, 200},
                };

                public static readonly Dictionary<WaterProfileEnum, float> TesselationOtherMeshMaxDistance = new Dictionary<WaterProfileEnum, float>()
                {
                    {WaterProfileEnum.Ultra, 150},
                    {WaterProfileEnum.High, 100},
                    {WaterProfileEnum.Medium, 50},
                    {WaterProfileEnum.Low, 25},
                    {WaterProfileEnum.PotatoPC, 10},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.MeshProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.MeshProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.MeshProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.WaterMeshQualityInfinite           = MeshQualityInfinite[currentProfile];
                        water.Settings.OceanDetailingFarDistance          = OceanDetailingFarDistance[currentProfile];
                        water.Settings.WaterMeshQualityFinite             = MeshQualityFinite[currentProfile];
                        water.Settings.TesselationFactor                  = TesselationFactor[currentProfile];
                        water.Settings.TesselationInfiniteMeshMaxDistance = TesselationInfiniteMeshMaxDistance[currentProfile];
                        water.Settings.TesselationOtherMeshMaxDistance    = TesselationOtherMeshMaxDistance[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.MeshProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.WaterMeshQualityInfinite                                                                               != MeshQualityInfinite[currentProfile]) isChanged       = true;
                        else if (water.Settings.OceanDetailingFarDistance                                                                         != OceanDetailingFarDistance[currentProfile]) isChanged = true;
                        else if (water.Settings.WaterMeshQualityFinite                                                                            != MeshQualityFinite[currentProfile]) isChanged         = true;
                        else if (Math.Abs(water.Settings.TesselationFactor                  - TesselationFactor[currentProfile])                  > tolerance) isChanged                                  = true;
                        else if (Math.Abs(water.Settings.TesselationInfiniteMeshMaxDistance - TesselationInfiniteMeshMaxDistance[currentProfile]) > tolerance) isChanged                                  = true;
                        else if (Math.Abs(water.Settings.TesselationOtherMeshMaxDistance    - TesselationOtherMeshMaxDistance[currentProfile])    > tolerance) isChanged                                  = true;

                        if (isChanged) water.Settings.MeshProfile = WaterProfileEnum.Custom;
                    }
                }
            }

            public struct Rendering : IWaterPerfomanceProfile
            {
                public static readonly Dictionary<WaterProfileEnum, bool> UseFiltering = new Dictionary<WaterProfileEnum, bool>()
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, true},
                    {WaterProfileEnum.Medium, true},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };

                public static readonly Dictionary<WaterProfileEnum, bool> UseAnisotropicFiltering = new Dictionary<WaterProfileEnum, bool>()
                {
                    {WaterProfileEnum.Ultra, true},
                    {WaterProfileEnum.High, false},
                    {WaterProfileEnum.Medium, false},
                    {WaterProfileEnum.Low, false},
                    {WaterProfileEnum.PotatoPC, false},
                };

                public WaterProfileEnum GetProfile(WaterSystem water)
                {
                    return water.Settings.RenderingProfile;
                }

                public void SetProfile(WaterProfileEnum profile, WaterSystem water)
                {
                    water.Settings.RenderingProfile = profile;
                }

                public void ReadDataFromProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.RenderingProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        water.Settings.UseFiltering            = UseFiltering[currentProfile];
                        water.Settings.UseAnisotropicFiltering = UseAnisotropicFiltering[currentProfile];
                    }
                }

                public void CheckDataChangesAnsSetCustomProfile(WaterSystem water)
                {
                    var currentProfile = water.Settings.RenderingProfile;
                    if (currentProfile != WaterProfileEnum.Custom)
                    {
                        var isChanged = false;

                        if (water.Settings.UseFiltering                 != UseFiltering[currentProfile]) isChanged            = true;
                        else if (water.Settings.UseAnisotropicFiltering != UseAnisotropicFiltering[currentProfile]) isChanged = true;

                        if (isChanged) water.Settings.RenderingProfile = WaterProfileEnum.Custom;
                    }
                }
            }
        }
    }
}