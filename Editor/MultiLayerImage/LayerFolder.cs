#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlendUtils;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu("TexTransTool/MultiLayer/TTT LayerFolder")]
    public class LayerFolder : AbstractLayer
    {
        public bool PassThrough;

        public override IEnumerable<BlendTextures> EvaluateTexture(MultiLayerImageCanvas.CanvasDescription canvasDescription)
        {

            var Layers = transform.GetChildren()
            .Select(I => I.GetComponent<AbstractLayer>())
            .Reverse()
            .Where(I => I.Visible)
            .SelectMany(I => I.EvaluateTexture(canvasDescription));

            if (Layers.Count() == 0) { yield break; }
            if (PassThrough)
            {
                foreach (var layer in Layers)
                {
                    MultipleRenderTexture((RenderTexture)layer.Texture, new Color(1, 1, 1, Opacity));
                    DrawMask(LayerMask, canvasDescription.CanvasSize, (RenderTexture)layer.Texture);
                    yield return layer;
                }
            }
            else
            {
                var rt = new RenderTexture(canvasDescription.CanvasSize.x, canvasDescription.CanvasSize.y, 0);
                TextureBlendUtils.BlendBlit(rt, Layers);
                TextureBlendUtils.MultipleRenderTexture(rt, new Color(1, 1, 1, Opacity));
                DrawMask(LayerMask, canvasDescription.CanvasSize, rt);
                yield return new BlendTextures(rt, BlendType.Normal);
            }

        }
    }
}
#endif