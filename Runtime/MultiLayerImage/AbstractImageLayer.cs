using net.rs64.TexTransUnityCore;
using net.rs64.TexTransUnityCore.Utils;
using UnityEngine;
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
            if (!Visible) { canvasContext.LayerCanvas.AddHiddenLayer(Clipping, false); return; }

            var canvasSize = canvasContext.CanvasSize;
            var rTex = TTRt.G(canvasSize, canvasSize, true);
            rTex.name = $"AbstractImageLayer.EvaluateTextureTempRt-{rTex.width}x{rTex.height}";


            GetImage(rTex, canvasContext.TextureManager);

            var mask = GetLayerAlphaMod(canvasContext);
            canvasContext.LayerCanvas.AddLayer(new(rTex, BlendTypeKey), mask, Clipping);

        }


    }
}
