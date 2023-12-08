#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.BlendTexture;

namespace net.rs64.TexTransTool.Decal
{
    internal abstract class AbstractSingleDecal<SpaceConverter, UVDimension> : AbstractDecal
    where SpaceConverter : DecalUtility.IConvertSpace<UVDimension>
    where UVDimension : struct
    {
        public Texture2D DecalTexture;
        public override bool IsPossibleApply => TargetRenderers.Any(i => i != null);
        public abstract SpaceConverter GetSpaceConverter { get; }
        public abstract DecalUtility.ITrianglesFilter<SpaceConverter> GetTriangleFilter { get; }
        public virtual bool? GetUseDepthOrInvert => null;

        public override Dictionary<Material, Dictionary<string, RenderTexture>> CompileDecal(ITextureManager textureManager, Dictionary<Material, Dictionary<string, RenderTexture>> decalCompiledRenderTextures = null)
        {
            RenderTexture mulDecalTexture;
            if (DecalTexture != null) { mulDecalTexture = RenderTexture.GetTemporary(DecalTexture.width, DecalTexture.height, 0); }
            else { mulDecalTexture = RenderTexture.GetTemporary(32, 32, 0); }

            mulDecalTexture.Clear();

            if (DecalTexture != null)
            {
                TextureBlend.MultipleRenderTexture(mulDecalTexture, textureManager.GetOriginalTexture2D(DecalTexture), Color);
            }
            else
            {
                TextureBlend.ColorBlit(mulDecalTexture, Color);
            }
            if (decalCompiledRenderTextures == null) { decalCompiledRenderTextures = new Dictionary<Material, Dictionary<string, RenderTexture>>(); }
            foreach (var renderer in TargetRenderers)
            {
                if (renderer == null) { continue; }
                DecalUtility.CreateDecalTexture<SpaceConverter, UVDimension>(
                   renderer,
                   decalCompiledRenderTextures,
                   mulDecalTexture,
                   GetSpaceConverter,
                   GetTriangleFilter,
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



#endif
