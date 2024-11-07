#nullable enable
namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class GrabBlendingAsLayer<TTCE> : GrabLayer<TTCE>
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    {
        ITTGrabBlending _grabBlendingObject;
        ITTBlendKey _blendTypeKey;
        public GrabBlendingAsLayer(bool visible, AlphaMask<TTCE> alphaMask, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, ITTGrabBlending grabBlending) : base(visible, alphaMask, preBlendToLayerBelow)
        {
            _grabBlendingObject = grabBlending;
            _blendTypeKey = blendTypeKey;
        }

        public override void GrabImage(TTCE engine, EvaluateContext<TTCE> evaluateContext, ITTRenderTexture grabTexture)
        {
            using (var tempTarget = engine.CreateRenderTexture(grabTexture.Width, grabTexture.Hight))
            using (var alphaBackup = engine.CreateRenderTexture(grabTexture.Width, grabTexture.Hight))
            {
                engine.CopyRenderTexture(tempTarget, grabTexture);


                engine.AlphaFill(tempTarget, 1f);
                _grabBlendingObject.GrabBlending(engine, tempTarget);
                evaluateContext.AlphaMask.Masking(engine, tempTarget);


                engine.AlphaCopy(alphaBackup, grabTexture);

                engine.AlphaFill(grabTexture, 1f);
                engine.Blending(grabTexture, tempTarget, _blendTypeKey);

                engine.AlphaCopy(grabTexture, alphaBackup);
            }
        }
    }
}
