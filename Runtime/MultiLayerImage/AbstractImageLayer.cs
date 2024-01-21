using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    /// <summary>
    /// これら class は非常に実験的なAPIで予告なく変更や削除される可能性があります。
    ///
    /// <see cref="MultiLayerImageCanvas"/> がレイヤーとして取り扱い、クリッピングやマスクなどの処理をしなくても、
    /// <see cref="GetImage"/>で Texture を返せばレイヤーとしてと差し込むことができます。
    /// <see cref="GetImage"/> の引数である レンダーテクスチャーにテクスチャを書き込んでね、
    /// <see cref="IOriginTexture"/> は元画像へのアクセスができるものだけど、プレビューだと例外とかはないけど機能しないよ。
    /// </summary>
    public abstract class AbstractImageLayer : AbstractLayer
    {
        public abstract void GetImage(RenderTexture renderTexture, IOriginTexture originTexture);
        internal override void EvaluateTexture(CanvasContext canvasContext)
        {
            var layerStack = canvasContext.RootLayerStack;
            if (!Visible)
            {
                if (Clipping) { return; }
                else { layerStack.Stack.Add(new BlendLayer(this, null, BlendTypeKey)); return; }
            }
            var canvasSize = canvasContext.CanvasSize;
            var rTex = RenderTexture.GetTemporary(canvasSize, canvasSize, 0); rTex.Clear();

            GetImage(rTex, canvasContext.TextureManager);

            if (!Mathf.Approximately(Opacity, 1)) { MultipleRenderTexture(rTex, new Color(1, 1, 1, Opacity)); }

            if (LayerMask.ContainedMask)
            {
                var tmpRtMask = RenderTexture.GetTemporary(canvasSize, canvasSize, 0);
                LayerMask.WriteMaskTexture(tmpRtMask, canvasContext.TextureManager);
                MaskDrawRenderTexture(rTex, tmpRtMask); RenderTexture.ReleaseTemporary(tmpRtMask);
            }

            if (Clipping) { layerStack.AddRtForClipping(this, rTex, BlendTypeKey); }
            else { layerStack.AddRenderTexture(this, rTex, BlendTypeKey); }

        }
    }
}