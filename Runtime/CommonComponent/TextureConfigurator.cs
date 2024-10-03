using UnityEngine;
using System.Collections.Generic;
using System;
using JetBrains.Annotations;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using System.Linq;
using net.rs64.TexTransUnityCore.Utils;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransUnityCore.MipMap;
using net.rs64.TexTransUnityCore;
using UnityEngine.Serialization;
namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class TextureConfigurator : TexTransRuntimeBehavior
    {
        internal const string FoldoutName = "Other";
        internal const string ComponentName = "TTT TextureConfigurator";
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.Optimizing;

        public TextureSelector TargetTexture;

        public bool OverrideTextureSetting = false;
        [PowerOfTwo] public int TextureSize = 2048;
        public bool MipMap = true;
        [FormerlySerializedAs("DownScalingAlgorism")] public DownScalingAlgorithm DownScalingAlgorithm = DownScalingAlgorithm.Average;
        public bool DownScalingWithLookAtAlpha = true;

        public bool OverrideCompression = false;
        public TextureCompressionData CompressionSetting = new();


        internal override void Apply([NotNull] IDomain domain)
        {
            domain.LookAt(this);
            if (!OverrideTextureSetting && !OverrideCompression) { return; }

            var textureManager = domain.GetTextureManager();
            var target = TargetTexture.GetTexture();
            if (target == null) { TTTRuntimeLog.Info("TextureConfigurator:info:TargetNotSet"); return; }

            var materials = domain.EnumerateRenderer()
            .SelectMany(i => i.sharedMaterials)
            .Distinct().Where(i => i != null);
            var targetTex2D = materials.SelectMany(i => i.GetAllTexture2D().Values).FirstOrDefault(i => domain.OriginEqual(i, target));

            if (targetTex2D == null) { TTTRuntimeLog.Info("TextureConfigurator:info:TargetNotFound"); return; }

            domain.LookAt(targetTex2D);

            var newTexture2D = default(Texture2D);
            if (OverrideTextureSetting)
            {
                var aspect = targetTex2D.height / targetTex2D.width;
                var originalSize = textureManager.GetOriginalTextureSize(targetTex2D);

                using (TTRt.U(out var newTempRt, TextureSize, Mathf.RoundToInt(aspect * TextureSize), true, false, MipMap, MipMap))
                {
                    if (originalSize >= TextureSize)
                        using (TTRt.U(out var originRt, originalSize, Mathf.RoundToInt(aspect * originalSize), true, false, true, true))
                        {
                            textureManager.WriteOriginalTexture(targetTex2D, originRt);
                            MipMapUtility.GenerateMips(originRt, DownScalingAlgorithm, !DownScalingWithLookAtAlpha);
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
                        }
                    else
                    {
                        textureManager.WriteOriginalTexture(targetTex2D, newTempRt);
                        if (MipMap) MipMapUtility.GenerateMips(newTempRt, DownScalingAlgorithm);
                    }
                    newTexture2D = newTempRt.CopyTexture2D();
                }
            }
            else
            {
                var useMipMapDefault = targetTex2D.mipmapCount > 1;
                using (TTRt.U(out var tmpRt, targetTex2D.width, targetTex2D.height, false, false, useMipMapDefault, useMipMapDefault))
                {
                    textureManager.WriteOriginalTexture(targetTex2D, tmpRt);
                    tmpRt.CopyFilWrap(targetTex2D);

                    if (useMipMapDefault) { tmpRt.GenerateMips(); }
                    newTexture2D = tmpRt.CopyTexture2D();
                }
            }

            newTexture2D.CopyFilWrap(targetTex2D);
            domain.ReplaceMaterials(MaterialUtility.ReplaceTextureAll(materials, targetTex2D, newTexture2D));

            if (OverrideCompression) { textureManager.DeferredTextureCompress(CompressionSetting, newTexture2D); }
            else { textureManager.DeferredInheritTextureCompress(targetTex2D, newTexture2D); }

        }


        internal override IEnumerable<Renderer> ModificationTargetRenderers(IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            return TargetTexture.ModificationTargetRenderers(domainRenderers, replaceTracking);
        }

    }

}
