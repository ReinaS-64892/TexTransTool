using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransCore.BlendTexture;
using System;
using net.rs64.TexTransTool.Utils;
using System.Linq;
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


        internal override List<Renderer> GetRenderers => new List<Renderer>() { TargetTexture.RendererAsPath };

        internal override bool IsPossibleApply => TargetTexture.RendererAsPath != null && BlendTexture != null;

        internal override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        internal override void Apply(IDomain domain)
        {
            if (!IsPossibleApply) { throw new TTTNotExecutable(); }

            var distTex = TargetTexture.GetTexture();
            if (distTex == null) { return; }

            var addTex = TextureBlend.CreateMultipliedRenderTexture(BlendTexture, Color);
            domain.AddTextureStack<TextureBlend.BlendTexturePair>(distTex, new(addTex, BlendTypeKey));
        }

        internal override IEnumerable<UnityEngine.Object> GetDependency(IEnumerable<Renderer> domainRenderers) { return TargetTexture.GetDependency().Append(BlendTexture); }
    }
}
