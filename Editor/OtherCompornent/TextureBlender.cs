#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransCore.BlendTexture;
using System;
namespace net.rs64.TexTransTool
{
    [AddComponentMenu("TexTransTool/TTT TextureBlender")]
    internal class TextureBlender : TextureTransformer
    {
        public RelativeTextureSelector TargetTexture;

        public Texture2D BlendTexture;
        public Color Color = Color.white;

        public string BlendTypeKey = TextureBlend.BL_KEY_DEFAULT;
        [Obsolete("Replaced with BlendTypeKey", true)] public BlendType BlendType = BlendType.Normal;


        public override List<Renderer> GetRenderers => new List<Renderer>() { TargetTexture.TargetRenderer };

        public override bool IsPossibleApply => TargetTexture.TargetRenderer != null && BlendTexture != null;

        public override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        public override void Apply(IDomain Domain)
        {
            if (!IsPossibleApply) return;

            var DistTex = TargetTexture.GetTexture();
            if (DistTex == null) { return; }

            var AddTex = TextureBlend.CreateMultipliedRenderTexture(BlendTexture, Color);
            Domain.AddTextureStack(DistTex, new(AddTex, BlendTypeKey));
        }
    }
}
#endif
