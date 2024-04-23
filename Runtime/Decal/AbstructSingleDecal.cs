using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransTool.Utils;
using UnityEngine.Profiling;

namespace net.rs64.TexTransTool.Decal
{
    public abstract class AbstractSingleDecal<SpaceConverter, UVDimension> : AbstractDecal
    where SpaceConverter : DecalUtility.IConvertSpace<UVDimension>
    where UVDimension : struct
    {
        [ExpandTexture2D] public Texture2D DecalTexture;
        internal override bool IsPossibleApply => TargetRenderers.Any(i => i != null);
        internal abstract SpaceConverter GetSpaceConverter();
        internal abstract DecalUtility.ITrianglesFilter<SpaceConverter> GetTriangleFilter();
        internal virtual bool? GetUseDepthOrInvert => null;

        internal override Dictionary<Material, RenderTexture> CompileDecal(ITextureManager textureManager, Dictionary<Material, RenderTexture> decalCompiledRenderTextures = null)
        {
            Profiler.BeginSample("GetMultipleDecalTexture");
            RenderTexture mulDecalTexture = GetMultipleDecalTexture(textureManager, DecalTexture, Color);
            Profiler.EndSample();

            decalCompiledRenderTextures ??= new();
            foreach (var renderer in TargetRenderers)
            {
                if (renderer == null) { continue; }
                Profiler.BeginSample("CreateDecalTexture");
                DecalUtility.CreateDecalTexture<SpaceConverter, UVDimension>(
                   renderer,
                   decalCompiledRenderTextures,
                   mulDecalTexture,
                   GetSpaceConverter(),
                   GetTriangleFilter(),
                   TargetPropertyName,
                   GetTextureWarp,
                   Padding,
                   HighQualityPadding,
                   GetUseDepthOrInvert
               );
               Profiler.EndSample();
            }
            RenderTexture.ReleaseTemporary(mulDecalTexture);
            return decalCompiledRenderTextures;
        }

        internal override IEnumerable<Object> GetDependency()
        {
            return base.GetDependency().Append(DecalTexture);
        }


    }
}
