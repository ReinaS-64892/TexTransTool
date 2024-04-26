using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.IslandSelector;
using System;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore;
using UnityEngine.Pool;
using UnityEngine.Profiling;
using net.rs64.TexTransCore.TransTextureCore.Utils;

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
        [FormerlySerializedAs("PolygonCaling")] public PolygonCulling PolygonCulling = PolygonCulling.Vertex;

        public AbstractIslandSelector IslandSelector;

        public bool UseDepth;
        public bool DepthInvert;
        internal bool? GetUseDepthOrInvert => UseDepth ? new bool?(DepthInvert) : null;

        #region V3SaveData
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
        [Obsolete("V0SaveData", true)][HideInInspector] public MatAndTexUtils.MatAndTexRelativeSeparator MigrationV0DataMatAndTexSeparator;
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

        internal override IEnumerable<UnityEngine.Object> GetDependency()
        {
            var dependencies = new UnityEngine.Object[] { transform }
            .Concat(GetComponentsInParent<Transform>(true))
            .Concat(TargetRenderers)
            .Concat(TargetRenderers.Select(r => r.transform))
            .Concat(TargetRenderers.Select(r => r.GetMesh()))
            .Concat(TargetRenderers.Where(r => r is SkinnedMeshRenderer).Cast<SkinnedMeshRenderer>().SelectMany(r => r.bones))
            .Append(DecalTexture);

            if (IslandSelector != null) { dependencies.Concat(IslandSelector.GetDependency()); }
            return dependencies;
        }

        [ExpandTexture2D] public Texture2D DecalTexture;
        internal override bool IsPossibleApply => TargetRenderers.Any(i => i != null);
        internal Dictionary<Material, RenderTexture> CompileDecal(ITextureManager textureManager, Dictionary<Material, RenderTexture> decalCompiledRenderTextures = null)
        {
            Profiler.BeginSample("GetMultipleDecalTexture");
            RenderTexture mulDecalTexture = GetMultipleDecalTexture(textureManager, DecalTexture, Color);
            Profiler.EndSample();

            decalCompiledRenderTextures ??= new();
            foreach (var renderer in TargetRenderers)
            {
                if (renderer == null) { continue; }
                Profiler.BeginSample("CreateDecalTexture");
                DecalUtility.CreateDecalTexture<ParallelProjectionSpace, Vector3>(
                   renderer,
                   decalCompiledRenderTextures,
                   mulDecalTexture,
                   GetSpaceConverter(),
                   GetTriangleFilter(),
                   TargetPropertyName,
                   TextureWrap.NotWrap,
                   Padding,
                   HighQualityPadding,
                   GetUseDepthOrInvert
               );
                Profiler.EndSample();
            }
            RenderTexture.ReleaseTemporary(mulDecalTexture);
            return decalCompiledRenderTextures;
        }

        internal ParallelProjectionSpace GetSpaceConverter() { return new ParallelProjectionSpace(transform.worldToLocalMatrix); }
        internal DecalUtility.ITrianglesFilter<ParallelProjectionSpace> GetTriangleFilter()
        {
            if (IslandSelector != null) { return new IslandSelectToPPFilter(IslandSelector, GetFilter()); }
            return new ParallelProjectionFilter(GetFilter());

        }

        internal List<TriangleFilterUtility.ITriangleFiltering<IList<Vector3>>> GetFilter()
        {
            var filters = new List<TriangleFilterUtility.ITriangleFiltering<IList<Vector3>>>
            {
                new TriangleFilterUtility.FarStruct(1, true),
                new TriangleFilterUtility.NearStruct(0, true)
            };
            if (SideCulling) filters.Add(new TriangleFilterUtility.SideStruct());
            filters.Add(new TriangleFilterUtility.OutOfPolygonStruct(PolygonCulling, 0, 1, true));

            return filters;
        }
        internal void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.black;
            var matrix = transform.localToWorldMatrix;

            Gizmos.matrix = matrix;

            var centerPos = Vector3.zero;
            Gizmos.DrawWireCube(centerPos + new Vector3(0, 0, 0.5f), new Vector3(1, 1, 1));//基準となる四角形


            DecalGizmoUtility.DrawGizmoQuad(DecalTexture, Color, matrix);
        }


    }
}
