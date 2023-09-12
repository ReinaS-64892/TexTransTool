#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool.Utils;
using System;

namespace net.rs64.TexTransTool.Decal
{
    public abstract class AbstractDecal : TextureTransformer
    {
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public bool MultiRendererMode = false;
        public BlendType BlendType = BlendType.Normal;
        public Color Color = Color.white;
        public PropertyName TargetPropertyName = new PropertyName("_MainTex");
        public float Padding = 0.5f;
        public bool FastMode = true;

        #region V0SaveData
        [Obsolete("V0SaveData", true)] public bool IsSeparateMatAndTexture;
        #endregion
        public virtual TextureWrap GetTextureWarp { get => TextureWrap.NotWrap; }

        public override List<Renderer> GetRenderers => TargetRenderers;


        public override void Apply(IDomain Domain)
        {
            if (Domain == null)
            {
                Debug.LogWarning("Decal : ドメインが存在しません。通常ではありえないエラーです。");
                return;
            }
            if (!IsPossibleApply)
            {
                Debug.LogWarning("Decal : デカールを張ることができない状態です。ターゲットレンダラーや、デカールテクスチャーなどが設定されているかどうかご確認ください。");
                return;
            }
            Dictionary<Texture2D, Texture> decalCompiledTextures = CompileDecal();


            foreach (var trp in decalCompiledTextures)
            {
                Domain.AddTextureStack(trp.Key, new TextureLayerUtil.BlendTextures(trp.Value, BlendType));
            }
        }


        public abstract Dictionary<Texture2D, Texture> CompileDecal();


        [ContextMenu("ExtractDecalCompiledTexture")]
        public void ExtractDecalCompiledTexture()
        {
            if (!IsPossibleApply) { Debug.LogError("Applyできないためデカールをコンパイルできません。"); return; }


            var path = EditorUtility.OpenFolderPanel("ExtractDecalCompiledTexture", "Assets", "");
            if (string.IsNullOrEmpty(path) && !Directory.Exists(path)) return;

            var decalCompiledTextures = CompileDecal();
            foreach (var TexturePair in decalCompiledTextures)
            {
                var name = TexturePair.Key.name;
                Texture2D extractDCtex;
                switch (TexturePair.Value)
                {
                    case RenderTexture rt:
                        extractDCtex = rt.CopyTexture2D();
                        break;
                    case Texture2D tex:
                        extractDCtex = tex;
                        break;
                    default:
                        continue;
                }
                var pngByte = extractDCtex.EncodeToPNG();

                System.IO.File.WriteAllBytes(Path.Combine(path, name + ".png"), pngByte);

            }
        }

    }
}



#endif
