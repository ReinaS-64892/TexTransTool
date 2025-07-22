#nullable enable
using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using UnityEngine.Serialization;
using net.rs64.TexTransCore;
namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class TextureConfigurator : TexTransRuntimeBehavior
    {
        internal const string ComponentName = "TTT TextureConfigurator";
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.Optimizing;

        public TextureSelector TargetTexture = new();

        public bool OverrideTextureSetting = false;
        [PowerOfTwo] public int TextureSize = 2048;
        public string DownScaleAlgorithm = ITexTransToolForUnity.DS_ALGORITHM_DEFAULT;
        public bool MipMap = true;
        public string MipMapGenerationAlgorithm = ITexTransToolForUnity.DS_ALGORITHM_DEFAULT;
        public bool OverrideCompression = false;
        public TextureCompressionData CompressionSetting = new();


        internal override void Apply([NotNull] IDomain domain)
        {
            domain.LookAt(this);
            if (OverrideTextureSetting is false && OverrideCompression is false) { return; }

            var target = TargetTexture.GetTextureWithLookAt(domain, this, GetTextureSelector);
            if (target == null) { TTLog.Info("TextureConfigurator:info:TargetNotSet"); return; }

            var targetTextures = domain.GetDomainsTextures(target).OfType<Texture>().ToArray();
            if (targetTextures.Any() is false) { TTLog.Info("TextureConfigurator:info:TargetNotFound"); return; }

            var engine = domain.GetTexTransCoreEngineForUnity();
            domain.LookAt(targetTextures);

            foreach (var originTexture in targetTextures)
            {
                ITTRenderTexture resultTexture;
                var textureDescription = domain.GetTextureDescriptor(originTexture);

                using var originalFullScaleTexture = engine.WrappingOrUploadToLoadFullScale(originTexture);

                if (OverrideTextureSetting)
                {
                    resultTexture = TextureSettingsConfigure(engine, originalFullScaleTexture, TextureSize);
                    // MipMapGenerationAlgorithm
                    textureDescription.UseMipMap = MipMap;
                }
                else { resultTexture = TextureSettingsConfigure(engine, originalFullScaleTexture, originTexture.width); }


                if (OverrideCompression) { textureDescription.TextureFormat = CompressionSetting; }
                resultTexture.Name = originTexture.name + "_Configured";

                var refRt = engine.GetReferenceRenderTexture(resultTexture);
                domain.ReplaceTexture(originTexture, refRt);
                domain.RegisterReplacement(originTexture, refRt);
                domain.RegisterPostProcessingAndLazyGPUReadBack(resultTexture, textureDescription);
            }
        }

        private ITTRenderTexture TextureSettingsConfigure(ITexTransToolForUnity engine, ITTRenderTexture originFullSizeRenderTexture, int targetTextureSize)
        {
            var aspect = (float)originFullSizeRenderTexture.Hight / originFullSizeRenderTexture.Width;
            var targetSizeTexture = engine.CreateRenderTexture(targetTextureSize, (int)(targetTextureSize * aspect));

            // TODO : リサイズアルゴリズムを何とかする
            // DownScaleAlgorithm
            if (targetTextureSize != originFullSizeRenderTexture.Width) engine.DefaultResizing(targetSizeTexture, originFullSizeRenderTexture);
            else engine.CopyRenderTexture(targetSizeTexture, originFullSizeRenderTexture);

            return targetSizeTexture;
        }

        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            return TargetTexture.ModificationTargetRenderers(rendererTargeting, this, GetTextureSelector);
        }
        TextureSelector GetTextureSelector(TextureConfigurator texBlend) { return texBlend.TargetTexture; }


    }

}
