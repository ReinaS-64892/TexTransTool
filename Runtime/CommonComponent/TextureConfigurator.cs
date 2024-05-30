using UnityEngine;
using System.Collections.Generic;
using System;
using JetBrains.Annotations;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using System.Linq;
using net.rs64.TexTransCore.Utils;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.MipMap;
using net.rs64.TexTransCore;
namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class TextureConfigurator : TexTransRuntimeBehavior
    {
        internal const string FoldoutName = "Other";
        internal const string ComponentName = "TTT TextureConfigurator";
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;

        internal override List<Renderer> GetRenderers => null;
        internal override bool IsPossibleApply => TargetTexture.GetTexture() != null;
        internal override TexTransPhase PhaseDefine => TexTransPhase.Optimizing;
        internal override IEnumerable<UnityEngine.Object> GetDependency(IDomain domain) { return TargetTexture.GetDependency(); }
        internal override int GetDependencyHash(IDomain domain) { return TargetTexture.GetDependencyHash(); }


        public TextureSelector TargetTexture;

        public bool OverrideTextureSetting;
        [PowerOfTwo] public int TextureSize;
        public bool MipMap;
        public DownScalingAlgorism DownScalingAlgorism;

        public bool OverrideCompression;
        public CompressionQualityData CompressionSetting;


        internal override void Apply([NotNull] IDomain domain)
        {
            if (!OverrideTextureSetting && !OverrideCompression) { return; }

            var textureManager = domain.GetTextureManager();
            var target = TargetTexture.GetTexture();
            var materials = domain.EnumerateRenderer()
            .SelectMany(i => i.sharedMaterials)
            .Distinct().Where(i => i != null);
            var targetTex2D = materials.SelectMany(i => i.GetAllTexture2D().Values)
            .FirstOrDefault(i => domain.OriginEqual(i, target));

            if (targetTex2D == null) { TTTRuntimeLog.Info("TextureConfigurator:error:TargetNotFound"); return;}

            var newTexture2D = default(Texture2D);
            if (OverrideTextureSetting)
            {
                var aspect = targetTex2D.height / targetTex2D.width;
                var originalSize = textureManager.GetOriginalTextureSize(targetTex2D);

                using (TTRt.U(out var originRt, originalSize, Mathf.RoundToInt(aspect * originalSize), true, false, true, true))
                using (TTRt.U(out var newTempRt, TextureSize, Mathf.RoundToInt(aspect * TextureSize), true, false, MipMap, MipMap))
                {
                    textureManager.WriteOriginalTexture(targetTex2D, originRt);
                    MipMapUtility.GenerateMips(originRt, DownScalingAlgorism);
                    if (MipMap)
                    {
                        var originMipCount = originRt.mipmapCount;
                        var targetSizeMipCount = newTempRt.mipmapCount;

                        var copyMipIndex = 1;
                        while ((originMipCount - copyMipIndex) >= 0 && (targetSizeMipCount - copyMipIndex) >= 0)
                        {
                            Graphics.CopyTexture(originRt, 0, originMipCount - copyMipIndex, newTempRt, 0, targetSizeMipCount - copyMipIndex);
                            copyMipIndex += 1;
                        }
                    }
                    else { Graphics.Blit(originRt, newTempRt); }

                    newTexture2D = newTempRt.CopyTexture2D();
                }
            }
            else
            {
                var tmpRt = textureManager.GetOriginTempRt(targetTex2D);
                newTexture2D = tmpRt.CopyTexture2D();
                TTRt.R(tmpRt);
            }

            newTexture2D.CopyFilWrap(targetTex2D);
            domain.ReplaceMaterials(MaterialUtility.ReplaceTextureAll(materials, targetTex2D, newTexture2D));

            if (OverrideCompression) { textureManager.DeferredTextureCompress((CompressionQualityApplicant.GetTextureFormat(CompressionSetting), CompressionSetting.CompressionQuality), newTexture2D); }
            else { textureManager.DeferredInheritTextureCompress(targetTex2D, newTexture2D); }

        }

    }

}
