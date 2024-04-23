using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using System;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.Island;
using UnityEngine.Pool;
using System.Linq;

namespace net.rs64.TexTransTool.Decal
{
    [ExecuteInEditMode]
    public abstract class AbstractDecal : TexTransRuntimeBehavior
    {
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public bool MultiRendererMode = false;
        [BlendTypeKey] public string BlendTypeKey = TextureBlend.BL_KEY_DEFAULT;

        public Color Color = Color.white;
        public PropertyName TargetPropertyName = PropertyName.DefaultValue;
        public float Padding = 5;
        public bool HighQualityPadding = false;

        #region V1SaveData
        [Obsolete("Replaced with BlendTypeKey", true)][HideInInspector][SerializeField] internal BlendType BlendType = BlendType.Normal;
        #endregion
        #region V0SaveData
        [Obsolete("V0SaveData", true)][HideInInspector] public bool MigrationV0ClearTarget;
        [Obsolete("V0SaveData", true)][HideInInspector] public GameObject MigrationV0DataMatAndTexSeparatorGameObject;
        [Obsolete("V0SaveData", true)][HideInInspector] public MatAndTexUtils.MatAndTexRelativeSeparator MigrationV0DataMatAndTexSeparator;
        [Obsolete("V0SaveData", true)][HideInInspector] public AbstractDecal MigrationV0DataAbstractDecal;
        [Obsolete("V0SaveData", true)][HideInInspector] public bool IsSeparateMatAndTexture;
        [Obsolete("V0SaveData", true)][HideInInspector] public bool FastMode = true;
        #endregion
        internal virtual TextureWrap GetTextureWarp { get => TextureWrap.NotWrap; }

        internal override List<Renderer> GetRenderers => TargetRenderers;

        internal override TexTransPhase PhaseDefine => TexTransPhase.AfterUVModification;

        internal override void Apply(IDomain domain)
        {
            if (!IsPossibleApply)
            {
                TTTRuntimeLog.Error(GetType().Name + ":error:TTTNotExecutable");
                return;
            }

            var decalCompiledTextures = CompileDecal(domain.GetTextureManager(), DictionaryPool<Material, RenderTexture>.Get());

            foreach (var matAndTex in decalCompiledTextures)
            {
                domain.AddTextureStack(matAndTex.Key.GetTexture(TargetPropertyName), new TextureBlend.BlendTexturePair(matAndTex.Value, BlendTypeKey));
            }

            DictionaryPool<Material, RenderTexture>.Release(decalCompiledTextures);

        }


        internal abstract Dictionary<Material, RenderTexture> CompileDecal(ITextureManager textureManager, Dictionary<Material, RenderTexture> decalCompiledRenderTextures = null);

        internal static RenderTexture GetMultipleDecalTexture(ITextureManager textureManager, Texture2D souseDecalTexture, Color color)
        {
            RenderTexture mulDecalTexture;

            if (souseDecalTexture != null)
            {
                var decalSouseSize = textureManager.GetOriginalTextureSize(souseDecalTexture);
                mulDecalTexture = RenderTexture.GetTemporary(decalSouseSize, decalSouseSize, 0);
            }
            else { mulDecalTexture = RenderTexture.GetTemporary(32, 32, 0); }
            mulDecalTexture.Clear();
            if (souseDecalTexture != null)
            {
                var tempRt = textureManager.GetOriginTempRt(souseDecalTexture);
                TextureBlend.MultipleRenderTexture(mulDecalTexture, tempRt, color);
                RenderTexture.ReleaseTemporary(tempRt);
            }
            else
            {
                TextureBlend.ColorBlit(mulDecalTexture, color);
            }
            return mulDecalTexture;
        }

        internal override IEnumerable<UnityEngine.Object> GetDependency()
        {
            return new UnityEngine.Object[]{transform}
            .Concat(TargetRenderers)
            .Concat(TargetRenderers.Select(r => r.transform))
            .Concat(TargetRenderers.Select(r => r.GetMesh()))
            .Concat(TargetRenderers.Where(r => r is SkinnedMeshRenderer).Cast<SkinnedMeshRenderer>().SelectMany(r => r.bones));
        }

    }
}
