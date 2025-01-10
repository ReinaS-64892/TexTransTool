#nullable enable
using System;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
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
        internal override LayerObject<TTCE4U> GetLayerObject<TTCE4U>(TTCE4U engine)
        {
            return new GrabBlendingAsLayer<TTCE4U>(Visible, GetAlphaMask(engine), Clipping, engine.QueryBlendKey(BlendTypeKey), new HSLAdjustment(Hue, Saturation, Lightness));
        }
    }
}
