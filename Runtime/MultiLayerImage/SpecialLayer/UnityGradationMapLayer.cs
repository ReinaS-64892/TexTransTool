using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class UnityGradationMapLayer : AbstractGrabLayer
    {
        internal const string ComponentName = "TTT UnityGradationMapLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;

        public Gradient Gradation = new();

        internal override LayerObject<TTT4U> GetLayerObject<TTT4U>(TTT4U engine)
        {
            return new GrabBlendingAsLayer<TTT4U>(Visible, GetAlphaMask(engine), Clipping, engine.QueryBlendKey(BlendTypeKey), new LuminanceMapping(new UnityGradationWrapper(Gradation)));
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
