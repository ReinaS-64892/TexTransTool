#if UNITY_EDITOR
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
        public override BlendTextures EvaluateTexture(MultiLayerImageCanvas.CanvasDescription canvasDescription)
        {
            var rt = new RenderTexture(canvasDescription.CanvasSize.x, canvasDescription.CanvasSize.y, 0);

            var Layers = transform.GetChildren()
            .Select(I => I.GetComponent<AbstractLayer>())
            .Reverse()
            .Where(I => I.Visible)
            .Select(I => I.EvaluateTexture(canvasDescription))
            .ToArray();


            TextureBlendUtils.BlendBlit(rt, Layers);
            TextureBlendUtils.MultipleRenderTexture(rt, new Color(1, 1, 1, Opacity));

            return new BlendTextures(rt, BlendMode);
        }
    }
}
#endif