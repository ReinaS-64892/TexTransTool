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

namespace net.rs64.TexTransTool.Decal
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class SimpleDecal : TexTransRuntimeBehavior//AbstractSingleDecal<ParallelProjectionSpace, Vector3>
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

        internal override List<Renderer> GetRenderers => TargetRenderers;
        internal override TexTransPhase PhaseDefine => TexTransPhase.AfterUVModification;

        internal override void Apply(IDomain domain)
        {
            if (!IsPossibleApply) { TTTRuntimeLog.Error(GetType().Name + ":error:TTTNotExecutable"); return; }

            var decalCompiledTextures = CompileDecal(domain.GetTextureManager(), DictionaryPool<Material, RenderTexture>.Get());

            foreach (var matAndTex in decalCompiledTextures)
            {
                domain.AddTextureStack(matAndTex.Key.GetTexture(TargetPropertyName), new TextureBlend.BlendTexturePair(matAndTex.Value, BlendTypeKey));
            }

            DictionaryPool<Material, RenderTexture>.Release(decalCompiledTextures);

        }

        internal static RenderTexture GetMultipleDecalTexture(ITextureManager textureManager, Texture2D souseDecalTexture, Color color)
        {
            RenderTexture mulDecalTexture;

            if (souseDecalTexture != null)
            {
                var decalSouseSize = textureManager.GetOriginalTextureSize(souseDecalTexture);
                mulDecalTexture = RenderTexture.GetTemporary(decalSouseSize, decalSouseSize, 0);
            }
            else { mulDecalTexture = RenderTexture.GetTemporary(32, 32, 0); }
            mulDecalTexture.Clear();
            if (souseDecalTexture != null)
            {
                var tempRt = textureManager.GetOriginTempRt(souseDecalTexture);
                TextureBlend.MultipleRenderTexture(mulDecalTexture, tempRt, color);
                RenderTexture.ReleaseTemporary(tempRt);
            }
            else
            {
                TextureBlend.ColorBlit(mulDecalTexture, color);
            }
            return mulDecalTexture;
        }

        internal override IEnumerable<UnityEngine.Object> GetDependency(IDomain domain)
        {
            var dependencies = new UnityEngine.Object[] { transform }
            .Concat(transform.GetParents())
            .Concat(TargetRenderers)
            .Concat(TargetRenderers.Select(r => r.transform))
            .Concat(TargetRenderers.Select(r => r.GetMesh()))
            .Concat(TargetRenderers.Where(r => r is SkinnedMeshRenderer).Cast<SkinnedMeshRenderer>().SelectMany(r => r.bones))
            .Append(DecalTexture);

            if (IslandSelector != null) { dependencies.Concat(IslandSelector.GetDependency()); }
            return dependencies;
        }
        internal override int GetDependencyHash(IDomain domain)
        {
            var hash = 0;
            foreach (var tr in TargetRenderers) { hash ^= tr?.GetInstanceID() ?? 0; }
            hash ^= DecalTexture?.GetInstanceID() ?? 0;
            return hash;
        }

        [ExpandTexture2D] public Texture2D DecalTexture;
        internal override bool IsPossibleApply => TargetRenderers.Any(i => i != null);
        internal Dictionary<Material, RenderTexture> CompileDecal(ITextureManager textureManager, Dictionary<Material, RenderTexture> decalCompiledRenderTextures = null)
        {
            Profiler.BeginSample("GetMultipleDecalTexture");
            RenderTexture mulDecalTexture = GetMultipleDecalTexture(textureManager, DecalTexture, Color);
            Profiler.EndSample();

            var decalContext = new DecalContext<ParallelProjectionSpace, ITrianglesFilter<ParallelProjectionSpace>, Vector3>(GetSpaceConverter(), GetTriangleFilter());
            decalContext.TargetPropertyName = TargetPropertyName;
            decalContext.TextureWarp = TextureWrap.NotWrap;
            decalContext.DecalPadding = Padding;
            decalContext.HighQualityPadding = HighQualityPadding;
            decalContext.UseDepthOrInvert = GetUseDepthOrInvert;

            decalCompiledRenderTextures ??= new();
            foreach (var renderer in TargetRenderers)
            {
                if (renderer == null) { continue; }
                Profiler.BeginSample("CreateDecalTexture");
                decalContext.WriteDecalTexture(decalCompiledRenderTextures, renderer, mulDecalTexture);
                Profiler.EndSample();
            }
            RenderTexture.ReleaseTemporary(mulDecalTexture);
            return decalCompiledRenderTextures;
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
            IslandSelector?.OnDrawGizmosSelected();
        }

    }
}
