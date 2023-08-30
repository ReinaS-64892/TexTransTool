#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace net.rs64.TexTransTool.Decal
{
    public abstract class AbstractSingleDecal<SpaceConverter> : AbstractDecal
    where SpaceConverter : DecalUtil.IConvertSpace
    {
        public Texture2D DecalTexture;
        public override bool IsPossibleApply => DecalTexture != null && TargetRenderers.Any(i => i != null);
        public abstract SpaceConverter GetSpaceConverter { get; }
        public abstract DecalUtil.ITrianglesFilter<SpaceConverter> GetTriangleFilter { get; }

        public override Dictionary<Texture2D, Texture> CompileDecal()
        {
            var mulDecalTexture = TextureLayerUtil.CreateMultipliedRenderTexture(DecalTexture, Color);
            var decalCompiledTextures = new Dictionary<Texture2D, Texture>();
            if (FastMode)
            {
                var decalCompiledRenderTextures = new Dictionary<Texture2D, RenderTexture>();
                foreach (var renderer in TargetRenderers)
                {
                    DecalUtil.CreateDecalTexture(
                        renderer,
                        decalCompiledRenderTextures,
                        mulDecalTexture,
                        GetSpaceConverter,
                        GetTriangleFilter,
                        TargetPropertyName,
                        GetOutRangeTexture,
                        Padding
                    );
                }

                foreach (var texture in decalCompiledRenderTextures)
                {
                    decalCompiledTextures.Add(texture.Key, texture.Value);
                }
            }
            else
            {
                var mulDecalTexture2D = mulDecalTexture.CopyTexture2D();
                List<Dictionary<Texture2D, List<Texture2D>>> DecalsCompileTexListDict = new List<Dictionary<Texture2D, List<Texture2D>>>();
                foreach (var renderer in TargetRenderers)
                {
                    var DecalsCompile = DecalUtil.CreateDecalTextureCS(
                        renderer,
                        mulDecalTexture2D,
                        GetSpaceConverter,
                        GetTriangleFilter,
                        TargetPropertyName,
                        GetOutRangeTexture,
                        Padding
                    );
                    DecalsCompileTexListDict.Add(DecalsCompile);
                }

                var zipDict = Utils.ZipToDictionaryOnList(DecalsCompileTexListDict);

                foreach (var texture in zipDict)
                {
                    var blendTexture = TextureLayerUtil.BlendTextureUseComputeShader(null, texture.Value, BlendType.AlphaLerp);
                    blendTexture.Apply();
                    decalCompiledTextures.Add(texture.Key, blendTexture);
                }
            }


            return decalCompiledTextures;
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
