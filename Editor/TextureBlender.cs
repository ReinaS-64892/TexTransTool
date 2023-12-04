#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransCore.BlendTexture;
using System;
namespace net.rs64.TexTransTool
{
    [AddComponentMenu("TexTransTool/TTT TextureBlender")]
    public class TextureBlender : TextureTransformer
    {
        public RelativeTextureSelector TargetTexture;

        public Texture2D BlendTexture;
        public Color Color = Color.white;
        public BlendType BlendType = BlendType.Normal;


        public override List<Renderer> GetRenderers => new List<Renderer>() { TargetTexture.TargetRenderer };

        public override bool IsPossibleApply => TargetTexture.TargetRenderer != null && BlendTexture != null;

        public override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        public override void Apply(IDomain Domain)
        {
            if (!IsPossibleApply) return;

            var DistTex = TargetTexture.GetTexture();
            if (DistTex == null) { return; }

            var AddTex = TextureBlendUtils.CreateMultipliedRenderTexture(BlendTexture, Color);
            Domain.AddTextureStack(DistTex, new TextureBlendUtils.BlendTexturePair(AddTex, BlendType));
        }
    }
    [Serializable]
    public class RelativeTextureSelector
    {
        public Renderer TargetRenderer;
        public int MaterialSelect = 0;
        public PropertyName TargetPropertyName = PropertyName.DefaultValue;

        public Texture2D GetTexture()
        {
            var DistMaterials = TargetRenderer.sharedMaterials;

            if (DistMaterials.Length <= MaterialSelect) return null;
            var DistMat = DistMaterials[MaterialSelect];

            return DistMat.GetTexture(TargetPropertyName) as Texture2D;
        }
    }
}
#endif
