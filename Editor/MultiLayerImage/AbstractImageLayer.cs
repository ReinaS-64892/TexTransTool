#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.MultiLayerImageParser.LayerData;
using net.rs64.TexTransCore.TransTextureCore;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;
using net.rs64.TexTransTool.Utils;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    public abstract class AbstractImageLayer : AbstractLayer
    {
        public abstract Texture GetImage();
        public override void EvaluateTexture(CanvasContext canvasContext)
        {
            var layerStack = canvasContext.RootLayerStack;
            if (!Visible) { layerStack.Stack.Add(new BlendLayer(this, null, BlendTypeKey)); return; }
            var canvasSize = layerStack.CanvasSize;
            var rTex = new RenderTexture(canvasSize.x, canvasSize.y, 0);

            var image = GetImage();
            if (image is Texture2D texture2D) { image = canvasContext.TextureManage.TryGetUnCompress(texture2D); }
            if (image is RenderTexture renderTexture) { canvasContext.TextureManage.DestroyTarget.Add(renderTexture); }
            Graphics.Blit(image, rTex);

            if (!Mathf.Approximately(Opacity, 1)) { MultipleRenderTexture(rTex, new Color(1, 1, 1, Opacity)); }

            if (!LayerMask.LayerMaskDisabled && LayerMask.MaskTexture != null) { MaskDrawRenderTexture(rTex, canvasContext.TextureManage.TryGetUnCompress(LayerMask.MaskTexture)); }

            if (Clipping) { layerStack.AddRtForClipping(this, rTex, BlendTypeKey); }
            else { layerStack.AddRenderTexture(this, rTex, BlendTypeKey); }
        }
    }
}
#endif