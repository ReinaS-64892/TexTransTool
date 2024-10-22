using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;

namespace net.rs64.TexTransTool.MultiLayerImage
{
    public abstract class AbstractGrabLayer : AbstractLayer
    {
        public virtual void GetImage<TTCE>(TTCE engine, ITTRenderTexture grabSource, ITTRenderTexture writeTarget)
        where TTCE : ITexTransGetTexture, ITexTranBlending
        {
            throw new NotImplementedException();
        }
        internal override LayerObject<TTT4U> GetLayerObject<TTT4U>(TTT4U engine)
        {
            return new TTTAbstractGrabLayerWarper<TTT4U>(Visible, GetAlphaMask(engine), Clipping, engine.QueryBlendKey(BlendTypeKey), this);
        }

        class TTTAbstractGrabLayerWarper<TTT4U> : GrabLayer<TTT4U>
        where TTT4U : ITexTransToolForUnity
        , ITexTransGetTexture
        , ITexTransLoadTexture
        , ITexTransRenderTextureOperator
        , ITexTransRenderTextureReScaler
        , ITexTranBlending
        {
            private AbstractGrabLayer _grabLayer;
            private ITTBlendKey _blendTypeKey;

            public TTTAbstractGrabLayerWarper(bool visible, AlphaMask<TTT4U> alphaMask, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, AbstractGrabLayer grabLayer) : base(visible, alphaMask, preBlendToLayerBelow)
            {
                _grabLayer = grabLayer;
                _blendTypeKey = blendTypeKey;
            }

            public override void GrabImage(TTT4U engine, EvaluateContext<TTT4U> evaluateContext, ITTRenderTexture grabTexture)
            {
                using (var tempDist = engine.CreateRenderTexture(grabTexture.Width, grabTexture.Hight))
                using (var tempTarget = engine.CreateRenderTexture(grabTexture.Width, grabTexture.Hight))
                using (var alphaBackup = engine.CreateRenderTexture(grabTexture.Width, grabTexture.Hight))
                {
                    engine.CopyRenderTexture(grabTexture, tempDist);
                    engine.CopyAlpha(grabTexture, alphaBackup);

                    engine.FillAlpha(tempDist, 1f);

                    _grabLayer.GetImage(engine, tempDist, tempTarget);
                    evaluateContext.AlphaMask.Masking(engine, tempTarget);

                    engine.FillAlpha(grabTexture, 1f);
                    engine.TextureBlend(grabTexture, tempTarget, _blendTypeKey);
                    engine.CopyAlpha(alphaBackup, grabTexture);
                }
            }
        }

    }
}
