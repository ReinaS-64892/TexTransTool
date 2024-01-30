using net.rs64.TexTransCore.BlendTexture;
using UnityEngine;
using static net.rs64.TexTransCore.TransTextureCore.Utils.TextureUtility;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu("TexTransTool/MultiLayer/TTT SolidColorLayer")]
    public sealed class SolidColorLayer : AbstractImageLayer
    {
        public Color Color;
        public override void GetImage(RenderTexture renderTexture, IOriginTexture originTexture)
        {
            TextureBlend.ColorBlit(renderTexture, Color);
        }
    }
}