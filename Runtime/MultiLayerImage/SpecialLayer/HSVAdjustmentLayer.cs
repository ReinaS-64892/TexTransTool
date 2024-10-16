using System;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransCoreForUnity;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class HSVAdjustmentLayer : AbstractGrabLayer
    {
        internal const string ComponentName = "TTT HSVAdjustmentLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        [Range(-1, 1)] public float Hue;
        [Range(-1, 1)] public float Saturation;
        [Range(-1, 1)] public float Value;
        internal override LayerObject GetLayerObject(TexTransCore.ITexTransToolEngine engine, ITextureManager textureManager)
        {
            var hsv = new HSVAdjustment(engine.QueryComputeKey(nameof(HSVAdjustment)), Hue, Saturation, Value);
            return new GrabBlendingAsLayer(Visible, GetAlphaMask(textureManager), Clipping, engine.QueryBlendKey(BlendTypeKey), hsv);
        }
        public override void GetImage(RenderTexture grabSource, RenderTexture writeTarget, IOriginTexture originTexture)
        {
            throw new NotSupportedException();
        }
    }
}
