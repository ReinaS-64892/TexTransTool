#nullable enable
using System;
using System.Collections.Generic;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{

    public abstract class LayerObject<TTCE> : IDisposable
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    {
        public bool Visible;
        public bool PreBlendToLayerBelow;
        public AlphaMask<TTCE> AlphaMask;

        public LayerObject(bool visible, AlphaMask<TTCE> alphaMask, bool preBlendToLayerBelow)
        {
            Visible = visible;
            PreBlendToLayerBelow = preBlendToLayerBelow;
            AlphaMask = alphaMask;
        }

        public virtual void Dispose()
        {
            AlphaMask.Dispose();
        }
    }
    /// <summary>
    /// マスクと Opacity を実現する存在。階層化されることもあるし、直接適用されることもある。
    /// </summary>
    public abstract class AlphaMask<TTCE> : IDisposable
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    {

        /// <summary>
        /// maskTarget の alpha 以外をいじってはならない。
        /// </summary>
        public abstract void Masking(TTCE engine, ITTRenderTexture maskTarget);
        public abstract void Dispose();
    }




    public class TextureToMask<TTCE> : AlphaMask<TTCE>
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    {
        ITTRenderTexture MaskTexture;

        public TextureToMask(ITTRenderTexture maskTexture)
        {
            MaskTexture = maskTexture;
        }

        public override void Masking(TTCE engine, ITTRenderTexture maskTarget) { engine.AlphaMultiplyWithTexture(maskTarget, MaskTexture); }

        public override void Dispose()
        {
            MaskTexture.Dispose();
        }
    }
}
