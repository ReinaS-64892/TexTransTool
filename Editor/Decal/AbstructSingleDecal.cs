#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.TransTextureCore.TransCompute;
using net.rs64.TexTransCore.BlendTexture;

namespace net.rs64.TexTransTool.Decal
{
    public abstract class AbstractSingleDecal<SpaceConverter> : AbstractDecal
    where SpaceConverter : DecalUtility.IConvertSpace
    {
        public Texture2D DecalTexture;
        public override bool IsPossibleApply => TargetRenderers.Any(i => i != null);
        public abstract SpaceConverter GetSpaceConverter { get; }
        public abstract DecalUtility.ITrianglesFilter<SpaceConverter> GetTriangleFilter { get; }

        public override Dictionary<Material, Dictionary<string, RenderTexture>> CompileDecal(Dictionary<Material, Dictionary<string, RenderTexture>> decalCompiledRenderTextures = null)
        {
            RenderTexture mulDecalTexture = DecalTexture != null ? RenderTexture.GetTemporary(DecalTexture.width, DecalTexture.height, 0) : RenderTexture.GetTemporary(32, 32, 0); ;
            if (DecalTexture != null)
            {
                TextureBlendUtils.MultipleRenderTexture(mulDecalTexture, DecalTexture, Color);
            }
            else
            {
                TextureBlendUtils.ColorBlit(mulDecalTexture, Color);
            }
            if (decalCompiledRenderTextures == null) { decalCompiledRenderTextures = new Dictionary<Material, Dictionary<string, RenderTexture>>(); }
            foreach (var renderer in TargetRenderers)
            {
                DecalUtility.CreateDecalTexture(
                   renderer,
                   decalCompiledRenderTextures,
                   mulDecalTexture,
                   GetSpaceConverter,
                   GetTriangleFilter,
                   TargetPropertyName,
                   GetTextureWarp,
                   Padding,
                   HighQualityPadding
               );
            }
            RenderTexture.ReleaseTemporary(mulDecalTexture);
            return decalCompiledRenderTextures;
        }



        public virtual void ScaleApply() { throw new NotImplementedException(); }

        public void ScaleApply(Vector3 Scale, bool FixedAspect)
        {
            if (DecalTexture != null && FixedAspect)
            {
                transform.localScale = new Vector3(Scale.x, Scale.x * ((float)DecalTexture.height / (float)DecalTexture.width), Scale.z);
            }
            else
            {
                transform.localScale = Scale;
            }
        }
    }
}



#endif
