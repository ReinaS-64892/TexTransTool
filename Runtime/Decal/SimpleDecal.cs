#nullable enable
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.IslandSelector;
using System;
using System.Linq;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine.Profiling;
using Unity.Collections;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransCore.TransTexture;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;

namespace net.rs64.TexTransTool.Decal
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class SimpleDecal : TexTransRuntimeBehavior, ICanBehaveAsLayer
    {
        internal const string ComponentName = "TTT SimpleDecal";
        internal const string MenuPath = ComponentName;
        public DecalRendererSelector RendererSelector = new();

        [ExpandTexture2D] public Texture2D? DecalTexture;
        [BlendTypeKey] public string BlendTypeKey = ITexTransToolForUnity.BL_KEY_DEFAULT;

        public UnityEngine.Color Color = UnityEngine.Color.white;
        public PropertyName TargetPropertyName = PropertyName.DefaultValue;
        public float Padding = 5;
        public bool HighQualityPadding = false;
        public bool FixedAspect = true;
        [FormerlySerializedAs("SideChek")][FormerlySerializedAs("SideCulling")] public bool BackCulling = true;
        public bool PolygonOutOfCulling = true;

        public AbstractIslandSelector? IslandSelector;
        public MultiLayerImageCanvas? OverrideDecalTextureWithMultiLayerImageCanvas;
        public bool UseDepth;
        public bool DepthInvert;
#nullable disable
        #region V5SaveData
        [Obsolete("V5SaveData", true)][SerializeField] internal List<Renderer> TargetRenderers = new List<Renderer> { null };
        [Obsolete("V5SaveData", true)][SerializeField] internal bool MultiRendererMode = false;
        #endregion V5SaveData
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
#nullable enable
        internal override TexTransPhase PhaseDefine => TexTransPhase.AfterUVModification;
        internal bool? GetUseDepthOrInvert => UseDepth ? new bool?(DepthInvert) : null;
        internal override void Apply(IDomain domain)
        {
            domain.LookAt(this);
            if (RendererSelector.IsTargetNotSet()) { TTTRuntimeLog.Info("SimpleDecal:info:TargetNotSet"); return; }
            var decalCompiledTextures = CompileDecal(domain);

            domain.LookAt(transform.GetParents().Append(transform));
            domain.LookAt(decalCompiledTextures.Keys);
            if (IslandSelector != null) { IslandSelector.LookAtCalling(domain); }

            var blKey = domain.GetTexTransCoreEngineForUnity().QueryBlendKey(BlendTypeKey);

            foreach (var matAndTex in decalCompiledTextures)
            {
                domain.AddTextureStack(matAndTex.Key, matAndTex.Value.Texture, blKey);
            }

            if (decalCompiledTextures.Keys.Any() is false) { TTTRuntimeLog.Info("SimpleDecal:info:TargetNotFound"); }
            foreach (var t in decalCompiledTextures.Values) { t.Dispose(); }
        }
        internal Dictionary<Texture, TTRenderTexWithPaddingDistance> CompileDecal(IDomain domain)
        {
            var ttce = domain.GetTexTransCoreEngineForUnity();


            Profiler.BeginSample("GetMultipleDecalTexture");
            using var mulDecalTexture = GetDecalSourceTexture(domain, ttce);
            Profiler.EndSample();

            var targetRenderers = ModificationTargetRenderers(domain);
            var decalContext = new DecalContext
                <ParallelProjectionSpaceConvertor, ParallelProjectionSpace, ITrianglesFilter<ParallelProjectionSpace, IFilteredTriangleHolder>, IFilteredTriangleHolder>
                (ttce, GetSpaceConverter(), GetTriangleFilter(domain.OriginEqual));
            decalContext.IsTextureStretch = false;
            decalContext.DecalPadding = Padding;
            decalContext.HighQualityPadding = domain.IsPreview() is false && HighQualityPadding;
            decalContext.UseDepthOrInvert = GetUseDepthOrInvert;
            decalContext.DrawMaskMaterials = RendererSelector.GetOrNullAutoMaterialHashSet(domain);

            domain.LookAt(targetRenderers);
            return decalContext.WriteDecalTexture<Texture>(domain, targetRenderers, mulDecalTexture, TargetPropertyName) ?? new();

        }

        private ITTRenderTexture GetDecalSourceTexture(IDomain domain, ITexTransToolForUnity ttce)
        {
            if (OverrideDecalTextureWithMultiLayerImageCanvas != null)
                return OverrideDecalTextureWithMultiLayerImageCanvas.EvaluateCanvas(new(domain, (2048, 2048), null));

            if (DecalTexture == null)
            {
                var solid = ttce.CreateRenderTexture(64, 64);
                ttce.ColorFill(solid, Color.ToTTCore());
                return solid;
            }

            domain.LookAt(DecalTexture);
            var decalTexDisk = ttce.Wrapping(DecalTexture);
            var mulDecalTexture = ttce.LoadTextureWidthFullScale(decalTexDisk);
            ttce.ColorMultiply(mulDecalTexture, Color.ToTTCore());
            return mulDecalTexture;
        }

        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            return DecalContextUtility.FilterDecalTarget(rendererTargeting, RendererSelector.GetSelectedOrIncludingAll(rendererTargeting, this, GetDRS, out var _), TargetPropertyName);
            DecalRendererSelector GetDRS(SimpleDecal simpleDecal) => simpleDecal.RendererSelector;
        }
        public List<Renderer> GetIntersectRenderers(IEnumerable<Renderer> renderers)
        {
            var decalAABB = GetDecalAABB();
            var intersected = new List<Renderer>();

            foreach (var r in renderers)
            {
                if (r == null) { continue; }
                if (r.GetMesh() == null) { continue; }
                var md = r.GetToMemorizedMeshData();

                var result = md.AxisAlignedBoundingBox.IsIntersect(decalAABB);
                if (result) { intersected.Add(r); }
            }
            return intersected;
        }

        internal ParallelProjectionSpaceConvertor GetSpaceConverter() { return new(transform.worldToLocalMatrix); }
        internal ITrianglesFilter<ParallelProjectionSpace, IFilteredTriangleHolder> GetTriangleFilter(OriginEqual originEqual)
        {
            if (IslandSelector != null) { return new IslandSelectToPPFilter(IslandSelector, GetFilter(), originEqual); }
            return new ParallelProjectionFilter(GetFilter());
        }

        internal JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>[] GetFilter()
        {
            var filters = new List<JobChain<FilterTriangleJobInput<NativeArray<Vector3>>>>
            {
                TriangleFilterUtility.FarStruct.GetJobChain(1, true),
                TriangleFilterUtility.NearStruct.GetJobChain(0, true)
            };
            if (BackCulling) filters.Add(TriangleFilterUtility.SideStruct.GetJobChain(false));
            if (PolygonOutOfCulling) filters.Add(TriangleFilterUtility.OutOfPolygonStruct.GetJobChain(0, 1, true));

            return filters.ToArray();
        }
        internal void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.black;
            var matrix = transform.localToWorldMatrix;

            Gizmos.matrix = matrix;

            var centerPos = Vector3.zero;
            Gizmos.DrawWireCube(centerPos + new Vector3(0, 0, 0.5f), new Vector3(1, 1, 1));//基準となる四角形


            DecalGizmoUtility.DrawGizmoQuad(DecalTexture, Color, matrix);
            if (IslandSelector != null) { IslandSelector.OnDrawGizmosSelected(); }
        }
        static readonly Vector3[] s_decalBoxVertexes = new Vector3[]{
            new(-0.5f,-0.5f,0),
            new(0.5f,-0.5f,0),
            new(-0.5f,0.5f,0),
            new(0.5f,0.5f,0),

            new(-0.5f,-0.5f,1f),
            new(0.5f,-0.5f,1f),
            new(-0.5f,0.5f,1f),
            new(0.5f,0.5f,1f),
        };

        internal TexTransUnityAABB GetDecalAABB()
        {
            var matrix = transform.localToWorldMatrix;
            var box = new Vector3[s_decalBoxVertexes.Length];
            for (var i = 0; box.Length > i; i += 1)
            {
                box[i] = matrix.MultiplyPoint3x4(s_decalBoxVertexes[i]);
            }

            var aabb = new TexTransUnityAABB(box[0]);
            for (var i = 1; box.Length > i; i += 1)
            {
                aabb.AddVertex(box[i]);
            }
            return aabb;
        }

        bool ICanBehaveAsLayer.HaveBlendTypeKey => true;
        LayerObject<ITexTransToolForUnity> ICanBehaveAsLayer.GetLayerObject(GenerateLayerObjectContext ctx, AsLayer asLayer)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            domain.LookAt(this);
            var alphaMask = asLayer.GetAlphaMaskObject(ctx);
            var blKey = engine.QueryBlendKey(BlendTypeKey);
            var alphaOp = asLayer.Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;

            if (ctx.TargetContainedMaterials is null)
            {
                TTTRuntimeLog.Error("SimpleDecal:error:CanNotAsLayerWhenUnsupportedContext");
                return new EmptyLayer<ITexTransToolForUnity>(asLayer.Visible, alphaMask, alphaOp, asLayer.Clipping, blKey);
            }

            domain.LookAt(transform.GetParents().Append(transform));
            using var decalSourceTex = GetDecalSourceTexture(domain, engine);
            var decalWriteTarget = ctx.Engine.CreateRenderTexture(ctx.CanvasSize.x, ctx.CanvasSize.y);

            var decalRenderTarget = ctx.Domain.RendererFilterForMaterialFromDomains(ctx.TargetContainedMaterials);
            domain.LookAt(decalRenderTarget);
            var decalContext = new DecalContext
                <ParallelProjectionSpaceConvertor, ParallelProjectionSpace, ITrianglesFilter<ParallelProjectionSpace, IFilteredTriangleHolder>, IFilteredTriangleHolder>
                (ctx.Engine, GetSpaceConverter(), GetTriangleFilter(domain.OriginEqual));
            decalContext.IsTextureStretch = false;
            decalContext.DecalPadding = Padding;
            decalContext.HighQualityPadding = domain.IsPreview() is false && HighQualityPadding;
            decalContext.UseDepthOrInvert = GetUseDepthOrInvert;
            decalContext.DrawMaskMaterials = ctx.TargetContainedMaterials;

            if (decalContext.WriteDecalTextureWithSingleTexture(domain, decalRenderTarget, decalWriteTarget, decalSourceTex))
                return new RasterLayer<ITexTransToolForUnity>(asLayer.Visible, alphaMask, alphaOp, asLayer.Clipping, blKey, decalWriteTarget);
            else { return new EmptyLayer<ITexTransToolForUnity>(asLayer.Visible, alphaMask, alphaOp, asLayer.Clipping, blKey); }

        }
    }
}
