using UnityEngine;
using System.Collections.Generic;
using System;
using net.rs64.TexTransTool.Utils;
using System.Linq;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCore;
using Color = UnityEngine.Color;
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

        [BlendTypeKey] public string BlendTypeKey = ITexTransToolForUnity.BL_KEY_DEFAULT;
        [Obsolete("Replaced with BlendTypeKey", true)][SerializeField] internal BlendType BlendType = BlendType.Normal;

        internal override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        internal override void Apply(IDomain domain)
        {
            domain.LookAt(this);

            var distTex = TargetTexture.GetTextureWithLookAt(domain, this, GetTextureSelector);
            if (distTex == null) { TTTRuntimeLog.Info("TextureBlender:info:TargetNotSet"); return; }

            var domainTexture = RendererUtility.GetAllTexture<Texture>(domain.EnumerateRenderer());
            var targetTextures = domainTexture.Where(m => domain.OriginEqual(m, distTex));
            if (targetTextures.Any() is false) { TTTRuntimeLog.Info("TextureBlender:info:TargetNotFound"); return; }

            domain.LookAt(targetTextures);

            var ttce4U = domain.GetTexTransCoreEngineForUnity();

            ITTRenderTexture addTex;
            var blKey = ttce4U.QueryBlendKey(BlendTypeKey);
            if (BlendTexture != null)
            {
                using var diskAddTexture = ttce4U.Wrapping(BlendTexture);
                addTex = ttce4U.LoadTextureWidthFullScale(diskAddTexture);
                ttce4U.ColorMultiply(addTex, Color.ToTTCore());
            }
            else
            {
                addTex = ttce4U.CreateRenderTexture(2, 2);
                ttce4U.ColorFill(addTex, Color.ToTTCore());
            }

            foreach (var t in targetTextures) { domain.AddTextureStack(t, addTex, blKey); }
        }

        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            return TargetTexture.ModificationTargetRenderers(rendererTargeting, this, GetTextureSelector);
        }
        TextureSelector GetTextureSelector(TextureBlender texBlend) { return texBlend.TargetTexture; }
    }
}
