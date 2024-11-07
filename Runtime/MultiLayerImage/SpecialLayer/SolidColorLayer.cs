
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;
using Color = UnityEngine.Color;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class SolidColorLayer : AbstractImageLayer
    {
        internal const string ComponentName = "TTT SolidColorLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        [ColorUsage(false)] public Color Color = Color.white;

        public override void GetImage<TTCE4U>(TTCE4U engine, ITTRenderTexture renderTexture)
        {
            engine.ColorFill(renderTexture, Color.ToTTCore());
        }
    }
}
