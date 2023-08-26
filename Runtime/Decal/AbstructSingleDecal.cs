#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace net.rs64.TexTransTool.Decal
{
    public abstract class AbstructSingleDecal<SpaseConverter> : AbstractDecal
    where SpaseConverter : DecalUtil.IConvertSpace
    {
        public Texture2D DecalTexture;
        public override bool IsPossibleApply => DecalTexture != null && TargetRenderers.Any(i => i != null);
        public abstract SpaseConverter GetSpaseConverter { get; }
        public abstract DecalUtil.ITraianglesFilter<SpaseConverter> GetTraiangleFilter { get; }

        public override Dictionary<Texture2D, Texture> CompileDecal()
        {
            var muldDecalTexture = TextureLayerUtil.CreatMuldRenderTexture(DecalTexture, Color);
            var DecalCompiledTextures = new Dictionary<Texture2D, Texture>();
            if (FastMode)
            {
                var DecalCompiledRenderTextures = new Dictionary<Texture2D, RenderTexture>();
                foreach (var Rendarer in TargetRenderers)
                {
                    DecalUtil.CreatDecalTexture(
                        Rendarer,
                        DecalCompiledRenderTextures,
                        muldDecalTexture,
                        GetSpaseConverter,
                        GetTraiangleFilter,
                        TargetPropatyName,
                        GetOutRengeTexture,
                        Pading
                    );
                }

                foreach (var Texture in DecalCompiledRenderTextures)
                {
                    DecalCompiledTextures.Add(Texture.Key, Texture.Value);
                }
            }
            else
            {
                var muldDecalTexture2D = muldDecalTexture.CopyTexture2D();
                List<Dictionary<Texture2D, List<Texture2D>>> DecalsCompoleTexs = new List<Dictionary<Texture2D, List<Texture2D>>>();
                foreach (var Rendarer in TargetRenderers)
                {
                    var DecalsCompoleds = DecalUtil.CreatDecalTextureCS(
                        Rendarer,
                        muldDecalTexture2D,
                        GetSpaseConverter,
                        GetTraiangleFilter,
                        TargetPropatyName,
                        GetOutRengeTexture,
                        Pading
                    );
                    DecalsCompoleTexs.Add(DecalsCompoleds);
                }

                var ZipDecit = Utils.ZipToDictionaryOnList(DecalsCompoleTexs);

                foreach (var Texture in ZipDecit)
                {
                    var BlendTexture = TextureLayerUtil.BlendTextureUseComputeSheder(null, Texture.Value, BlendType.AlphaLerp);
                    BlendTexture.Apply();
                    DecalCompiledTextures.Add(Texture.Key, BlendTexture);
                }
            }


            return DecalCompiledTextures;
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