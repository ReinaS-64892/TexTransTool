using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransCoreForUnity;
using net.rs64.TexTransCoreForUnity.Utils;
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

        internal override LayerObject GetLayerObject(ITexTransToolEngine engine, ITextureManager textureManager)
        {
            var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
            return new TTTAbstractImageWarper(Visible, GetAlphaMask(textureManager), alphaOperator, Clipping, engine.QueryBlendKey(BlendTypeKey), this, textureManager);
        }

        class TTTAbstractImageWarper : ImageLayer
        {
            private AbstractImageLayer _imageLayer;
            private ITextureManager _textureManager;

            public TTTAbstractImageWarper(bool visible, AlphaMask alphaMask, AlphaOperation alphaOperation, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, AbstractImageLayer imageLayer, ITextureManager textureManager) : base(visible, alphaMask, alphaOperation, preBlendToLayerBelow, blendTypeKey)
            {
                _imageLayer = imageLayer;
                _textureManager = textureManager;
            }

            public override void GetImage(ITexTransCoreEngine engine, ITTRenderTexture writeTarget)
            {
                _imageLayer.GetImage(writeTarget.ToUnity(), _textureManager);
            }
        }

    }
}
