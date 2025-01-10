#nullable enable
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class ColorizeLayer : AbstractGrabLayer
    {
        internal const string ComponentName = "TTT ColorizeLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        [ColorUsage(false)] public Color Color = Color.white;

        internal override LayerObject<TTCE4U> GetLayerObject<TTCE4U>(TTCE4U engine)
        {
            return new GrabBlendingAsLayer<TTCE4U>(Visible, GetAlphaMask(engine), Clipping, engine.QueryBlendKey(BlendTypeKey), new Colorize(Color.ToTTCore()));
        }
    }
}
