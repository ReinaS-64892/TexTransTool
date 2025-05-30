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

        public class UnityStandardComputeKeyHolder : ITexTransStandardComputeKey
        , ITransTextureComputeKey
        , IQuayGeneraleComputeKey
        , IBlendingComputeKey
        , ISamplerComputeKey
        , INearTransComputeKey
        , IAtlasComputeKey
        , IAtlasSamplerComputeKey
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
            public ITTComputeKey TransMappingWithDepth { get; private set; }

            public ITTComputeKey TransWarpNone { get; private set; }
            public ITTComputeKey TransWarpStretch { get; private set; }

            public ITTComputeKey DepthRenderer { get; private set; }
            public ITTComputeKey CullingDepth { get; private set; }

            public ITexTransComputeKeyDictionary<string> GrabBlend { get; } = new GrabBlendQuery();
            public ITexTransComputeKeyDictionary<ITTBlendKey> BlendKey { get; } = new BlendKeyUnWrapper();
            public ITexTransComputeKeyDictionary<string> GenealCompute { get; } = new GenealComputeQuery();
            public IKeyValueStore<string, ITTSamplerKey> SamplerKey { get; } = new SamplerKeyQuery();
            public ITexTransComputeKeyDictionary<ITTSamplerKey> ResizingSamplerKey { get; } = new SamplerKeyToResizing();
            public ITexTransComputeKeyDictionary<ITTSamplerKey> TransSamplerKey { get; } = new SamplerKeyToTransSampler();

            public ITTComputeKey NearTransTexture { get; private set; }
            public ITTComputeKey PositionMapper { get; private set; }
            public ITTComputeKey FilleFloat4StorageBuffer { get; private set; }
            public ITTComputeKey NearDistanceFadeWrite { get; private set; }

            public ITTComputeKey RectangleTransMapping { get; private set; }
            public ITTComputeKey MergeAtlasedTextures { get; private set; }

            public ITexTransComputeKeyDictionary<ITTSamplerKey> AtlasSamplerKey { get; } = new SamplerKeyToAtlasSampler();


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
                // DefaultSampler = SamplerComputeShaders["BilinearSampling"];

                TransMapping = GeneralComputeObjects[nameof(TransMapping)];
                TransMappingWithDepth = GeneralComputeObjects[nameof(TransMappingWithDepth)];

                TransWarpNone = GeneralComputeObjects[nameof(TransWarpNone)];
                TransWarpStretch = GeneralComputeObjects[nameof(TransWarpStretch)];

                DepthRenderer = GeneralComputeObjects[nameof(DepthRenderer)];
                CullingDepth = GeneralComputeObjects[nameof(CullingDepth)];

                NearTransTexture = GenealCompute[nameof(NearTransTexture)];
                PositionMapper = GenealCompute[nameof(PositionMapper)];
                FilleFloat4StorageBuffer = GenealCompute[nameof(FilleFloat4StorageBuffer)];
                NearDistanceFadeWrite = GenealCompute[nameof(NearDistanceFadeWrite)];

                RectangleTransMapping = GenealCompute[nameof(RectangleTransMapping)];
                MergeAtlasedTextures = GenealCompute[nameof(MergeAtlasedTextures)];
            }

            class BlendKeyUnWrapper : ITexTransComputeKeyDictionary<ITTBlendKey> { public ITTComputeKey this[ITTBlendKey key] => (TTBlendingComputeShader)key; }
            class GrabBlendQuery : ITexTransComputeKeyDictionary<string> { public ITTComputeKey this[string key] => GrabBlendObjects[key]; }
            class GenealComputeQuery : ITexTransComputeKeyDictionary<string> { public ITTComputeKey this[string key] => GeneralComputeObjects[key]; }
            class SamplerKeyQuery : IKeyValueStore<string, ITTSamplerKey> { public ITTSamplerKey this[string key] => SamplerComputeShaders[key]; }
            class SamplerKeyToResizing : ITexTransComputeKeyDictionary<ITTSamplerKey> { public ITTComputeKey this[ITTSamplerKey key] => ((TTSamplerComputeShader)key).GetResizingComputeKey; }
            class SamplerKeyToTransSampler : ITexTransComputeKeyDictionary<ITTSamplerKey> { public ITTComputeKey this[ITTSamplerKey key] => ((TTSamplerComputeShader)key).GetTransSamplerComputeKey; }
            class SamplerKeyToAtlasSampler : ITexTransComputeKeyDictionary<ITTSamplerKey> { public ITTComputeKey this[ITTSamplerKey key] => ((TTSamplerComputeShader)key).GetAtlasSamplerComputeKey; }
        }

    }
}
