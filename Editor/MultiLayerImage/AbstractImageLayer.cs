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
    /// <summary>
    /// これら class は非常に実験的なAPIで予告なく変更や削除される可能性があります。
    ///
    /// <see cref="MultiLayerImageCanvas"/> がレイヤーとして取り扱い、クリッピングやマスクなどの処理をしなくても、
    /// <see cref="GetImage"/>で Texture を返せばレイヤーとしてと差し込むことができます。
    /// <see cref="GetImage"/> の引数である 幅と高さはそのサイズでレンダリングする推奨値であり、必要でなければ無視してよい。
    /// </summary>
    public abstract class AbstractImageLayer : AbstractLayer
    {
        public abstract Texture GetImage(int width, int height);
        internal override void EvaluateTexture(CanvasContext canvasContext)
        {
            var layerStack = canvasContext.RootLayerStack;
            if (!Visible)
            {
                if (Clipping) { return; }
                else { layerStack.Stack.Add(new BlendLayer(this, null, BlendTypeKey)); return; }
            }
            var canvasSize = canvasContext.CanvasSize;
            var rTex = new RenderTexture(canvasSize.x, canvasSize.y, 0);

            var image = GetImage(canvasSize.x, canvasSize.y);
            if (image is Texture2D texture2D) { image = canvasContext.TextureManager.GetOriginalTexture2D(texture2D); }
            Graphics.Blit(image, rTex);

            if (!Mathf.Approximately(Opacity, 1)) { MultipleRenderTexture(rTex, new Color(1, 1, 1, Opacity)); }

            if (!LayerMask.LayerMaskDisabled && LayerMask.MaskTexture != null) { MaskDrawRenderTexture(rTex, canvasContext.TextureManager.GetOriginalTexture2D(LayerMask.MaskTexture)); }

            if (Clipping) { layerStack.AddRtForClipping(this, rTex, BlendTypeKey); }
            else { layerStack.AddRenderTexture(this, rTex, BlendTypeKey); }

            if (image is RenderTexture renderTexture) { DestroyImmediate(renderTexture); }
        }
    }
}
#endif