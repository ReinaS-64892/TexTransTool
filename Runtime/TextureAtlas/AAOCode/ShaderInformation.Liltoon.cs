#nullable enable
using System.Linq;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.AAOCode
{
    internal class LiltoonShaderInformation
    {
        public void GetMaterialInformation(IMaterialInformationCallbackAbstractionInterface matInfo, bool isOutline)
        {
            var uvMain = UsingUVChannels.UV0;
            var uvMainScaleOffset = "_MainTex_ST";
            Matrix2x3? uvMainMatrix = ComputeUVMainMatrix();

            Matrix2x3? ComputeUVMainMatrix()
            {
                // _ShiftBackfaceUV
                if (matInfo.GetFloat("_ShiftBackfaceUV") != 0) return null; // changed depends on face
                return STAndScrollRotateToMatrix("_MainTex_ST", "_MainTex_ScrollRotate");
            }

            #region Default.lilblock / DefaultALL.lilblock
            matInfo.RegisterTextureUVUsage("_DitherTex", SamplerStateInformation.LinearRepeatSampler,
                UsingUVChannels.NonMesh, null); // dither UV is based on screen space

            // TODO: _MainTex with POM / PARALLAX (using LIL_SAMPLE_2D_POM)
            LIL_SAMPLE_2D_WithMat("_MainTex", "_MainTex", uvMain, uvMainMatrix); // main texture
                                                                                 // dummy properties set by liltoon
                                                                                 // TODO: consider remove _BaseMap and _BaseColorMap from material on build since they are unused
            LIL_SAMPLE_2D_WithMat("_BaseMap", "_MainTex", uvMain, uvMainMatrix);
            LIL_SAMPLE_2D_WithMat("_BaseColorMap", "_MainTex", uvMain, uvMainMatrix);
            matInfo.RegisterTextureUVUsage("_MainGradationTex", SamplerStateInformation.LinearClampSampler,
                UsingUVChannels.NonMesh, null); // GradationMap UV is based on color
            LIL_SAMPLE_2D_WithMat("_MainColorAdjustMask", "_MainTex", uvMain, uvMainMatrix); // simple LIL_SAMPLE_2D

            if (matInfo.GetInt("_UseMain2ndTex") != 0)
            {
                // caller of lilGetMain2nd will pass sampler for _MainTex as samp
                SamplerStateInformation samp = "_MainTex";

                UsingUVChannels uv2nd;
                switch (matInfo.GetInt("_Main2ndTex_UVMode"))
                {
                    case 0:
                        uv2nd = UsingUVChannels.UV0;
                        break;
                    case 1:
                        uv2nd = UsingUVChannels.UV1;
                        break;
                    case 2:
                        uv2nd = UsingUVChannels.UV2;
                        break;
                    case 3:
                        uv2nd = UsingUVChannels.UV3;
                        break;
                    case 4:
                        uv2nd = UsingUVChannels.NonMesh;
                        break; // MatCap (normal-based UV)
                    default:
                        uv2nd = UsingUVChannels.UV0 | UsingUVChannels.UV1 | UsingUVChannels.UV2 | UsingUVChannels.UV3;
                        break;
                }

                LIL_GET_SUBTEX("_Main2ndTex", uv2nd);
                LIL_SAMPLE_2D_WithMat("_Main2ndBlendMask", samp, uvMain, uvMainMatrix);
                lilCalcDissolveWithOrWithoutNoise(
                    UsingUVChannels.UV0,
                    "_Main2ndDissolveMask",
                    "_Main2ndDissolveMask_ST",
                    "_Main2ndDissolveNoiseMask",
                    "_Main2ndDissolveNoiseMask_ST",
                    "_Main2ndDissolveNoiseMask_ScrollRotate",
                    samp
                );
            }

            if (matInfo.GetInt("_UseMain3rdTex") != 0)
            {
                // caller of lilGetMain3rd will pass sampler for _MainTex as samp
                var samp = "_MainTex";

                UsingUVChannels uv3rd;
                switch (matInfo.GetInt("_Main2ndTex_UVMode"))
                {
                    case 0:
                        uv3rd = UsingUVChannels.UV0;
                        break;
                    case 1:
                        uv3rd = UsingUVChannels.UV1;
                        break;
                    case 2:
                        uv3rd = UsingUVChannels.UV2;
                        break;
                    case 3:
                        uv3rd = UsingUVChannels.UV3;
                        break;
                    case 4:
                        uv3rd = UsingUVChannels.NonMesh;
                        break; // MatCap (normal-based UV)
                    default:
                        uv3rd = UsingUVChannels.UV0 | UsingUVChannels.UV1 | UsingUVChannels.UV2 | UsingUVChannels.UV3;
                        break;
                }

                LIL_GET_SUBTEX("_Main3rdTex", uv3rd);
                LIL_SAMPLE_2D_WithMat("_Main3rdBlendMask", samp, uvMain, uvMainMatrix);
                lilCalcDissolveWithOrWithoutNoise(
                    UsingUVChannels.UV0,
                    "_Main3rdDissolveMask",
                    "_Main3rdDissolveMask_ST",
                    "_Main3rdDissolveNoiseMask",
                    "_Main3rdDissolveNoiseMask_ST",
                    "_Main3rdDissolveNoiseMask_ScrollRotate",
                    samp
                );
            }

            LIL_SAMPLE_2D_ST_WithMat("_AlphaMask", "_MainTex", uvMain, uvMainMatrix);
            if (matInfo.GetInt("_UseBumpMap") != 0)
            {
                LIL_SAMPLE_2D_ST_WithMat("_BumpMap", "_MainTex", uvMain, uvMainMatrix);
            }

            if (matInfo.GetInt("_UseBump2ndMap") != 0)
            {
                var uvBump2nd = UsingUVChannels.UV0;

                switch (matInfo.GetInt("_Bump2ndMap_UVMode"))
                {
                    case 0:
                        uvBump2nd = UsingUVChannels.UV0;
                        break;
                    case 1:
                        uvBump2nd = UsingUVChannels.UV1;
                        break;
                    case 2:
                        uvBump2nd = UsingUVChannels.UV2;
                        break;
                    case 3:
                        uvBump2nd = UsingUVChannels.UV3;
                        break;
                    case null:
                        uvBump2nd = UsingUVChannels.UV0 | UsingUVChannels.UV1 | UsingUVChannels.UV2 | UsingUVChannels.UV3;
                        break;
                }

                LIL_SAMPLE_2D_ST("_Bump2ndMap", SamplerStateInformation.LinearRepeatSampler, uvBump2nd);
                LIL_SAMPLE_2D_ST_WithMat("_Bump2ndScaleMask", "_MainTex", uvMain, uvMainMatrix);

                // Note: _Bump2ndScaleMask is defined as NoScaleOffset but sampled with LIL_SAMPLE_2D_ST?
            }

            if (matInfo.GetInt("_UseAnisotropy") != 0)
            {
                LIL_SAMPLE_2D_ST_WithMat("_AnisotropyTangentMap", "_MainTex", uvMain, uvMainMatrix);
                LIL_SAMPLE_2D_ST_WithMat("_AnisotropyScaleMask", "_MainTex", uvMain, uvMainMatrix);

                // _AnisotropyShiftNoiseMask is used in another place but under _UseAnisotropy condition
                LIL_SAMPLE_2D_ST_WithMat("_AnisotropyShiftNoiseMask", "_MainTex", uvMain, uvMainMatrix);
            }

            if (matInfo.GetInt("_UseBacklight") != 0)
            {
                var samp = "_MainTex";
                LIL_SAMPLE_2D_ST_WithMat("_BacklightColorTex", samp, uvMain, uvMainMatrix);
            }

            if (matInfo.GetInt("_UseShadow") != 0)
            {
                SamplerStateInformation samp = "_MainTex";
                LIL_SAMPLE_2D_GRAD_WithMat("_ShadowStrengthMask", SamplerStateInformation.LinearRepeatSampler, uvMain,
                    uvMainMatrix);
                LIL_SAMPLE_2D_GRAD_WithMat("_ShadowBorderMask", SamplerStateInformation.LinearRepeatSampler, uvMain,
                    uvMainMatrix);
                LIL_SAMPLE_2D_GRAD_WithMat("_ShadowBlurMask", SamplerStateInformation.LinearRepeatSampler, uvMain,
                    uvMainMatrix);
                // lilSampleLUT
                switch (matInfo.GetInt("_ShadowColorType"))
                {
                    case 1:
                        LIL_SAMPLE_2D_WithMat("_ShadowColorTex", SamplerStateInformation.LinearClampSampler,
                            UsingUVChannels.NonMesh, null);
                        LIL_SAMPLE_2D_WithMat("_Shadow2ndColorTex", SamplerStateInformation.LinearClampSampler,
                            UsingUVChannels.NonMesh, null);
                        LIL_SAMPLE_2D_WithMat("_Shadow3rdColorTex", SamplerStateInformation.LinearClampSampler,
                            UsingUVChannels.NonMesh, null);
                        break;
                    case null:
                        var sampler = samp | SamplerStateInformation.LinearClampSampler;
                        LIL_SAMPLE_2D_WithMat("_ShadowColorTex", sampler, UsingUVChannels.NonMesh | uvMain, null);
                        LIL_SAMPLE_2D_WithMat("_Shadow2ndColorTex", sampler, UsingUVChannels.NonMesh | uvMain, null);
                        LIL_SAMPLE_2D_WithMat("_Shadow3rdColorTex", sampler, UsingUVChannels.NonMesh | uvMain, null);
                        break;
                    default:
                        LIL_SAMPLE_2D_WithMat("_ShadowColorTex", samp, uvMain, uvMainMatrix);
                        LIL_SAMPLE_2D_WithMat("_Shadow2ndColorTex", samp, uvMain, uvMainMatrix);
                        LIL_SAMPLE_2D_WithMat("_Shadow3rdColorTex", samp, uvMain, uvMainMatrix);
                        break;
                }
            }

            if (matInfo.GetInt("_UseRimShade") != 0)
            {
                var samp = "_MainTex";

                LIL_SAMPLE_2D_WithMat("_RimShadeMask", samp, uvMain, uvMainMatrix);
            }

            if (matInfo.GetInt("_UseReflection") != 0)
            {
                // TODO: research
                var samp = "_MainTex"; // or SamplerStateInformation.LinearRepeatSampler in lil_pass_foreward_reblur.hlsl

                LIL_SAMPLE_2D_ST_WithMat("_SmoothnessTex", samp, uvMain, uvMainMatrix);
                LIL_SAMPLE_2D_ST_WithMat("_MetallicGlossMap", samp, uvMain, uvMainMatrix);
                LIL_SAMPLE_2D_ST_WithMat("_ReflectionColorTex", samp, uvMain, uvMainMatrix);
            }

            // Matcap
            if (matInfo.GetInt("_UseMatCap") != 0)
            {
                var samp = "_MainTex"; // caller of lilGetMatCap

                LIL_SAMPLE_2D("_MatCapTex", SamplerStateInformation.LinearRepeatSampler, UsingUVChannels.NonMesh);
                LIL_SAMPLE_2D_ST_WithMat("_MatCapBlendMask", samp, uvMain, uvMainMatrix);

                var matCapBlendUv1 = matInfo.GetVector("_MatCapBlendUV1");
                if (matCapBlendUv1 is null or not { x: 0, y: 0 })
                    matInfo.RegisterOtherUVUsage(UsingUVChannels.UV1);

                if (matInfo.GetInt("_MatCapCustomNormal") != 0)
                {
                    LIL_SAMPLE_2D_ST_WithMat("_MatCapBumpMap", samp, uvMain, uvMainMatrix);
                }
            }

            if (matInfo.GetInt("_UseMatCap2nd") != 0)
            {
                var samp = "_MainTex"; // caller of lilGetMatCap

                LIL_SAMPLE_2D("_MatCap2ndTex", SamplerStateInformation.LinearRepeatSampler, UsingUVChannels.NonMesh);
                LIL_SAMPLE_2D_ST_WithMat("_MatCap2ndBlendMask", samp, uvMain, uvMainMatrix);

                var matCapBlendUv1 = matInfo.GetVector("_MatCap2ndBlendUV1");
                if (matCapBlendUv1 is null or not { x: 0, y: 0 })
                    matInfo.RegisterOtherUVUsage(UsingUVChannels.UV1);

                if (matInfo.GetInt("_MatCap2ndCustomNormal") != 0)
                {
                    LIL_SAMPLE_2D_ST_WithMat("_MatCap2ndBumpMap", samp, uvMain, uvMainMatrix);
                }
            }

            // rim light
            if (matInfo.GetInt("_UseRim") != 0)
            {
                var samp = "_MainTex"; // caller of lilGetRim
                LIL_SAMPLE_2D_ST_WithMat("_RimColorTex", samp, uvMain, uvMainMatrix);
            }

            if (matInfo.GetInt("_UseGlitter") != 0)
            {
                var samp = "_MainTex"; // caller of lilGetGlitter

                LIL_SAMPLE_2D_ST_WithMat("_GlitterColorTex", samp, uvMain, uvMainMatrix);
                if (matInfo.GetInt("_GlitterApplyShape") != 0)
                {
                    // complex uv
                    LIL_SAMPLE_2D_GRAD("_GlitterShapeTex", SamplerStateInformation.LinearClampSampler,
                        UsingUVChannels.NonMesh);
                }
            }

            if (matInfo.GetInt("_UseEmission") != 0)
            {
                UsingUVChannels emissionUV = UsingUVChannels.UV0;

                switch (matInfo.GetInt("_EmissionMap_UVMode"))
                {
                    case 1:
                        emissionUV = UsingUVChannels.UV1;
                        break;
                    case 2:
                        emissionUV = UsingUVChannels.UV2;
                        break;
                    case 3:
                        emissionUV = UsingUVChannels.UV3;
                        break;
                    case 4:
                        emissionUV = UsingUVChannels.NonMesh;
                        break; // uvRim; TODO: check
                    case null:
                        emissionUV = UsingUVChannels.UV0 | UsingUVChannels.UV1 | UsingUVChannels.UV2 | UsingUVChannels.UV3 |
                                     UsingUVChannels.NonMesh;
                        break;
                }

                var parallaxEnabled = matInfo.GetFloat("_EmissionParallaxDepth") != 0;

                LIL_GET_EMITEX("_EmissionMap", emissionUV, parallaxEnabled);

                // if LIL_FEATURE_ANIMATE_EMISSION_MASK_UV is enabled, UV0 is used and if not UVMain is used.
                var LIL_FEATURE_ANIMATE_EMISSION_MASK_UV =
                    matInfo.GetVector("_EmissionBlendMask_ScrollRotate") != new Vector4(0, 0, 0, 0) ||
                    matInfo.GetVector("_Emission2ndBlendMask_ScrollRotate") != new Vector4(0, 0, 0, 0);

                if (LIL_FEATURE_ANIMATE_EMISSION_MASK_UV)
                {
                    LIL_GET_EMIMASK("_EmissionBlendMask", UsingUVChannels.UV0);
                }
                else
                {
                    LIL_GET_EMIMASK_WithMat("_EmissionBlendMask", uvMain, uvMainMatrix);
                }

                if (matInfo.GetInt("_EmissionUseGrad") != 0)
                {
                    LIL_SAMPLE_1D("_EmissionGradTex", SamplerStateInformation.LinearRepeatSampler, UsingUVChannels.NonMesh);
                }
            }

            if (matInfo.GetInt("_UseEmission2nd") != 0)
            {
                UsingUVChannels emission2ndUV = UsingUVChannels.UV0;

                switch (matInfo.GetInt("_Emission2ndMap_UVMode"))
                {
                    case 1:
                        emission2ndUV = UsingUVChannels.UV1;
                        break;
                    case 2:
                        emission2ndUV = UsingUVChannels.UV2;
                        break;
                    case 3:
                        emission2ndUV = UsingUVChannels.UV3;
                        break;
                    case 4:
                        emission2ndUV = UsingUVChannels.NonMesh;
                        break; // uvRim; TODO: check
                    case null:
                        emission2ndUV = UsingUVChannels.UV0 | UsingUVChannels.UV1 | UsingUVChannels.UV2 |
                                        UsingUVChannels.UV3 | UsingUVChannels.NonMesh;
                        break;
                }

                var parallaxEnabled = matInfo.GetFloat("_Emission2ndParallaxDepth") != 0;

                // actually LIL_GET_EMITEX is used but same as LIL_SAMPLE_2D_ST
                LIL_GET_EMITEX("_Emission2ndMap", emission2ndUV, parallaxEnabled);

                // if LIL_FEATURE_ANIMATE_EMISSION_MASK_UV is enabled, UV0 is used and if not UVMain is used. (weird)
                // https://github.com/lilxyzw/lilToon/blob/b96470d3dd9092b840052578048b2307fe6d8786/Assets/lilToon/Shader/Includes/lil_common_frag.hlsl#L1819-L1821
                var LIL_FEATURE_ANIMATE_EMISSION_MASK_UV =
                    matInfo.GetVector("_EmissionBlendMask_ScrollRotate") != new Vector4(0, 0, 0, 0) ||
                    matInfo.GetVector("_Emission2ndBlendMask_ScrollRotate") != new Vector4(0, 0, 0, 0);

                if (LIL_FEATURE_ANIMATE_EMISSION_MASK_UV)
                {
                    LIL_GET_EMIMASK("_Emission2ndBlendMask", UsingUVChannels.UV0);
                }
                else
                {
                    LIL_GET_EMIMASK_WithMat("_Emission2ndBlendMask", uvMain, uvMainMatrix);
                }

                if (matInfo.GetInt("_Emission2ndUseGrad") != 0)
                {
                    LIL_SAMPLE_1D("_Emission2ndGradTex", SamplerStateInformation.LinearRepeatSampler,
                        UsingUVChannels.NonMesh);
                }
            }

            if (matInfo.GetInt("_UseParallax") != 0)
            {
                matInfo.RegisterTextureUVUsage("_ParallaxMap", SamplerStateInformation.LinearRepeatSampler,
                    UsingUVChannels.UV0, null);
            }

            if (matInfo.GetInt("_UseAudioLink") != 0 && matInfo.GetInt("_AudioLink2Vertex") != 0)
            {
                var _AudioLinkUVMode = matInfo.GetInt("_AudioLinkUVMode");

                if (_AudioLinkUVMode is 3 or 4 or null)
                {
                    // TODO: _AudioLinkMask_ScrollRotate
                    var sampler = "_AudioLinkMask" | SamplerStateInformation.LinearRepeatSampler;
                    switch (matInfo.GetInt("_AudioLinkMask_UVMode"))
                    {
                        case 0:
                        default:
                            LIL_SAMPLE_2D_ST_WithMat("_AudioLinkMask", sampler, uvMain, uvMainMatrix);
                            break;
                        case 1:
                            LIL_SAMPLE_2D_ST("_AudioLinkMask", sampler, UsingUVChannels.UV1);
                            break;
                        case 2:
                            LIL_SAMPLE_2D_ST("_AudioLinkMask", sampler, UsingUVChannels.UV2);
                            break;
                        case 3:
                            LIL_SAMPLE_2D_ST("_AudioLinkMask", sampler, UsingUVChannels.UV3);
                            break;
                        case null:
                            LIL_SAMPLE_2D_ST_WithMat("_AudioLinkMask", sampler,
                                uvMain | UsingUVChannels.UV1 | UsingUVChannels.UV2 | UsingUVChannels.UV3,
                                Combine(uvMainMatrix, Matrix2x3.Identity));
                            break;
                    }
                }
            }

            if (matInfo.GetVector("_DissolveParams")?.x != 0)
            {
                lilCalcDissolveWithOrWithoutNoise(
                    //fd.col.a,
                    //dissolveAlpha,
                    UsingUVChannels.UV0,
                    //fd.positionOS,
                    //_DissolveParams,
                    //_DissolvePos,
                    "_DissolveMask",
                    "_DissolveMask_ST",
                    //_DissolveMaskEnabled,
                    "_DissolveNoiseMask",
                    "_DissolveNoiseMask_ST",
                    "_DissolveNoiseMask_ScrollRotate",
                    //_DissolveNoiseStrength
                    "_MainTex"
                );
            }

            if (isOutline) // _UseOutline is not used, use shader name to determine
            {
                // not on material side, on editor side toggle
                LIL_SAMPLE_2D_WithMat("_OutlineTex", "_OutlineTex", uvMain, uvMainMatrix);
                LIL_SAMPLE_2D_WithMat("_OutlineWidthMask", SamplerStateInformation.LinearRepeatSampler, uvMain,
                    uvMainMatrix);
                // _OutlineVectorTex SamplerStateInformation.LinearRepeatSampler
                // UVs _OutlineVectorUVMode main,1,2,3

                switch (matInfo.GetInt("_AudioLinkMask_UVMode"))
                {
                    case 0:
                        LIL_SAMPLE_2D_WithMat("_OutlineVectorTex", SamplerStateInformation.LinearRepeatSampler, uvMain,
                            uvMainMatrix);
                        break;
                    case 1:
                        LIL_SAMPLE_2D("_OutlineVectorTex", SamplerStateInformation.LinearRepeatSampler,
                            UsingUVChannels.UV1);
                        break;
                    case 2:
                        LIL_SAMPLE_2D("_OutlineVectorTex", SamplerStateInformation.LinearRepeatSampler,
                            UsingUVChannels.UV2);
                        break;
                    case 3:
                        LIL_SAMPLE_2D("_OutlineVectorTex", SamplerStateInformation.LinearRepeatSampler,
                            UsingUVChannels.UV3);
                        break;
                    default:
                    case null:
                        matInfo.RegisterTextureUVUsage(
                            "_OutlineVectorTex",
                            SamplerStateInformation.LinearRepeatSampler,
                            UsingUVChannels.UV0 | UsingUVChannels.UV1 | UsingUVChannels.UV2 | UsingUVChannels.UV3,
                            Combine(uvMainMatrix, Matrix2x3.Identity)
                        );
                        break;
                }
            }

            // _BaseMap and _BaseColorMap are unused
            #endregion Default.lilblock

            #region DefaultFurCutout, DefaultFurTransparent, DefaultAll, 
            // fur
            LIL_SAMPLE_2D_ST("_FurNoiseMask", "_MainTex", UsingUVChannels.UV0);
            LIL_SAMPLE_2D_ST_WithMat("_FurMask", "_MainTex", uvMain, uvMainMatrix);
            LIL_SAMPLE_2D_ST_WithMat("_FurLengthMask", "_MainTex", uvMain, uvMainMatrix);
            LIL_SAMPLE_2D_WithMat("_FurLengthMask", SamplerStateInformation.LinearRepeatSampler, UsingUVChannels.UV0,
                STToMatrix($"_MainTex_ST"));
            LIL_SAMPLE_2D_WithMat("_FurVectorTex", SamplerStateInformation.LinearRepeatSampler, uvMain, uvMainMatrix);

            #endregion
            // Vertex ID
            var idMaskProperties = new[]
            {
            "_IDMask1", "_IDMask2", "_IDMask3", "_IDMask4", "_IDMask5", "_IDMask6", "_IDMask7", "_IDMask8",
            "_IDMaskPrior1", "_IDMaskPrior2", "_IDMaskPrior3", "_IDMaskPrior4", "_IDMaskPrior5", "_IDMaskPrior6", "_IDMaskPrior7", "_IDMaskPrior8",
            "_IDMaskIsBitmap", "_IDMaskCompile"
        };
            if (idMaskProperties.Any(prop => matInfo.GetInt(prop) != 0))
            {
                // with _IDMaskFrom = 0..7, uv is used for ID Mask, but it will only use integer part
                // (cast to int with normal rounding in hlsl) so it's not necessary to register UV usage.
                matInfo.RegisterVertexIndexUsage();
            }

            // fur shader will use vertex ID, but it's for noise so it's not necessary to register UV usage.

            return;

            void LIL_SAMPLE_1D(string textureName, SamplerStateInformation samplerName, UsingUVChannels uvChannel)
            {
                matInfo.RegisterTextureUVUsage(
                    textureName,
                    samplerName,
                    uvChannel,
                    Matrix2x3.Identity
                );
            }

            void LIL_SAMPLE_2D(string textureName, SamplerStateInformation samplerName, UsingUVChannels uvChannel)
            {
                // might be _LOD: using SampleLevel
                matInfo.RegisterTextureUVUsage(
                    textureName,
                    samplerName,
                    uvChannel,
                    Matrix2x3.Identity
                );
            }

            void LIL_SAMPLE_2D_WithMat(string textureName, SamplerStateInformation samplerName, UsingUVChannels uvChannel,
                Matrix2x3? matrix)
            {
                // might be _LOD: using SampleLevel
                matInfo.RegisterTextureUVUsage(
                    textureName,
                    samplerName,
                    uvChannel,
                    matrix
                );
            }

            void LIL_SAMPLE_2D_GRAD(string textureName, SamplerStateInformation samplerName, UsingUVChannels uvChannel)
            {
                // additional parameter for SampleGrad does not affect UV location much
                LIL_SAMPLE_2D(textureName, samplerName, uvChannel);
            }

            void LIL_SAMPLE_2D_GRAD_WithMat(string textureName, SamplerStateInformation samplerName,
                UsingUVChannels uvChannel, Matrix2x3? matrix)
            {
                // additional parameter for SampleGrad does not affect UV location much
                LIL_SAMPLE_2D_WithMat(textureName, samplerName, uvChannel, matrix);
            }

            void LIL_SAMPLE_2D_ST(string textureName, SamplerStateInformation samplerName, UsingUVChannels uvChannel)
            {
                matInfo.RegisterTextureUVUsage(
                    textureName,
                    samplerName,
                    uvChannel,
                    STToMatrix($"{textureName}_ST")
                );
            }

            void LIL_SAMPLE_2D_ST_WithMat(string textureName, SamplerStateInformation samplerName,
                UsingUVChannels uvChannel, Matrix2x3? matrix)
            {
                matInfo.RegisterTextureUVUsage(
                    textureName,
                    samplerName,
                    uvChannel,
                    Multiply(STToMatrix($"{textureName}_ST"), matrix)
                );
            }

            void LIL_GET_SUBTEX(string textureName, UsingUVChannels uvChannel)
            {
                // lilGetSubTex

                // TODO: consider the following properties
                var st = $"{textureName}_ST";
                var scrollRotate = $"{textureName}_ScrollRotate";
                var angle = $"{textureName}Angle";
                var isDecal = $"{textureName}IsDecal";
                var isLeftOnly = $"{textureName}IsLeftOnly";
                var isRightOnly = $"{textureName}IsRightOnly";
                var shouldCopy = $"{textureName}ShouldCopy";
                var shouldFlipMirror = $"{textureName}ShouldFlipMirror";
                var shouldFlipCopy = $"{textureName}ShouldFlipCopy";
                var isMSDF = $"{textureName}IsMSDF";
                var decalAnimation = $"{textureName}DecalAnimation";
                var decalSubParam = $"{textureName}DecalSubParam";
                // fd.nv?
                // fd.isRightHand?


                Matrix2x3? ComputeMatrix()
                {
                    var stValueOpt = matInfo.GetVector(st);
                    var rotateValueOpt = matInfo.GetVector(scrollRotate);
                    var angleValueOpt = matInfo.GetFloat(angle);
                    //var isDecalValueOpt = matInfo.GetFloat(isDecal);
                    var isLeftOnlyValueOpt = matInfo.GetFloat(isLeftOnly);
                    var isRightOnlyValueOpt = matInfo.GetFloat(isRightOnly);
                    var shouldCopyValueOpt = matInfo.GetFloat(shouldCopy);
                    var shouldFlipMirrorValueOpt = matInfo.GetFloat(shouldFlipMirror);
                    var shouldFlipCopyValueOpt = matInfo.GetFloat(shouldFlipCopy);
                    //var isMSDFValueOpt = matInfo.GetFloat(isMSDF);
                    var decalAnimationValueOpt = matInfo.GetVector(decalAnimation);
                    // var decalSubParamValueOpt = matInfo.GetVector(decalSubParam);

                    if (stValueOpt is not { } stValue) return null;
                    if (rotateValueOpt is not { } rotateValue) return null;
                    if (angleValueOpt is not { } angleValue) return null;

                    rotateValue.z = angleValue;

                    if (STAndScrollRotateValueToMatrix(stValue, rotateValue) is not { } matrix) return null;

                    // shouldCopy is true => x = abs(x - 0.5) + 0.5
                    if (shouldCopyValueOpt != 0) return null;
                    // shouldFlipCopy is true => flips
                    if (shouldFlipCopyValueOpt != 0) return null;
                    // shouldFlipMirror is true => flips
                    if (shouldFlipMirrorValueOpt != 0) return null;

                    // isDecal is true => decal
                    if (isLeftOnlyValueOpt != 0) return null;
                    if (isRightOnlyValueOpt != 0) return null;

                    // rotation is performed in STAndScrollRotateValueToMatrix

                    if (decalAnimationValueOpt != new Vector4(1.0f, 1.0f, 1.0f, 30.0f)) return null;

                    return matrix;
                }

                matInfo.RegisterTextureUVUsage(textureName, textureName, uvChannel, ComputeMatrix());
            }

            void LIL_GET_EMITEX(string textureName, UsingUVChannels uvChannel, bool parallaxEnabled)
            {
                LIL_SAMPLE_2D_WithMat(textureName, textureName, uvChannel,
                    parallaxEnabled ? null : STAndScrollRotateToMatrix($"{textureName}_ST", $"{textureName}_ScrollRotate"));
            }

            void LIL_GET_EMIMASK_WithMat(string textureName, UsingUVChannels uvChannel, Matrix2x3? matrix)
            {
                LIL_SAMPLE_2D_WithMat(textureName, "_MainTex", uvChannel,
                    Multiply(STAndScrollRotateToMatrix($"{textureName}_ST", $"{textureName}_ScrollRotate"), matrix));
            }

            void LIL_GET_EMIMASK(string textureName, UsingUVChannels uvChannel)
            {
                LIL_SAMPLE_2D_WithMat(textureName, "_MainTex", uvChannel,
                    STAndScrollRotateToMatrix($"{textureName}_ST", $"{textureName}_ScrollRotate"));
            }

            void lilCalcDissolveWithOrWithoutNoise(
                // alpha,
                // dissolveAlpha,
                UsingUVChannels uv, // ?
                                    // positionOS,
                                    // dissolveParams,
                                    // dissolvePos,
                string dissolveMask,
                string dissolveMaskST,
                //  dissolveMaskEnabled
                string dissolveNoiseMask,
                string dissolveNoiseMaskST,
                string dissolveNoiseMaskScrollRotate,
                // dissolveNoiseStrength,
                SamplerStateInformation samp
            )
            {
                LIL_SAMPLE_2D_WithMat(dissolveMask, samp, uv, STToMatrix(dissolveMaskST));
                LIL_SAMPLE_2D_WithMat(dissolveNoiseMask, samp, uv,
                    STAndScrollRotateToMatrix(dissolveNoiseMaskST, dissolveNoiseMaskScrollRotate));
            }

            // lilCalcUV
            Matrix2x3? STToMatrix(string stPropertyName) => STValueToMatrix(matInfo.GetVector(stPropertyName));

            Matrix2x3? STValueToMatrix(Vector4? stIn)
            {
                if (stIn is not { } st) return null;
                return Matrix2x3.NewScaleOffset(st);
            }

            // lilCalcUV
            Matrix2x3? STAndScrollRotateToMatrix(string stPropertyName, string scrollRotatePropertyName) =>
                STAndScrollRotateValueToMatrix(matInfo.GetVector(stPropertyName),
                    matInfo.GetVector(scrollRotatePropertyName));

            Matrix2x3? STAndScrollRotateValueToMatrix(Vector4? stValueIn, Vector4? scrollRotateIn)
            {
                if (STValueToMatrix(stValueIn) is not { } stMatrix) return null;
                if (scrollRotateIn is not { } scrollRotate) return stMatrix;

                float staticAngle = scrollRotate.z;
                float scrollAngleSpeed = scrollRotate.w;
                Vector2 scrollSpeed = new(scrollRotate.x, scrollRotate.y);

                if (scrollSpeed != Vector2.zero || scrollAngleSpeed != 0) return null;

                if (staticAngle == 0) return stMatrix;

                var result = stMatrix;

                result = Matrix2x3.Translate(-0.5f, -0.5f) * result;
                result = Matrix2x3.Rotate(staticAngle) * result;
                result = Matrix2x3.Translate(0.5f, 0.5f) * result;

                return result;
            }

            static Matrix2x3? Combine(Matrix2x3? a, Matrix2x3? b)
            {
                if (a == null) return b;
                if (b == null) return a;
                if (a == b) return a;
                return null;
            }

            Matrix2x3? Multiply(Matrix2x3? a, Matrix2x3? b)
            {
                if (a == null || b == null) return null;
                return a.Value * b.Value;
            }
        }
    }
}
