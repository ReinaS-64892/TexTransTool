using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;
using UnityEngine;
using Color = UnityEngine.Color;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class ColorDifferenceChanger : TexTransRuntimeBehavior
    {
        internal const string ComponentName = "TTT " + nameof(ColorDifferenceChanger);
        internal const string FoldoutName = "Other";
        internal const string MenuPath = FoldoutName + "/" + ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.PostProcessing;

        public TextureSelector TargetTexture = new();
        [ColorUsage(false)] public Color DifferenceSourceColor = Color.gray;
        [ColorUsage(false)] public Color TargetColor = Color.gray;

        // public ColorDiffSpace ColorDiffSpaceMode = ColorDiffSpace.RGB;
        public enum ColorDiffSpace
        {
            RGB,
            HSV,
        }

        internal override void Apply(IDomain domain)
        {
            var distTex = TargetTexture.GetTextureWithLookAt(domain, this, GetTextureSelector);
            if (distTex == null) { TTTRuntimeLog.Info("ColorDifferenceChanger:info:TargetNotSet"); return; }

            var targetTextures = domain.GetDomainsTextures(distTex).ToArray();
            if (targetTextures.Any() is false) { TTTRuntimeLog.Info("ColorDifferenceChanger:info:TargetNotFound"); return; }

            domain.LookAt(this);
            var engine = domain.GetTexTransCoreEngineForUnity();
            var gcQuay = engine.GetExKeyQuery<IQuayGeneraleComputeKey>();

            foreach (var targetTex in targetTextures)
            {
                using var rt = engine.WrappingToLoadOrUpload(targetTex);

                // switch (ColorDiffSpaceMode)
                // {
                //     default:
                //     case ColorDiffSpace.RGB:
                //          {
                            using var ch = engine.GetComputeHandler(gcQuay.GenealCompute["ColorAddition"]);

                            var gvBufID = ch.NameToID("gv");
                            var texID = ch.NameToID("Tex");

                            Span<float> gv = stackalloc float[4];
                            gv[0] = TargetColor.r - DifferenceSourceColor.r;
                            gv[1] = TargetColor.g - DifferenceSourceColor.g;
                            gv[2] = TargetColor.b - DifferenceSourceColor.b;
                            gv[3] = 0f;

                            ch.UploadConstantsBuffer<float>(gvBufID, gv);
                            ch.SetTexture(texID, rt);

                            ch.DispatchWithTextureSize(rt);
                //             break;
                //         }
                //     case ColorDiffSpace.HSV: // 実装してみたはいい物のあんまりいい感じにならなかったから一旦実装を保留
                //         {
                //             using var ch = engine.GetComputeHandler(gcQuay.GenealCompute["ColorAdditionWithHSV"]);

                //             var gvBufID = ch.NameToID("gv");
                //             var texID = ch.NameToID("Tex");

                //             Color.RGBToHSV(DifferenceSourceColor, out var sH, out var sS, out var sV);
                //             Color.RGBToHSV(TargetColor, out var tH, out var tS, out var tV);

                //             Span<float> gv = stackalloc float[4];
                //             gv[0] = tH - sH;
                //             gv[1] = tS - sS;
                //             gv[2] = tV - sV;
                //             gv[3] = 0f;

                //             ch.UploadConstantsBuffer<float>(gvBufID, gv);
                //             ch.SetTexture(texID, rt);

                //             ch.DispatchWithTextureSize(rt);
                //             break;
                //         }
                // }

                var botBlendKey = engine.QueryBlendKey(ITexTransToolForUnity.BL_KEY_NOT_BLEND);
                domain.AddTextureStack(targetTex, rt, botBlendKey);
            }
        }

        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            return TargetTexture.ModificationTargetRenderers(rendererTargeting, this, GetTextureSelector);
        }
        TextureSelector GetTextureSelector(ColorDifferenceChanger texBlend) { return texBlend.TargetTexture; }
    }
}
