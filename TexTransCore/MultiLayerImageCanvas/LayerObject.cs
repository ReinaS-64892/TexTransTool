#nullable enable
using System;
using System.Collections.Generic;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{

    public abstract class LayerObject<TTCE>
    where TTCE : ITexTransGetTexture
    , ITexTransLoadTexture
    , ITexTransRenderTextureOperator
    , ITexTransRenderTextureReScaler
    , ITexTranBlending
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
    }
    /// <summary>
    /// マスクと Opacity を実現する存在。階層化されることもあるし、直接適用されることもある。
    /// </summary>
    public abstract class AlphaMask<TTCE>
    where TTCE : ITexTransGetTexture
    , ITexTransLoadTexture
    , ITexTransRenderTextureOperator
    , ITexTransRenderTextureReScaler
    {
        /// <summary>
        /// maskTarget の alpha 以外をいじってはならない。
        /// </summary>
        public abstract void Masking(TTCE engine, ITTRenderTexture maskTarget);
    }




    public class TextureToMask<TTCE> : AlphaMask<TTCE>
    where TTCE : ITexTransGetTexture
    , ITexTransLoadTexture
    , ITexTransRenderTextureOperator
    , ITexTransRenderTextureReScaler
    {
        ITTRenderTexture MaskTexture;

        public TextureToMask(ITTRenderTexture maskTexture)
        {
            MaskTexture = maskTexture;
        }
        public override void Masking(TTCE engine, ITTRenderTexture maskTarget) { engine.MulAlpha(maskTarget, MaskTexture); }
    }
}
