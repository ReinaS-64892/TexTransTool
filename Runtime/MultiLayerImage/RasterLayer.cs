using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public sealed class RasterLayer : AbstractImageLayer
    {
        internal const string ComponentName = "TTT RasterLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public Texture2D RasterTexture;

        public override void GetImage(RenderTexture renderTexture, IOriginTexture originTexture)
        {
            originTexture.WriteOriginalTexture(RasterTexture, renderTexture);
        }
    }
}
