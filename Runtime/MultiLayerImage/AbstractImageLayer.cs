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
        public abstract void GetImage(ITexTransToolForUnity engine, ITTRenderTexture renderTexture);

        internal override LayerObject<ITexTransToolForUnity> GetLayerObject(GenerateLayerObjectContext ctx)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;
            domain.Observe(this);// 個別にやるべきか ... 否か ... ?
            domain.Observe(gameObject);

            var alphaMask = GetAlphaMaskObject(ctx);
            var alphaOperator = Clipping ? AlphaOperation.Inherit : AlphaOperation.Normal;
            var blendTypeKey = engine.QueryBlendKey(BlendTypeKey);

            return new TTTAbstractImageWarper(Visible, alphaMask, alphaOperator, Clipping, blendTypeKey, this);
        }

        class TTTAbstractImageWarper : ImageLayer<ITexTransToolForUnity>
        {
            private AbstractImageLayer _imageLayer;
            public TTTAbstractImageWarper(
                bool visible
                , AlphaMask<ITexTransToolForUnity> alphaMask
                , AlphaOperation alphaOperation
                , bool preBlendToLayerBelow
                , ITTBlendKey blendTypeKey
                , AbstractImageLayer imageLayer
                ) : base(visible, alphaMask, alphaOperation, preBlendToLayerBelow, blendTypeKey)
            { _imageLayer = imageLayer; }

            public override void GetImage(ITexTransToolForUnity engine, ITTRenderTexture writeTarget) { _imageLayer.GetImage(engine, writeTarget); }
        }

    }
}
