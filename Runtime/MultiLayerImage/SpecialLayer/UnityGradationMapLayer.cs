#nullable enable
using System;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class UnityGradationMapLayer : AbstractLayer
    {
        internal const string ComponentName = "TTT UnityGradationMapLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;

        public Gradient Gradation = new();

        internal override LayerObject<ITexTransToolForUnity> GetLayerObject(IDomain domain, ITexTransToolForUnity engine)
        {
            domain.LookAt(this);
            domain.LookAt(gameObject);

            var lm = GetAlphaMask(domain, engine);
            var blKey = engine.QueryBlendKey(BlendTypeKey);
            var lumMap = new LuminanceMapping(new UnityGradationWrapper(Gradation));

            return new GrabBlendingAsLayer<ITexTransToolForUnity>(Visible, lm, Clipping, blKey, lumMap);
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
