using UnityEngine;
using System.Collections.Generic;
using System;
using net.rs64.TexTransTool.Utils;
using System.Linq;
using net.rs64.TexTransUnityCore.Utils;
using net.rs64.TexTransUnityCore;
namespace net.rs64.TexTransTool
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class TextureBlender : TexTransRuntimeBehavior
    {
        internal const string FoldoutName = "Other";
        internal const string ComponentName = "TTT TextureBlender";
        internal const string MenuPath = TextureBlender.FoldoutName + "/" + ComponentName;
        public TextureSelector TargetTexture;

        [ExpandTexture2D] public Texture2D BlendTexture;
        public Color Color = Color.white;

        [BlendTypeKey] public string BlendTypeKey = TextureBlend.BL_KEY_DEFAULT;
        [Obsolete("Replaced with BlendTypeKey", true)][SerializeField] internal BlendType BlendType = BlendType.Normal;

        internal override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        internal override void Apply(IDomain domain)
        {
            domain.LookAt(this);

            var distTex = TargetTexture.GetTexture();
            if (distTex == null) { TTTRuntimeLog.Info("TextureBlender:info:TargetNotSet"); return; }

            var domainTexture = RendererUtility.GetAllTexture<Texture>(domain.EnumerateRenderer());
            var targetTextures = domainTexture.Where(m => domain.OriginEqual(m, distTex));
            if (targetTextures.Any() is false) { TTTRuntimeLog.Info("TextureBlender:info:TargetNotFound"); return; }

            domain.LookAt(targetTextures);

            var addTex = BlendTexture == null ? TextureUtility.CreateColorTexForRT(Color) : TextureBlend.CreateMultipliedRenderTexture(BlendTexture, Color);
            foreach (var t in targetTextures) { domain.AddTextureStack<TextureBlend.BlendTexturePair>(t, new(addTex, BlendTypeKey)); }
        }

        internal override IEnumerable<Renderer> ModificationTargetRenderers(IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            return TargetTexture.ModificationTargetRenderers(domainRenderers, replaceTracking);
        }
    }
}
