#nullable enable
namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{
    public class GrabBlendingAsLayer : GrabLayer
    {
        TTGrabBlending _grabBlendingObject;
        ITTBlendKey _blendTypeKey;
        public GrabBlendingAsLayer(bool visible, AlphaMask alphaMask, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, TTGrabBlending grabBlending) : base(visible, alphaMask, preBlendToLayerBelow)
        {
            _grabBlendingObject = grabBlending;
            _blendTypeKey = blendTypeKey;
        }

        public override void GrabImage(ITTEngine engine, EvaluateContext evaluateContext, ITTRenderTexture grabTexture)
        {
            using (var tempTarget = engine.CreateRenderTexture(grabTexture.Width, grabTexture.Hight))
            using (var alphaBackup = engine.CreateRenderTexture(grabTexture.Width, grabTexture.Hight))
            {
                engine.CopyRenderTexture(grabTexture, tempTarget);


                engine.FillAlpha(tempTarget, 1f);
                engine.GrabBlending(tempTarget, _grabBlendingObject);
                evaluateContext.AlphaMask.Masking(engine, tempTarget);


                engine.CopyAlpha(grabTexture, alphaBackup);

                engine.FillAlpha(grabTexture, 1f);
                engine.TextureBlend(grabTexture, tempTarget, _blendTypeKey);

                engine.CopyAlpha(alphaBackup, grabTexture);
            }
        }
    }
}
