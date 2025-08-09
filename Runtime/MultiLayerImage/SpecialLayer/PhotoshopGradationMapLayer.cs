#nullable enable
using System;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using UnityEngine;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class PhotoshopGradationMapLayer : AbstractLayer
    {
        internal const string ComponentName = "TTT " + nameof(PhotoshopGradationMapLayer);
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;

        public bool IsGradientReversed;
        public bool IsGradientDithered;
        public string GradientInteropMethodKey;
        public float Smoothens;
        public ColorKey[] ColorKeys;
        public TransparencyKey[] TransparencyKeys;

        internal override LayerObject<ITexTransToolForUnity> GetLayerObject(GenerateLayerObjectContext ctx)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            domain.Observe(this);
            domain.Observe(gameObject);

            var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
            var alphaMask = GetAlphaMaskObject(ctx);
            var blKey = engine.QueryBlendKey(BlendTypeKey);

            return new SolidColorLayer<ITexTransToolForUnity>(Visible, alphaMask, alphaOperator, Clipping, blKey, new());
        }
        [Serializable]
        public struct ColorKey
        {
            public float KeyLocation;
            public float MidLocation;
            [ColorUsage(false)] public Color Color;
        }
        [Serializable]
        public struct TransparencyKey
        {
            public float KeyLocation;
            public float MidLocation;
            public float Transparency;
        }
    }
}
