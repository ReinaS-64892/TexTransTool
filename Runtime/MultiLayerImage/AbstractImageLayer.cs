using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCoreEngineForUnity.Utils;
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
        public abstract void GetImage<TexTransCoreEngine>(TexTransCoreEngine engine, ITTRenderTexture renderTexture)
        where TexTransCoreEngine : ITexTransToolForUnity
        , ITexTransGetTexture
        , ITexTransLoadTexture
        , ITexTransRenderTextureOperator
        , ITexTransRenderTextureReScaler
        , ITexTranBlending;

        internal override LayerObject<TTT4U> GetLayerObject<TTT4U>(TTT4U engine)
        {
            var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
            return new TTTAbstractImageWarper<TTT4U>(Visible, GetAlphaMask(engine), alphaOperator, Clipping, engine.QueryBlendKey(BlendTypeKey), this);
        }

        class TTTAbstractImageWarper<TTT4U> : ImageLayer<TTT4U>
        where TTT4U : ITexTransToolForUnity
        , ITexTransGetTexture
        , ITexTransLoadTexture
        , ITexTransRenderTextureOperator
        , ITexTransRenderTextureReScaler
        , ITexTranBlending
        {
            private AbstractImageLayer _imageLayer;

            public TTTAbstractImageWarper(bool visible, AlphaMask<TTT4U> alphaMask, AlphaOperation alphaOperation, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, AbstractImageLayer imageLayer) : base(visible, alphaMask, alphaOperation, preBlendToLayerBelow, blendTypeKey)
            {
                _imageLayer = imageLayer;
            }


            public override void GetImage(TTT4U engine, ITTRenderTexture writeTarget)
            {
                _imageLayer.GetImage(engine, writeTarget);
            }
        }

    }
}
