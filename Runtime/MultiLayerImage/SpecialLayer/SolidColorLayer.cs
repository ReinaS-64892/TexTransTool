
using net.rs64.TexTransCoreForUnity;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class SolidColorLayer : AbstractImageLayer
    {
        internal const string ComponentName = "TTT SolidColorLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        [ColorUsage(false)] public Color Color = Color.white;
        public override void GetImage(RenderTexture renderTexture, IOriginTexture originTexture)
        {
            var col = Color;
            col.a = 1f;
            TextureBlend.FillColor(renderTexture, col);
        }
    }
}
