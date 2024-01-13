using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransCore.BlendTexture;
using System;
using net.rs64.TexTransTool.Utils;
namespace net.rs64.TexTransTool
{
    [AddComponentMenu("TexTransTool/TTT TextureBlender")]
    public sealed class TextureBlender : TexTransRuntimeBehavior
    {
        public RelativeTextureSelector TargetTexture;

        public Texture2D BlendTexture;
        public Color Color = Color.white;

        [BlendTypeKey] public string BlendTypeKey = TextureBlend.BL_KEY_DEFAULT;
        [Obsolete("Replaced with BlendTypeKey", true)][SerializeField] internal BlendType BlendType = BlendType.Normal;


        internal override List<Renderer> GetRenderers => new List<Renderer>() { TargetTexture.TargetRenderer };

        internal override bool IsPossibleApply => TargetTexture.TargetRenderer != null && BlendTexture != null;

        internal override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        internal override void Apply(IDomain domain)
        {
            if (!IsPossibleApply) { throw new TTTNotExecutable(); }

            var distTex = TargetTexture.GetTexture();
            if (distTex == null) { return; }

            var addTex = TextureBlend.CreateMultipliedRenderTexture(BlendTexture, Color);
            domain.AddTextureStack(distTex, new(addTex, BlendTypeKey));
        }
    }
}