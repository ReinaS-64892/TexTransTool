#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransCore.BlendTexture;
using System;
using net.rs64.TexTransTool.Utils;
namespace net.rs64.TexTransTool
{
    [AddComponentMenu("TexTransTool/TTT TextureBlender")]
    internal class TextureBlender : TextureTransformer
    {
        public RelativeTextureSelector TargetTexture;

        public Texture2D BlendTexture;
        public Color Color = Color.white;

        [BlendTypeKey] public string BlendTypeKey = TextureBlend.BL_KEY_DEFAULT;
        [Obsolete("Replaced with BlendTypeKey", true)] public BlendType BlendType = BlendType.Normal;


        public override List<Renderer> GetRenderers => new List<Renderer>() { TargetTexture.TargetRenderer };

        public override bool IsPossibleApply => TargetTexture.TargetRenderer != null && BlendTexture != null;

        public override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        public override void Apply(IDomain domain)
        {
            if (!IsPossibleApply) return;

            var distTex = TargetTexture.GetTexture();
            if (distTex == null) { return; }

            var addTex = TextureBlend.CreateMultipliedRenderTexture(BlendTexture, Color);
            domain.AddTextureStack(distTex, new(addTex, BlendTypeKey));
        }
    }
}
#endif
