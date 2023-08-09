#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Rs64.TexTransTool.Decal
{
    public abstract class AbstructSingleDecal<SpaseConverter> : AbstractDecal
    where SpaseConverter : DecalUtil.IConvertSpace
    {
        public abstract SpaseConverter GetSpaseConverter { get; }
        public abstract DecalUtil.ITraianglesFilter<SpaseConverter> GetTraiangleFilter { get; }

        public override Dictionary<Texture2D, Texture> CompileDecal()
        {
            var DecalCompiledTextures = new Dictionary<Texture2D, Texture>();
            if (FastMode)
            {
                var DecalCompiledRenderTextures = new Dictionary<Texture2D, RenderTexture>();
                foreach (var Rendarer in TargetRenderers)
                {
                    DecalUtil.CreatDecalTexture(
                        Rendarer,
                        DecalCompiledRenderTextures,
                        DecalTexture,
                        GetSpaseConverter,
                        GetTraiangleFilter,
                        TargetPropatyName,
                        GetOutRengeTexture,
                        DefaultPading
                    );
                }

                foreach (var Texture in DecalCompiledRenderTextures)
                {
                    DecalCompiledTextures.Add(Texture.Key, Texture.Value);
                }
            }
            else
            {
                List<Dictionary<Texture2D, List<Texture2D>>> DecalsCompoleTexs = new List<Dictionary<Texture2D, List<Texture2D>>>();
                foreach (var Rendarer in TargetRenderers)
                {
                    var DecalsCompoleds = DecalUtil.CreatDecalTextureCS(
                        Rendarer,
                        DecalTexture,
                        GetSpaseConverter,
                        GetTraiangleFilter,
                        TargetPropatyName,
                        GetOutRengeTexture,
                        DefaultPading
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