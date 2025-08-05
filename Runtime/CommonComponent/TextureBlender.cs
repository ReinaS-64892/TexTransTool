#nullable enable
using UnityEngine;
using System.Collections.Generic;
using System;
using net.rs64.TexTransTool.Utils;
using System.Linq;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCore;
using Color = UnityEngine.Color;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class TextureBlender : TexTransBehavior, ICanBehaveAsLayer ,ITexTransToolStableComponent
    {
        internal const string FoldoutName = "Other";
        internal const string ComponentName = "TTT TextureBlender";
        internal const string MenuPath = ComponentName;
        public TextureSelector TargetTexture = new();

        [ExpandTexture2D] public Texture2D? BlendTexture;
        public Color Color = Color.white;

        [BlendTypeKey] public string BlendTypeKey = ITexTransToolForUnity.BL_KEY_DEFAULT;
        internal override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        public int StabilizeSaveDataVersion => TTTDataVersion_0_10_X;

        internal override void Apply(IDomain domain)
        {
            domain.Observe(this);

            var distTex = TargetTexture.GetTextureWithObserve(domain, this, GetTextureSelector);
            if (distTex == null) { TTLog.Info("TextureBlender:info:TargetNotSet"); return; }

            var targetTextures = domain.GetDomainsTextures(distTex).ToArray();
            if (targetTextures.Any() is false) { TTLog.Info("TextureBlender:info:TargetNotFound"); return; }

            domain.Observe(targetTextures);

            var ttce4U = domain.GetTexTransCoreEngineForUnity();

            ITTRenderTexture addTex;
            var blKey = ttce4U.QueryBlendKey(BlendTypeKey);
            if (BlendTexture != null)
            {
                using var diskAddTexture = ttce4U.Wrapping(BlendTexture);
                addTex = ttce4U.LoadTextureWidthFullScale(diskAddTexture);
                ttce4U.ColorMultiply(addTex, Color.ToTTCore());
            }
            else
            {
                addTex = ttce4U.CreateRenderTexture(2, 2);
                ttce4U.ColorFill(addTex, Color.ToTTCore());
            }

            foreach (var t in targetTextures) { domain.AddTextureStack(t, addTex, blKey); }
        }

        internal override IEnumerable<Renderer> TargetRenderers(IDomainReferenceViewer rendererTargeting)
        {
            return TargetTexture.ModificationTargetRenderers(rendererTargeting, this, GetTextureSelector);
        }
        TextureSelector GetTextureSelector(TextureBlender texBlend) { return texBlend.TargetTexture; }

        LayerObject<ITexTransToolForUnity> ICanBehaveAsLayer.GetLayerObject(GenerateLayerObjectContext ctx, AsLayer asLayer)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            domain.Observe(this);
            var alphaMask = asLayer.GetAlphaMaskObject(ctx);
            var blKey = engine.QueryBlendKey(BlendTypeKey);
            var alphaOp = asLayer.Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
            if (BlendTexture != null)
            {
                var blendTex = engine.Wrapping(BlendTexture);
                return new RasterLayerAndMultipleColor<ITexTransToolForUnity>(asLayer.Visible, alphaMask, alphaOp, asLayer.Clipping, blKey, blendTex, Color.ToTTCore());
            }
            else { return new SolidColorLayer<ITexTransToolForUnity>(asLayer.Visible, alphaMask, alphaOp, asLayer.Clipping, blKey, Color.ToTTCore()); }
        }
        public bool HaveBlendTypeKey => true;

        class RasterLayerAndMultipleColor<TTCE> : RasterLayer<TTCE>
        where TTCE : ITexTransCreateTexture
        , ITexTransLoadTexture
        , ITexTransCopyRenderTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        , ITexTransDriveStorageBufferHolder
        {
            public TexTransCore.Color Color;
            public RasterLayerAndMultipleColor(
                bool visible
                , AlphaMask<TTCE> alphaModifier
                , AlphaOperation alphaOperation
                , bool preBlendToLayerBelow
                , ITTBlendKey blendTypeKey
                , ITTTexture texture
                , TexTransCore.Color color
                ) : base(visible, alphaModifier, alphaOperation, preBlendToLayerBelow, blendTypeKey, texture)
            { Color = color; }

            public override void GetImage(TTCE engine, ITTRenderTexture renderTexture)
            {
                base.GetImage(engine, renderTexture);
                engine.ColorMultiply(renderTexture, Color);
            }
        }
    }
}
