using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransUnityCore;
using UnityEngine;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    public abstract class AbstractGrabLayer : AbstractLayer
    {
        public abstract void GetImage(RenderTexture grabSource, RenderTexture writeTarget, IOriginTexture originTexture);
        internal override LayerObject GetLayerObject(ITexTransToolEngine engine, ITextureManager textureManager)
        {
            return new TTTAbstractGrabLayerWarper(Visible, GetAlphaMask(textureManager), Clipping, BlendTypeKey.ToTTUnityEngin(), this, textureManager);
        }

        class TTTAbstractGrabLayerWarper : GrabLayer
        {
            private AbstractGrabLayer _grabLayer;
            private ITextureManager _textureManager;
            private TTBlendUnityObject _blendTypeKey;

            public TTTAbstractGrabLayerWarper(bool visible, AlphaMask alphaMask, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, AbstractGrabLayer grabLayer, ITextureManager textureManager) : base(visible, alphaMask, preBlendToLayerBelow)
            {
                _grabLayer = grabLayer;
                _textureManager = textureManager;
                _blendTypeKey = blendTypeKey.ToUnity();
            }

            public override void GrabImage(ITTEngine engine, EvaluateContext evaluateContext, ITTRenderTexture grabTexture)
            {
                using (var tempDist = engine.CreateRenderTexture(grabTexture.Width, grabTexture.Hight))
                using (var tempTarget = engine.CreateRenderTexture(grabTexture.Width, grabTexture.Hight))
                using (var alphaBackup = engine.CreateRenderTexture(grabTexture.Width, grabTexture.Hight))
                {
                    engine.CopyRenderTexture(grabTexture, tempDist);
                    engine.CopyAlpha(grabTexture, alphaBackup);

                    engine.FillAlpha(tempDist, 1f);

                    _grabLayer.GetImage(tempDist.ToUnity(), tempTarget.ToUnity(), _textureManager);
                    evaluateContext.AlphaMask.Masking(engine, tempTarget);

                    engine.FillAlpha(grabTexture, 1f);
                    engine.TextureBlend(grabTexture, tempTarget, _blendTypeKey);
                    engine.CopyAlpha(alphaBackup, grabTexture);
                }
            }
        }

    }
}
