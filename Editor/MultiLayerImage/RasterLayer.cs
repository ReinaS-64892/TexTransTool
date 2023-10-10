#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.Layer;
using net.rs64.TexTransCore.TransTextureCore;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlendUtils;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu("TexTransTool/MultiLayer/TTT RasterLayer")]
    public class RasterLayer : AbstractLayer
    {
        public Texture2D RasterTexture;
        public Vector2Int TexturePivot;

        public override IEnumerable<BlendTextures> EvaluateTexture(MultiLayerImageCanvas.CanvasDescription canvasDescription)
        {
            var canvasSize = canvasDescription.CanvasSize;
            var tex = new RenderTexture(canvasSize.x, canvasSize.y, 0);
            DrawOffsetEvaluateTexture(tex, RasterTexture, TexturePivot, canvasDescription.CanvasSize);

            TextureBlendUtils.MultipleRenderTexture(tex, new Color(1, 1, 1, Opacity));
            DrawMask(LayerMask, canvasSize, tex);

            yield return new TextureBlendUtils.BlendTextures(tex, BlendMode);
        }
    }
}
#endif