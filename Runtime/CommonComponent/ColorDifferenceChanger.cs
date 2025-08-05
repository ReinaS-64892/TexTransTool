#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransTool.MultiLayerImage;
using UnityEngine;
using Color = UnityEngine.Color;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class ColorDifferenceChanger : TexTransBehavior, ICanBehaveAsLayer, ITTGrabBlending
    {
        internal const string ComponentName = "TTT " + nameof(ColorDifferenceChanger);
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;
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
            var distTex = domain.ObserveToGet(this, b => b.TargetTexture.SelectTexture);
            if (distTex == null) { TTLog.Info("ColorDifferenceChanger:info:TargetNotSet"); return; }

            var targetTextures = domain.GetDomainsTextures(distTex).ToArray();
            if (targetTextures.Any() is false) { TTLog.Info("ColorDifferenceChanger:info:TargetNotFound"); return; }

            domain.Observe(this);
            var engine = domain.GetTexTransCoreEngineForUnity();
            var gcQuay = engine.GetExKeyQuery<IQuayGeneraleComputeKey>();

            foreach (var targetTex in targetTextures)
            {
                using var rt = engine.WrappingOrUploadToLoadFullScale(targetTex);

                WriteColorDifferenceChange(engine, gcQuay, rt);

                var botBlendKey = engine.QueryBlendKey(ITexTransToolForUnity.BL_KEY_NOT_BLEND);
                domain.AddTextureStack(targetTex, rt, botBlendKey);
            }
        }

        internal void WriteColorDifferenceChange(ITexTransToolForUnity engine, IQuayGeneraleComputeKey gcQuay, ITTRenderTexture writeTarget)
        {
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
            ch.SetTexture(texID, writeTarget);

            ch.DispatchWithTextureSize(writeTarget);
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
        }

        internal override IEnumerable<Renderer> TargetRenderers(IDomainReferenceViewer domainView)
        {
            return TextureSelector.TargetRenderers(domainView.ObserveToGet(this, b => b.TargetTexture.SelectTexture), domainView);
        }

        LayerObject<ITexTransToolForUnity> ICanBehaveAsLayer.GetLayerObject(GenerateLayerObjectContext ctx, AsLayer asLayer)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            domain.Observe(this);
            var alphaMask = asLayer.GetAlphaMaskObject(ctx);
            var blKey = engine.QueryBlendKey(asLayer.BlendTypeKey);
            return new GrabBlendingAsLayer<ITexTransToolForUnity>(asLayer.Visible, alphaMask, asLayer.Clipping, blKey, this);
        }
        public void GrabBlending<TTCE>(TTCE engine, ITTRenderTexture grabTexture) where TTCE : ITexTransCreateTexture, ITexTransComputeKeyQuery, ITexTransGetComputeHandler, ITexTransDriveStorageBufferHolder
        {
            var ttce4u = engine as ITexTransToolForUnity;

            Debug.Assert(ttce4u != null);
            if (ttce4u == null) { return; }

            var gcQuay = engine.GetExKeyQuery<IQuayGeneraleComputeKey>();
            WriteColorDifferenceChange(ttce4u, gcQuay, grabTexture);
        }
    }
}
