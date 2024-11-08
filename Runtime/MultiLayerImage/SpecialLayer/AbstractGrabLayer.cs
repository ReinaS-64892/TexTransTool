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
        public virtual void GetImage<TTCE4U>(TTCE4U engine, ITTRenderTexture grabSource, ITTRenderTexture writeTarget)
        where TTCE4U : ITexTransCreateTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            throw new NotImplementedException();
        }
        internal override LayerObject<TTCE4U> GetLayerObject<TTCE4U>(TTCE4U engine)
        {
            return new TTTAbstractGrabLayerWarper<TTCE4U>(Visible, GetAlphaMask(engine), Clipping, engine.QueryBlendKey(BlendTypeKey), this);
        }

        class TTTAbstractGrabLayerWarper<TTCE4U> : GrabLayer<TTCE4U>
        where TTCE4U : ITexTransToolForUnity
        , ITexTransCreateTexture
        , ITexTransLoadTexture
        , ITexTransCopyRenderTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            private AbstractGrabLayer _grabLayer;
            private ITTBlendKey _blendTypeKey;

            public TTTAbstractGrabLayerWarper(bool visible, AlphaMask<TTCE4U> alphaMask, bool preBlendToLayerBelow, ITTBlendKey blendTypeKey, AbstractGrabLayer grabLayer) : base(visible, alphaMask, preBlendToLayerBelow)
            {
                _grabLayer = grabLayer;
                _blendTypeKey = blendTypeKey;
            }

            public override void GrabImage(TTCE4U engine, EvaluateContext<TTCE4U> evaluateContext, ITTRenderTexture grabTexture)
            {
                using var tempDist = engine.CreateRenderTexture(grabTexture.Width, grabTexture.Hight);
                using var tempTarget = engine.CreateRenderTexture(grabTexture.Width, grabTexture.Hight);
                using var alphaBackup = engine.CreateRenderTexture(grabTexture.Width, grabTexture.Hight);

                engine.CopyRenderTexture(grabTexture, tempDist);
                engine.AlphaCopy(alphaBackup, grabTexture);

                engine.AlphaFill(tempDist, 1f);

                _grabLayer.GetImage(engine, tempDist, tempTarget);
                evaluateContext.AlphaMask.Masking(engine, tempTarget);

                engine.AlphaFill(grabTexture, 1f);
                engine.Blending(grabTexture, tempTarget, _blendTypeKey);
                engine.AlphaCopy(grabTexture, alphaBackup);

            }
        }

    }
}
