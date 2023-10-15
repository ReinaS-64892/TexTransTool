#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.Layer;
using net.rs64.TexTransCore.TransTextureCore;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlendUtils;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu("TexTransTool/MultiLayer/TTT RasterLayer")]
    public class RasterLayer : AbstractImageLayer
    {
        public Texture2D RasterTexture;

        public override Texture GetImage()
        {
            return RasterTexture;
        }
    }
}
#endif