using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransCoreEngineForUnity
{
    internal static class ComputeObjectUtility
    {
        public static Dictionary<string, TTBlendingComputeShader> BlendingObject;
        public static Dictionary<string, TTSamplerComputeShader> SamplerComputeShaders;
        public static Dictionary<string, TTGrabBlendingComputeShader> GrabBlendObjects;
        public static Dictionary<string, TTGeneralComputeOperator> GeneralComputeObjects;
        public static UnityStandardComputeKeyHolder UStdHolder;

        public static event Action InitBlendShadersCallBack;
        [TexTransInitialize]
        public static void ComputeObjectsInit()
        {
            InitImpl();
            TexTransCoreRuntime.AssetModificationListen[typeof(TTBlendingComputeShader)] = InitImpl;

            static void InitImpl()
            {
                BlendingObject = TexTransCoreRuntime.LoadAssetsAtType(typeof(TTBlendingComputeShader)).Cast<TTBlendingComputeShader>().ToDictionary(i => i.BlendTypeKey, i => i);
                InitBlendShadersCallBack?.Invoke();
            }

            BlendingObject = TexTransCoreRuntime.LoadAssetsAtType(typeof(TTBlendingComputeShader)).Cast<TTBlendingComputeShader>().ToDictionary(i => i.BlendTypeKey, i => i);
            GrabBlendObjects = TexTransCoreRuntime.LoadAssetsAtType(typeof(TTGrabBlendingComputeShader)).Cast<TTGrabBlendingComputeShader>().ToDictionary(i => i.name, i => i);
            GeneralComputeObjects = TexTransCoreRuntime.LoadAssetsAtType(typeof(TTGeneralComputeOperator)).Cast<TTGeneralComputeOperator>().ToDictionary(i => i.name, i => i);
            SamplerComputeShaders = TexTransCoreRuntime.LoadAssetsAtType(typeof(TTSamplerComputeShader)).Cast<TTSamplerComputeShader>().ToDictionary(i => i.name, i => i);
            UStdHolder = new();
        }

        public class UnityStandardComputeKeyHolder : ITexTransStandardComputeKey, ITexTransTransTextureComputeKey
        {
            public ITTComputeKey AlphaFill { get; private set; }
            public ITTComputeKey AlphaCopy { get; private set; }
            public ITTComputeKey AlphaMultiply { get; private set; }
            public ITTComputeKey AlphaMultiplyWithTexture { get; private set; }
            public ITTComputeKey ColorFill { get; private set; }
            public ITTComputeKey ColorMultiply { get; private set; }
            public ITTComputeKey GammaToLinear { get; private set; }
            public ITTComputeKey LinearToGamma { get; private set; }
            public ITTComputeKey FillR { get; private set; }
            public ITTComputeKey FillRG { get; private set; }
            public ITTComputeKey FillROnly { get; private set; }
            public ITTComputeKey FillGOnly { get; private set; }
            public ITTComputeKey Swizzling { get; private set; }

            public ITTSamplerKey DefaultSampler { get; private set; }


            public ITTComputeKey TransMapping { get; private set; }
            public ITTComputeKey TransMappingUseOffset { get; }

            public ITTComputeKey TransWarpNone { get; private set; }
            public ITTComputeKey TransWarpStretch { get; private set; }

            public ITTComputeKey DepthRenderer { get; private set; }
            public ITTComputeKey CullingDepth { get; private set; }

            public UnityStandardComputeKeyHolder()
            {
                AlphaFill = GeneralComputeObjects[nameof(AlphaFill)];
                AlphaCopy = GeneralComputeObjects[nameof(AlphaCopy)];
                AlphaMultiply = GeneralComputeObjects[nameof(AlphaMultiply)];
                AlphaMultiplyWithTexture = GeneralComputeObjects[nameof(AlphaMultiplyWithTexture)];
                ColorFill = GeneralComputeObjects[nameof(ColorFill)];
                ColorMultiply = GeneralComputeObjects[nameof(ColorMultiply)];
                GammaToLinear = GeneralComputeObjects[nameof(GammaToLinear)];
                LinearToGamma = GeneralComputeObjects[nameof(LinearToGamma)];
                FillR = GeneralComputeObjects[nameof(FillR)];
                FillRG = GeneralComputeObjects[nameof(FillRG)];
                FillROnly = GeneralComputeObjects[nameof(FillROnly)];
                FillGOnly = GeneralComputeObjects[nameof(FillGOnly)];
                Swizzling = GeneralComputeObjects[nameof(Swizzling)];

                DefaultSampler = SamplerComputeShaders["AverageSampling"];

                TransMapping = GeneralComputeObjects[nameof(TransMapping)];

                TransWarpNone = GeneralComputeObjects[nameof(TransWarpNone)];
                TransWarpStretch = GeneralComputeObjects[nameof(TransWarpStretch)];

                DepthRenderer = GeneralComputeObjects[nameof(DepthRenderer)];
                CullingDepth = GeneralComputeObjects[nameof(CullingDepth)];
            }
        }

    }
}
