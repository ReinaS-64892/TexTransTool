using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransUnityCore;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class UnityGradationMapLayer : AbstractGrabLayer
    {
        internal const string ComponentName = "TTT UnityGradationMapLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;

        public Gradient Gradation = new();

        internal override LayerObject GetLayerObject(ITexTransToolEngine engine, ITextureManager textureManager)
        {
            var lumMap = new LuminanceMapping(engine.QueryComputeKey("LuminanceMapping"), new UnityGradationWrapper(Gradation));
            return new GrabBlendingAsLayer(Visible, GetAlphaMask(textureManager), Clipping, engine.QueryBlendKey(BlendTypeKey), lumMap);
        }
        public override void GetImage(RenderTexture grabSource, RenderTexture writeTarget, IOriginTexture originTexture)
        {
            throw new NotSupportedException();
            // var mat = MatTemp.GetTempMatShader(SpecialLayerShaders.LuminanceMappingShader);
            // mat.SetTexture("_MapTex", GradientTempTexture.Get(Gradation, 1));

            // Graphics.Blit(grabSource, writeTarget, mat);
        }
    }

    public class UnityGradationWrapper : ILuminanceMappingGradient
    {
        Gradient _gradient;

        public UnityGradationWrapper(Gradient gradient)
        {
            _gradient = gradient;
        }
        public int RecommendedResolution => 256;

        public void WriteGradient(Span<TexTransCore.Color> writeSpan)
        {
            float maxValue = writeSpan.Length - 1;
            for (var i = 0; writeSpan.Length > i; i += 1)
            {
                writeSpan[i] = _gradient.Evaluate(i / maxValue).ToTTCore();
            }
        }
    }
}
