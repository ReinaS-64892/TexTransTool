using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.IslandSelector;
using System;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore;
using UnityEngine.Pool;
using UnityEngine.Profiling;
using net.rs64.TexTransCore.Utils;
using Unity.Collections;
using net.rs64.TexTransTool.MultiLayerImage;

namespace net.rs64.TexTransTool.Decal
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class SimpleDecal : TexTransRuntimeBehavior
    {
        internal const string ComponentName = "TTT SimpleDecal";
        internal const string MenuPath = ComponentName;
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public bool MultiRendererMode = false;
        [BlendTypeKey] public string BlendTypeKey = TextureBlend.BL_KEY_DEFAULT;

        public Color Color = Color.white;
        public PropertyName TargetPropertyName = PropertyName.DefaultValue;
        public float Padding = 5;
        public bool HighQualityPadding = false;
        public bool FixedAspect = true;
        [FormerlySerializedAs("SideChek")] public bool SideCulling = true;
        public bool PolygonOutOfCulling = true;

        public AbstractIslandSelector IslandSelector;
        public MultiLayerImageCanvas OverrideDecalTextureWithMultiLayerImageCanvas;
        public bool UseDepth;
        public bool DepthInvert;
        internal bool? GetUseDepthOrInvert => UseDepth ? new bool?(DepthInvert) : null;

        #region V3SaveData
        [Obsolete("V3SaveData", true)][FormerlySerializedAs("PolygonCaling")][SerializeField] internal PolygonCulling PolygonCulling = PolygonCulling.Vertex;
        [Obsolete("V3SaveData", true)][SerializeField] internal bool IslandCulling = false;
        [Obsolete("V3SaveData", true)][SerializeField] internal Vector2 IslandSelectorPos = new Vector2(0.5f, 0.5f);
        [Obsolete("V3SaveData", true)][SerializeField] internal float IslandSelectorRange = 1;
        #endregion V3SaveData


        #region V1SaveData
        [Obsolete("Replaced with BlendTypeKey", true)][HideInInspector][SerializeField] internal BlendType BlendType = BlendType.Normal;
        #endregion
        #region V0SaveData
        [Obsolete("V0SaveData", true)][HideInInspector] public bool MigrationV0ClearTarget;
        [Obsolete("V0SaveData", true)][HideInInspector] public GameObject MigrationV0DataMatAndTexSeparatorGameObject;
        // [Obsolete("V0SaveData", true)][HideInInspector] public MatAndTexUtils.MatAndTexRelativeSeparator MigrationV0DataMatAndTexSeparator;
        [Obsolete("V0SaveData", true)][HideInInspector] public SimpleDecal MigrationV0DataAbstractDecal;
        [Obsolete("V0SaveData", true)][HideInInspector] public bool IsSeparateMatAndTexture;
        [Obsolete("V0SaveData", true)][HideInInspector] public bool FastMode = true;
        #endregion

        internal override TexTransPhase PhaseDefine => TexTransPhase.AfterUVModification;

        internal override void Apply(IDomain domain)
        {
            domain.LookAt(this);
            if (!IsPossibleApply) { TTTRuntimeLog.Error(GetType().Name + ":error:TTTNotExecutable"); return; }
            var targetRenderers = domain.GetDomainsRenderers(TargetRenderers);
            var decalCompiledTextures = CompileDecal(targetRenderers, domain);

            domain.LookAt(transform.GetParents().Append(transform));
            domain.LookAt(decalCompiledTextures.Keys);
            if (IslandSelector != null) { IslandSelector.LookAtCalling(domain); }

            foreach (var matAndTex in decalCompiledTextures)
            {
                domain.AddTextureStack(matAndTex.Key.GetTexture(TargetPropertyName), new TextureBlend.BlendTexturePair(matAndTex.Value, BlendTypeKey));
            }

            if (decalCompiledTextures.Keys.Any() is false) { TTTRuntimeLog.Info("SimpleDecal:info:TargetNotFound"); }
        }

        internal static RenderTexture GetMultipleDecalTexture(IDomain domain, Texture2D sourceDecalTexture, Color color)
        {
            RenderTexture mulDecalTexture;
            var texManager = domain.GetTextureManager();
            domain.LookAt(sourceDecalTexture);

            if (sourceDecalTexture != null)
            {
                var decalSourceSize = texManager.GetOriginalTextureSize(sourceDecalTexture);
                mulDecalTexture = TTRt.G(decalSourceSize, decalSourceSize);
                mulDecalTexture.name = $"{sourceDecalTexture.name}:GetMultipleDecalTextureWithNotNullSourceDecalTexture-{mulDecalTexture.width}x{mulDecalTexture.height}";
            }
            else
            {
                mulDecalTexture = TTRt.G(32, 32);
                mulDecalTexture.name = $"GetMultipleDecalTextureWithNullSourceDecalTexture-{mulDecalTexture.width}x{mulDecalTexture.height}";
            }
            mulDecalTexture.Clear();
            if (sourceDecalTexture != null)
            {
                using (texManager.GetOriginTempRtU(out var tempRt, sourceDecalTexture))
                    TextureBlend.MultipleRenderTexture(mulDecalTexture, tempRt, color);
            }
            else
            {
                TextureBlend.ColorBlit(mulDecalTexture, color);
            }
            return mulDecalTexture;
        }
        [ExpandTexture2D] public Texture2D DecalTexture;
        internal override bool IsPossibleApply => TargetRenderers.Any(i => i != null);
        internal Dictionary<Material, RenderTexture> CompileDecal(IEnumerable<Renderer> targetRenderers, IDomain domain)
        {
            RenderTexture mulDecalTexture;
            if (OverrideDecalTextureWithMultiLayerImageCanvas == null)
            {
                Profiler.BeginSample("GetMultipleDecalTexture");
                mulDecalTexture = GetMultipleDecalTexture(domain, DecalTexture, Color);
                Profiler.EndSample();
            }
            else
            {
                Profiler.BeginSample("Rendering MultiLayerImageCanvas");
                OverrideDecalTextureWithMultiLayerImageCanvas.LookAtCallingCanvas(domain);
                mulDecalTexture = OverrideDecalTextureWithMultiLayerImageCanvas.EvaluateCanvas(domain.GetTextureManager(), 2048);
                Profiler.EndSample();
            }

            var decalContext = new DecalContext<ParallelProjectionSpace, ITrianglesFilter<ParallelProjectionSpace>, Vector3>(GetSpaceConverter(), GetTriangleFilter());
            decalContext.TargetPropertyName = TargetPropertyName;
            decalContext.TextureWarp = TextureWrap.NotWrap;
            decalContext.DecalPadding = Padding;
            decalContext.HighQualityPadding = HighQualityPadding;
            decalContext.UseDepthOrInvert = GetUseDepthOrInvert;

            var decalCompiledRenderTextures = new Dictionary<Material, RenderTexture>();
            domain.LookAt(targetRenderers);
            foreach (var renderer in targetRenderers)
            {
                Profiler.BeginSample("CreateDecalTexture");
                decalContext.WriteDecalTexture(decalCompiledRenderTextures, renderer, mulDecalTexture);
                Profiler.EndSample();
            }
            TTRt.R(mulDecalTexture);
            return decalCompiledRenderTextures;
        }
        internal override IEnumerable<Renderer> ModificationTargetRenderers(IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            var targetRenderers = replaceTracking.GetDomainsRenderers(domainRenderers, TargetRenderers);
            var modificationTarget = new HashSet<Texture>();

            foreach (var tr in targetRenderers)
            {
                if (tr is not SkinnedMeshRenderer && tr is not MeshRenderer) { continue; }
                if (tr.GetMesh() == null) { continue; }
                foreach (var mat in tr.sharedMaterials)
                {
                    if (mat == null) { continue; }
                    var targetTex = mat.HasProperty(TargetPropertyName) ? mat.GetTexture(TargetPropertyName) : null;
                    if (targetTex == null) { continue; }
                    modificationTarget.Add(targetTex);
                }
            }
            return GetTextureReplacedRange(domainRenderers, modificationTarget);
        }

        public static IEnumerable<Renderer> GetTextureReplacedRange(IEnumerable<Renderer> domainRenderers, HashSet<Texture> modificationTarget)
        {
            var modificationMatHash = new HashSet<Material>();
            foreach (var mat in RendererUtility.GetFilteredMaterials(domainRenderers))
            {
                foreach (var tex in mat.GetAllTexture2D().Values)
                { if (modificationTarget.Contains(tex)) { modificationMatHash.Add(mat); break; } }
            }

            return domainRenderers.Where(dr => dr.sharedMaterials.Any(mat => modificationMatHash.Contains(mat)));
        }

        internal ParallelProjectionSpace GetSpaceConverter() { return new ParallelProjectionSpace(transform.worldToLocalMatrix); }
        internal ITrianglesFilter<ParallelProjectionSpace> GetTriangleFilter()
        {
            if (IslandSelector != null) { return new IslandSelectToPPFilter(IslandSelector, GetFilter()); }
            return new ParallelProjectionFilter(GetFilter());
        }

        internal JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] GetFilter()
        {
            var filters = new List<JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>>
            {
                TriangleFilterUtility.FarStruct.GetJobChain(1, true),
                TriangleFilterUtility.NearStruct.GetJobChain(0, true)
            };
            if (SideCulling) filters.Add(TriangleFilterUtility.SideStruct.GetJobChain(false));
            if (PolygonOutOfCulling) filters.Add(TriangleFilterUtility.OutOfPolygonStruct.GetJobChain(0, 1, true));

            return filters.ToArray();
        }
        internal void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.black;
            var matrix = transform.localToWorldMatrix;

            Gizmos.matrix = matrix;

            var centerPos = Vector3.zero;
            Gizmos.DrawWireCube(centerPos + new Vector3(0, 0, 0.5f), new Vector3(1, 1, 1));//基準となる四角形


            DecalGizmoUtility.DrawGizmoQuad(DecalTexture, Color, matrix);
            if (IslandSelector != null) { IslandSelector.OnDrawGizmosSelected(); }
        }

    }
}
