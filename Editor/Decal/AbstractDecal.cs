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

        public override void Apply(IDomain domain)
        {
            if (!IsPossibleApply) { TTTLog.Fatal("Not executable"); return; }

            domain.ProgressStateEnter("AbstractDecal");

            domain.ProgressUpdate("DecalCompile", 0.25f);

            var decalCompiledTextures = CompileDecal(domain);

            domain.ProgressUpdate("AddStack", 0.75f);

            foreach (var matAndTex in decalCompiledTextures)
            {
                foreach (var PramAndRt in matAndTex.Value)
                {
                    domain.AddTextureStack(matAndTex.Key.GetTexture(PramAndRt.Key) as Texture2D, new TextureBlend.BlendTexturePair(PramAndRt.Value, BlendTypeKey));
                }
            }

            domain.ProgressUpdate("End", 1);
            domain.ProgressStateExit();
        }


        public abstract Dictionary<Material, Dictionary<string, RenderTexture>> CompileDecal(ITextureManager textureManager, Dictionary<Material, Dictionary<string, RenderTexture>> decalCompiledRenderTextures = null);

        public static RenderTexture GetMultipleDecalTexture(ITextureManager textureManager, Texture2D targetDecalTexture, Color color)
        {
            RenderTexture mulDecalTexture;
            if (targetDecalTexture != null) { mulDecalTexture = RenderTexture.GetTemporary(targetDecalTexture.width, targetDecalTexture.height, 0); }
            else { mulDecalTexture = RenderTexture.GetTemporary(32, 32, 0); }
            mulDecalTexture.Clear();
            if (targetDecalTexture != null)
            {
                TextureBlend.MultipleRenderTexture(mulDecalTexture, textureManager.GetOriginalTexture2D(targetDecalTexture), color);
            }
            else
            {
                TextureBlend.ColorBlit(mulDecalTexture, color);
            }
            return mulDecalTexture;
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

    }
}



#endif
