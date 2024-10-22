using System;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class HSLAdjustmentLayer : AbstractGrabLayer
    {
        internal const string ComponentName = "TTT HSLAdjustmentLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        [Range(-1, 1)] public float Hue;
        [Range(-1, 1)] public float Saturation;
        [Range(-1, 1)] public float Lightness;
        internal override LayerObject<TTT4U> GetLayerObject<TTT4U>(TTT4U engine)
        {
            return new GrabBlendingAsLayer<TTT4U>(Visible, GetAlphaMask(engine), Clipping, engine.QueryBlendKey(BlendTypeKey), new HSLAdjustment(Hue, Saturation, Lightness));
        }
    }
}
