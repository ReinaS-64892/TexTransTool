#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using net.rs64.TexTransCore.TransTextureCore;
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
        [Obsolete("V0SaveData", true)] public bool MigrationV0ClearTarget;
        [Obsolete("V0SaveData", true)] public GameObject MigrationV0DataMatAndTexSeparatorGameObject;
        [Obsolete("V0SaveData", true)] public MatAndTexUtils.MatAndTexRelativeSeparator MigrationV0DataMatAndTexSeparator;
        [Obsolete("V0SaveData", true)] public AbstractDecal MigrationV0DataAbstractDecal;
        [Obsolete("V0SaveData", true)] public bool IsSeparateMatAndTexture;
        #endregion
        public virtual TextureWrap GetTextureWarp { get => TextureWrap.NotWrap; }

        public override List<Renderer> GetRenderers => TargetRenderers;

        public override TexTransPhase PhaseDefine => TexTransPhase.AfterUVModification;

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

            Domain.ProgressStateEnter("AbstractDecal");

            Domain.ProgressUpdate("DecalCompile", 0.25f);

            Dictionary<Texture2D, Texture> decalCompiledTextures = CompileDecal();

            Domain.ProgressUpdate("AddStack", 0.75f);

            foreach (var trp in decalCompiledTextures)
            {
                Domain.AddTextureStack(trp.Key, new TextureLayerUtil.BlendTextures(trp.Value, BlendType));
            }

            Domain.ProgressUpdate("End", 1);
            Domain.ProgressStateExit();
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
