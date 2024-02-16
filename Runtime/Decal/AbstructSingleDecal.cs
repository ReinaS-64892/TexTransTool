using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.Island;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Decal
{
    public abstract class AbstractSingleDecal<SpaceConverter, UVDimension> : AbstractDecal
    where SpaceConverter : DecalUtility.IConvertSpace<UVDimension>
    where UVDimension : struct
    {
        [ExpandTexture2D] public Texture2D DecalTexture;
        internal override bool IsPossibleApply => TargetRenderers.Any(i => i != null);
        internal abstract SpaceConverter GetSpaceConverter(IIslandCache islandCacheManager);
        internal abstract DecalUtility.ITrianglesFilter<SpaceConverter> GetTriangleFilter(IIslandCache islandCacheManager);
        internal virtual bool? GetUseDepthOrInvert => null;

        internal override Dictionary<Material, RenderTexture> CompileDecal(ITextureManager textureManager, IIslandCache islandCacheManager, Dictionary<Material, RenderTexture> decalCompiledRenderTextures = null)
        {
            RenderTexture mulDecalTexture = GetMultipleDecalTexture(textureManager, DecalTexture, Color);
            decalCompiledRenderTextures ??= new();
            foreach (var renderer in TargetRenderers)
            {
                if (renderer == null) { continue; }
                DecalUtility.CreateDecalTexture<SpaceConverter, UVDimension>(
                   renderer,
                   decalCompiledRenderTextures,
                   mulDecalTexture,
                   GetSpaceConverter(islandCacheManager),
                   GetTriangleFilter(islandCacheManager),
                   TargetPropertyName,
                   GetTextureWarp,
                   Padding,
                   HighQualityPadding,
                   GetUseDepthOrInvert
               );
            }
            RenderTexture.ReleaseTemporary(mulDecalTexture);
            return decalCompiledRenderTextures;
        }




    }
}
