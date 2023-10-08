#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu("TexTransTool/MultiLayer/TTT RasterLayer")]
    public class RasterLayer : AbstractLayer
    {
        public Texture2D RasterTexture;
        public Vector2Int TexturePivot;

        public override TextureBlendUtils.BlendTextures EvaluateTexture(MultiLayerImageCanvas.CanvasDescription canvasDescription)
        {
            var canvasSize = canvasDescription.CanvasSize;
            var tex = new RenderTexture(canvasSize.x, canvasSize.y, 0);

            var RightUpPos = TexturePivot + new Vector2Int(RasterTexture.width, RasterTexture.height);
            var Pivot = TexturePivot;
            if (RightUpPos != canvasDescription.CanvasSize || Pivot != Vector2Int.zero)
            {
                TextureOffset(tex, RasterTexture, new Vector2((float)RightUpPos.x / canvasSize.x, (float)RightUpPos.y / canvasSize.y), new Vector2((float)Pivot.x / canvasSize.x, (float)Pivot.y / canvasSize.y));
            }
            else
            {
                Graphics.Blit(RasterTexture, tex);
            }

            TextureBlendUtils.MultipleRenderTexture(tex, new Color(1, 1, 1, Opacity));

            return new TextureBlendUtils.BlendTextures(tex, BlendMode);
        }

        public static void TextureOffset(RenderTexture tex, Texture texture, Vector2 RightUpPos, Vector2 Pivot)
        {
            var triangle = new List<TriangleIndex>() { new TriangleIndex(0, 1, 2), new TriangleIndex(2, 1, 3) };
            var souse = new List<Vector2>() { Vector2.zero, new Vector2(0, 1), new Vector2(1, 0), Vector2.one };
            var target = new List<Vector2>() { Pivot, new Vector2(Pivot.x, RightUpPos.y), new Vector2(RightUpPos.x, Pivot.y), RightUpPos };

            var TransData = new TransTexture.TransData(triangle, souse, target);
            TransTexture.TransTextureToRenderTexture(tex, texture, TransData);
        }
    }
}
#endif