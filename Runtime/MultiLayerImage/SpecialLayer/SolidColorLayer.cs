
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

        public override void GetImage<TTT4U>(TTT4U engine, ITTRenderTexture renderTexture)
        {
            engine.ClearRenderTexture(renderTexture, Color.ToTTCore());
        }
    }
}
