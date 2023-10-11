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
    public class RasterLayer : AbstractLayer
    {
        public Texture2D RasterTexture;

        public override void EvaluateTexture(LayerStack layerStack)
        {
            if (!Visible) { layerStack.Stack.Add(new BlendLayer(this, null, BlendMode)); return; }
            var canvasSize = layerStack.CanvasSize;
            var rTex = new RenderTexture(canvasSize.x, canvasSize.y, 0);

            Graphics.Blit(RasterTexture, rTex);

            TextureBlendUtils.MultipleRenderTexture(rTex, new Color(1, 1, 1, Opacity));
            if (!LayerMask.LayerMaskDisabled && LayerMask.MaskTexture != null) { MaskDrawRenderTexture(rTex, LayerMask.MaskTexture); }
            if (Clipping) { DrawClipping(layerStack, rTex); }

            layerStack.Stack.Add(new BlendLayer(this, rTex, BlendMode));
        }
    }
}
#endif