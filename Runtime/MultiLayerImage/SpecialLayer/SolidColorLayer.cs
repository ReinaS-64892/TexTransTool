using net.rs64.TexTransCore.BlendTexture;
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
            TextureBlend.ColorBlit(renderTexture, col);
        }
    }
}
