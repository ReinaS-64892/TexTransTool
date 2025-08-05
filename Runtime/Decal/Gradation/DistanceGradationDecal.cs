#nullable enable
using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransTool.IslandSelector;
using System;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransCore.MultiLayerImageCanvas;

namespace net.rs64.TexTransTool.Decal
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class DistanceGradationDecal : TexTransBehavior, ICanBehaveAsLayer
    {
        internal const string ComponentName = "TTT " + nameof(DistanceGradationDecal);
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;// 安定的機能ではないから 一旦 other にいてもらいます。
        internal override TexTransPhase PhaseDefine => TexTransPhase.AfterUVModification;

        public DecalRendererSelector RendererSelector = new() { UseMaterialFilteringForAutoSelect = true };

        public float GradationMinDistance = 0f;
        public float GradationMaxDistance = 0f;

        public Gradient Gradient = new();
        [Range(0, 1)] public float Alpha = 1;
        public bool GradientClamp = true;

        public AbstractIslandSelector? IslandSelector;
        [BlendTypeKey] public string BlendTypeKey = ITexTransToolForUnity.BL_KEY_DEFAULT;
        public PropertyName TargetPropertyName = PropertyName.DefaultValue;

        public float Padding = 5;

        #region V6SaveData
        [Obsolete("V6SaveData", true)][SerializeField] internal bool HighQualityPadding = false;
        #endregion V6SaveData

        internal override void Apply(IDomain domain)
        {
            domain.Observe(this);
            domain.Observe(transform.GetParents().Append(transform));

            if (RendererSelector.IsTargetNotSet()) { TTLog.Info("GradationDecal:info:TargetNotSet"); return; }
            var ttce = domain.GetTexTransCoreEngineForUnity();

            using var gradDiskTex = ttce.Wrapping(GradientTempTexture.Get(Gradient, Alpha));

            var decalContext = GenerateDecalCtx(domain, ttce);
            decalContext.DrawMaskMaterials = RendererSelector.GetOrNullAutoMaterialHashSet(domain);

            var targetRenderers = TargetRenderers(domain);
            var blKey = ttce.QueryBlendKey(BlendTypeKey);
            using var gradTex = ttce.LoadTextureWidthFullScale(gradDiskTex);


            domain.Observe(targetRenderers);
            var result = decalContext.WriteDecalTexture<Texture>(domain, targetRenderers, gradTex, TargetPropertyName) ?? new();

            foreach (var m2rt in result) { domain.AddTextureStack(m2rt.Key, m2rt.Value.Texture, blKey); }


            foreach (var w in result) { w.Value.Dispose(); }
            if (result.Keys.Any() is false) { TTLog.Info("GradationDecal:info:TargetNotFound"); }
        }

        private DecalContext<DistanceGradationConvertor, DistanceGradationSpace, DistanceGradationDecalIslandSelectFilter, DistanceGradationFilteredTrianglesHolder>
            GenerateDecalCtx(IDomain domain, ITexTransToolForUnity ttce)
        {
            var islandSelector = IslandSelector != null ? IslandSelector : null;
            if (islandSelector != null) { islandSelector?.LookAtCalling(domain); }

            var space = new DistanceGradationConvertor(transform.worldToLocalMatrix, (GradationMinDistance, GradationMaxDistance));
            var filter = new DistanceGradationDecalIslandSelectFilter(islandSelector, domain);

            var decalContext = new DecalContext<DistanceGradationConvertor, DistanceGradationSpace, DistanceGradationDecalIslandSelectFilter, DistanceGradationFilteredTrianglesHolder>(ttce, space, filter);
            decalContext.IsTextureStretch = GradientClamp is false;
            decalContext.DecalPadding = Padding;
            return decalContext;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.black;
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawWireSphere(Vector3.zero, GradationMinDistance);
            Gizmos.DrawWireSphere(Vector3.zero, GradationMaxDistance);
            if (IslandSelector != null) { IslandSelector.OnDrawGizmosSelected(); }
        }
        internal override IEnumerable<Renderer> TargetRenderers(IDomainReferenceViewer rendererTargeting)
        {
            return DecalContextUtility.FilterDecalTarget(rendererTargeting, RendererSelector.GetSelectedOrIncludingAll(rendererTargeting, this, GetDRS, out var _), TargetPropertyName);
        }
        DecalRendererSelector GetDRS(DistanceGradationDecal d) => d.RendererSelector;

        bool ICanBehaveAsLayer.HaveBlendTypeKey => true;
        LayerObject<ITexTransToolForUnity> ICanBehaveAsLayer.GetLayerObject(GenerateLayerObjectContext ctx, AsLayer asLayer)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            domain.Observe(this);
            var alphaMask = asLayer.GetAlphaMaskObject(ctx);
            var blKey = engine.QueryBlendKey(BlendTypeKey);
            var alphaOp = asLayer.Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;

            if (ctx.TargetContainedMaterials is null)
            {
                TTLog.Error("GradationDecal:error:CanNotAsLayerWhenUnsupportedContext");
                return new EmptyLayer<ITexTransToolForUnity>(asLayer.Visible, alphaMask, alphaOp, asLayer.Clipping, blKey);
            }

            var islandSelector = IslandSelector != null ? IslandSelector : null;
            if (islandSelector != null) { islandSelector?.LookAtCalling(domain); }

            domain.Observe(transform.GetParents().Append(transform));

            var decalWriteTarget = ctx.Engine.CreateRenderTexture(ctx.CanvasSize.x, ctx.CanvasSize.y);
            using var gradDiskTex = engine.WrappingOrUploadToLoadFullScale(GradientTempTexture.Get(Gradient, Alpha));

            var decalContext = GenerateDecalCtx(domain, engine);
            decalContext.DrawMaskMaterials = ctx.TargetContainedMaterials;

            var decalRenderTarget = ctx.Domain.RendererFilterForMaterialFromDomains(ctx.TargetContainedMaterials);
            domain.Observe(decalRenderTarget);


            if (decalContext.WriteDecalTextureWithSingleTexture(domain, decalRenderTarget, decalWriteTarget, gradDiskTex))
                return new RasterLayer<ITexTransToolForUnity>(asLayer.Visible, alphaMask, alphaOp, asLayer.Clipping, blKey, decalWriteTarget);
            else { return new EmptyLayer<ITexTransToolForUnity>(asLayer.Visible, alphaMask, alphaOp, asLayer.Clipping, blKey); }

        }
    }
}
