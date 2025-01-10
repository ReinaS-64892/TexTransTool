#nullable enable
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;

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
        public abstract void GetImage<TTCE4U>(TTCE4U engine, ITTRenderTexture renderTexture)
        where TTCE4U : ITexTransToolForUnity
        , ITexTransCreateTexture
        , ITexTransLoadTexture
        , ITexTransCopyRenderTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler;

        internal override LayerObject<TTCE4U> GetLayerObject<TTCE4U>(TTCE4U engine)
        {
            var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
            return new TTTAbstractImageWarper<TTCE4U>(Visible, GetAlphaMask(engine), alphaOperator, Clipping, engine.QueryBlendKey(BlendTypeKey), this);
        }

        class TTTAbstractImageWarper<TTCE4U> : ImageLayer<TTCE4U>
        where TTCE4U : ITexTransToolForUnity
        , ITexTransCreateTexture
        , ITexTransLoadTexture
        , ITexTransCopyRenderTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            private AbstractImageLayer _imageLayer;

            public TTTAbstractImageWarper(bool visible, AlphaMask<TTCE4U> alphaMask, AlphaOperation alphaOperation, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, AbstractImageLayer imageLayer) : base(visible, alphaMask, alphaOperation, preBlendToLayerBelow, blendTypeKey)
            {
                _imageLayer = imageLayer;
            }


            public override void GetImage(TTCE4U engine, ITTRenderTexture writeTarget)
            {
                _imageLayer.GetImage(engine, writeTarget);
            }
        }

    }
}
