using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.Island;

namespace net.rs64.TexTransTool.Decal
{
    public abstract class AbstractSingleDecal<SpaceConverter, UVDimension> : AbstractDecal
    where SpaceConverter : DecalUtility.IConvertSpace<UVDimension>
    where UVDimension : struct
    {
        public Texture2D DecalTexture;
        internal override bool IsPossibleApply => TargetRenderers.Any(i => i != null);
        internal abstract SpaceConverter GetSpaceConverter(IIslandCache islandCacheManager);
        internal abstract DecalUtility.ITrianglesFilter<SpaceConverter> GetTriangleFilter(IIslandCache islandCacheManager);
        internal virtual bool? GetUseDepthOrInvert => null;

        internal override Dictionary<Material, Dictionary<string, RenderTexture>> CompileDecal(ITextureManager textureManager, IIslandCache islandCacheManager, Dictionary<Material, Dictionary<string, RenderTexture>> decalCompiledRenderTextures = null)
        {
            RenderTexture mulDecalTexture = GetMultipleDecalTexture(textureManager, DecalTexture, Color);
            if (decalCompiledRenderTextures == null) { decalCompiledRenderTextures = new Dictionary<Material, Dictionary<string, RenderTexture>>(); }
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
