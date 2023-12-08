#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using System;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Decal
{
    [ExecuteInEditMode]
    internal abstract class AbstractDecal : TextureTransformer
    {
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public bool MultiRendererMode = false;
        [BlendTypeKey] public string BlendTypeKey = TextureBlend.BL_KEY_DEFAULT;
        #region V1SaveData
        [Obsolete("Replaced with BlendTypeKey", true)][HideInInspector] public BlendType BlendType = BlendType.Normal;
        #endregion
        public Color Color = Color.white;
        public PropertyName TargetPropertyName = PropertyName.DefaultValue;
        public float Padding = 5;
        public bool HighQualityPadding = false;

        #region V0SaveData
        [Obsolete("V0SaveData", true)][HideInInspector] public bool MigrationV0ClearTarget;
        [Obsolete("V0SaveData", true)][HideInInspector] public GameObject MigrationV0DataMatAndTexSeparatorGameObject;
        [Obsolete("V0SaveData", true)][HideInInspector] public MatAndTexUtils.MatAndTexRelativeSeparator MigrationV0DataMatAndTexSeparator;
        [Obsolete("V0SaveData", true)][HideInInspector] public AbstractDecal MigrationV0DataAbstractDecal;
        [Obsolete("V0SaveData", true)][HideInInspector] public bool IsSeparateMatAndTexture;
        [Obsolete("V0SaveData", true)][HideInInspector] public bool FastMode = true;
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

            var decalCompiledTextures = CompileDecal(Domain);

            Domain.ProgressUpdate("AddStack", 0.75f);

            foreach (var matAndTex in decalCompiledTextures)
            {
                foreach (var PramAndRt in matAndTex.Value)
                {
                    Domain.AddTextureStack(matAndTex.Key.GetTexture(PramAndRt.Key) as Texture2D, new TextureBlend.BlendTexturePair(PramAndRt.Value, BlendTypeKey));
                }
            }

            Domain.ProgressUpdate("End", 1);
            Domain.ProgressStateExit();
        }


        public abstract Dictionary<Material, Dictionary<string, RenderTexture>> CompileDecal(ITextureManager textureManager, Dictionary<Material, Dictionary<string, RenderTexture>> decalCompiledRenderTextures = null);

        public static void DecalCompiledConvert(Dictionary<Texture2D, Texture> decalCompiledTextures, Dictionary<Material, Dictionary<string, RenderTexture>> decalCompiledRenderTextures)
        {
            foreach (var matAndTex in decalCompiledRenderTextures)
            {
                foreach (var texture in matAndTex.Value)
                {
                    var souseTex = matAndTex.Key.GetTexture(texture.Key) as Texture2D;
                    if (decalCompiledTextures.ContainsKey(souseTex))
                    {
                        TextureBlend.BlendBlit(decalCompiledTextures[souseTex] as RenderTexture, texture.Value, TextureBlend.BL_KEY_DEFAULT);
                    }
                    else
                    {
                        decalCompiledTextures.Add(souseTex, texture.Value);
                    }
                }
            }
        }

        [NonSerialized] public bool ThisIsForces = false;
        private void Update()
        {
            if (ThisIsForces && RealTimePreviewManager.instance.RealTimePreviews.ContainsKey(this))
            {
                RealTimePreviewManager.instance.UpdateAbstractDecal(this);
            }
            ThisIsForces = false;
        }
        [ContextMenu("ExtractDecalCompiledTexture")]
        public void ExtractDecalCompiledTexture()
        {
            if (!IsPossibleApply) { Debug.LogError("Applyできないためデカールをコンパイルできません。"); return; }


            var path = EditorUtility.OpenFolderPanel("ExtractDecalCompiledTexture", "Assets", "");
            if (string.IsNullOrEmpty(path) && !Directory.Exists(path)) return;

            var decalCompiledTextures = CompileDecal(new TextureManager(false));
            var decalCompiledTexPier = new Dictionary<Texture2D, Texture>();
            DecalCompiledConvert(decalCompiledTexPier, decalCompiledTextures);
            foreach (var TexturePair in decalCompiledTexPier)
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
