#nullable enable
using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransTool.IslandSelector;
using System;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool.Decal
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class SingleGradationDecal : TexTransRuntimeBehavior
    {
        internal const string ComponentName = "TTT SingleGradationDecal";
        internal const string MenuPath = ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.AfterUVModification;
        public DecalRendererSelector RendererSelector = new() { UseMaterialFilteringForAutoSelect = true };
        public Gradient Gradient = new();
        [Range(0, 1)] public float Alpha = 1;
        public bool GradientClamp = true;
        public AbstractIslandSelector? IslandSelector;
        [BlendTypeKey] public string BlendTypeKey = ITexTransToolForUnity.BL_KEY_DEFAULT;
        public PropertyName TargetPropertyName = PropertyName.DefaultValue;
        public float Padding = 5;
        public bool HighQualityPadding = false;


        #region V5SaveData
        [Obsolete("V5SaveData", true)][SerializeField] internal List<Material> TargetMaterials = new();
        #endregion V5SaveData
        internal override void Apply(IDomain domain)
        {
            domain.LookAt(this);
            domain.LookAt(transform.GetParents().Append(transform));
            var islandSelector = IslandSelector != null ? IslandSelector : null;
            if (islandSelector != null) { islandSelector?.LookAtCalling(domain); }

            if (RendererSelector.IsTargetNotSet()) { TTTRuntimeLog.Info("SingleGradationDecal:info:TargetNotSet"); return; }
            var ttce = domain.GetTexTransCoreEngineForUnity();

            using var gradDiskTex = ttce.Wrapping(GradientTempTexture.Get(Gradient, Alpha));
            var space = new SingleGradientConvertor(transform.worldToLocalMatrix);
            var filter = new IslandSelectFilter(islandSelector, domain.OriginEqual);


            var decalContext = new DecalContext<SingleGradientConvertor, SingleGradientSpace, IslandSelectFilter, SingleGradientFilteredTrianglesHolder>(ttce, space, filter);
            decalContext.TargetPropertyName = TargetPropertyName;
            decalContext.IsTextureStretch = GradientClamp is false;
            decalContext.DecalPadding = Padding;
            decalContext.HighQualityPadding = domain.IsPreview() is false && HighQualityPadding;
            decalContext.DrawMaskMaterials = RendererSelector.GetOrNullAutoMaterialHashSet(domain);

            var targetRenderers = ModificationTargetRenderers(domain);
            var blKey = ttce.QueryBlendKey(BlendTypeKey);
            using var gradTex = ttce.LoadTextureWidthFullScale(gradDiskTex);


            domain.LookAt(targetRenderers);
            var result = decalContext.WriteDecalTexture<Texture>(targetRenderers, gradTex) ?? new();

            foreach (var m2rt in result) { domain.AddTextureStack(m2rt.Key, m2rt.Value.Texture, blKey); }


            foreach (var w in result) { w.Value.Dispose(); }
            if (result.Keys.Any() is false) { TTTRuntimeLog.Info("SingleGradationDecal:info:TargetNotFound"); }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.black;
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawLine(Vector3.zero, Vector3.up);
            if (IslandSelector != null) { IslandSelector.OnDrawGizmosSelected(); }
        }
        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            return DecalContextUtility.FilterDecalTarget(rendererTargeting, RendererSelector.GetSelectedOrIncludingAll(rendererTargeting, this, GetDRS, out var _), TargetPropertyName);
        }
        DecalRendererSelector GetDRS(SingleGradationDecal d) => d.RendererSelector;
    }
}
