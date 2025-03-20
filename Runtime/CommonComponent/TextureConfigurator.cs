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
        public bool MipMap = true;
        public bool OverrideCompression = false;
        public TextureCompressionData CompressionSetting = new();


        internal override void Apply([NotNull] IDomain domain)
        {
            domain.LookAt(this);
            if (OverrideTextureSetting is false && OverrideCompression is false) { return; }

            var target = TargetTexture.GetTextureWithLookAt(domain, this, GetTextureSelector);
            if (target == null) { TTTRuntimeLog.Info("TextureConfigurator:info:TargetNotSet"); return; }

            var targetTex2Ds = domain.GetDomainsTextures(target).OfType<Texture2D>().ToArray();
            if (targetTex2Ds.Any() is false) { TTTRuntimeLog.Info("TextureConfigurator:info:TargetNotFound"); return; }

            var engine = domain.GetTexTransCoreEngineForUnity();
            var textureManager = domain.GetTextureManager();
            domain.LookAt(targetTex2Ds);

            foreach (var tex2D in targetTex2Ds)
            {
                var originTex2D = tex2D;
                Texture2D resultTex2D;

                using var originalFullScaleTexture = engine.WrappingToLoadFullScaleOrUpload(originTex2D);

                if (OverrideTextureSetting) { resultTex2D = TextureSettingsConfigure(engine, originalFullScaleTexture, TextureSize, MipMap); }
                else { resultTex2D = TextureSettingsConfigure(engine, originalFullScaleTexture, originTex2D.width, originTex2D.mipmapCount > 1); }

                if (OverrideCompression) { textureManager.DeferredTextureCompress(CompressionSetting, resultTex2D); }
                else { textureManager.DeferredInheritTextureCompress(originTex2D, resultTex2D); }

                resultTex2D.name = originTex2D.name + "_Configured";
                resultTex2D.CopyFilWrap(originTex2D);

                domain.ReplaceTexture(originTex2D, resultTex2D);
                domain.TransferAsset(resultTex2D);
            }
        }

        private Texture2D TextureSettingsConfigure(ITexTransToolForUnity engine, ITTRenderTexture originFullSizeRenderTexture, int targetTextureSize, bool useMipMap)
        {
            var aspect = (float)originFullSizeRenderTexture.Hight / originFullSizeRenderTexture.Width;
            using var targetSizeTexture = engine.CreateRenderTexture(targetTextureSize, (int)(targetTextureSize * aspect));

            // TODO : リサイズアルゴリズムを何とかする
            if (targetTextureSize != originFullSizeRenderTexture.Width) engine.DefaultResizing(targetSizeTexture, originFullSizeRenderTexture);
            else engine.CopyRenderTexture(targetSizeTexture, originFullSizeRenderTexture);


            if (useMipMap)
            {
                //TODO : TTCE を用いた MipMap の生成
                var resizedTexture2d = engine.DownloadToTexture2D(targetSizeTexture, true, TexTransCoreTextureFormat.Byte);// 個々のフォーマット指定は出力空間次第で変更できてもよい気がする。
                resizedTexture2d.Apply(true);
                return resizedTexture2d;
            }
            else
            {
                var resizedTexture2d = engine.DownloadToTexture2D(targetSizeTexture, false, TexTransCoreTextureFormat.Byte);
                resizedTexture2d.Apply();
                return resizedTexture2d;
            }
        }

        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            return TargetTexture.ModificationTargetRenderers(rendererTargeting, this, GetTextureSelector);
        }
        TextureSelector GetTextureSelector(TextureConfigurator texBlend) { return texBlend.TargetTexture; }


    }

}
