using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    [AddComponentMenu("TexTransTool/MultiLayer/TTT LayerFolder")]
    internal class LayerFolder : AbstractLayer
    {
        public bool PassThrough;

        internal override void EvaluateTexture(CanvasContext canvasContext)
        {
            var layerStack = canvasContext.RootLayerStack;
            if (!Visible) { layerStack.Stack.Add(new(this, null, BlendTypeKey)); return; }
            var subContext = canvasContext.CreateSubCanvas;
            var Layers = transform.GetChildren()
            .Select(I => I.GetComponent<AbstractLayer>())
            .Reverse();
            foreach (var layer in Layers) { layer.EvaluateTexture(subContext); }

            var subStack = subContext.RootLayerStack;
            if (subStack.Stack.Count() == 0) { return; }
            if (!Clipping && PassThrough)
            {
                foreach (var layer in subStack.GetLayers)
                {
                    MultipleRenderTexture((RenderTexture)layer.Texture, new Color(1, 1, 1, Opacity));

                    if (!LayerMask.LayerMaskDisabled && LayerMask.MaskTexture != null) { MaskDrawRenderTexture((RenderTexture)layer.Texture, canvasContext.TextureManager.GetOriginalTexture2D(LayerMask.MaskTexture)); }
                    layerStack.AddRenderTexture(this, layer.Texture as RenderTexture, layer.BlendTypeKey);
                }
            }
            else
            {
                var rt = new RenderTexture(canvasContext.CanvasSize.x, canvasContext.CanvasSize.y, 0); rt.Clear();
                var first = subStack.Stack[0]; first.BlendTextures.BlendTypeKey = TTTBlendTypeKeyEnum.NotBlend.ToString(); subStack.Stack[0] = first;
                rt.BlendBlit(subStack.GetLayers);
                if (!Mathf.Approximately(Opacity, 1)) { MultipleRenderTexture(rt, new Color(1, 1, 1, Opacity)); }
                if (!LayerMask.LayerMaskDisabled && LayerMask.MaskTexture != null) { MaskDrawRenderTexture(rt, canvasContext.TextureManager.GetOriginalTexture2D(LayerMask.MaskTexture)); }

                if (Clipping) { layerStack.AddRtForClipping(this, rt, BlendTypeKey); }
                else { layerStack.AddRenderTexture(this, rt, PassThrough ? BL_KEY_DEFAULT : BlendTypeKey); }

                foreach (var layer in subStack.GetLayers) { DestroyImmediate(layer.Texture); }
            }

        }
    }
}