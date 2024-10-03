#nullable enable
using System;
using System.Collections.Generic;

namespace net.rs64.TexTransCore.MultiLayerImageCanvas
{

    public abstract class LayerObject
    {
        public bool Visible;
        public bool PreBlendToLayerBelow;
        public AlphaMask AlphaMask;

        public LayerObject(bool visible, bool preBlendToLayerBelow, AlphaMask alphaMask)
        {
            Visible = visible;
            PreBlendToLayerBelow = preBlendToLayerBelow;
            AlphaMask = alphaMask;
        }
    }
    /// <summary>
    /// マスクと Opacity を実現する存在。階層化されることもあるし、直接適用されることもある。
    /// </summary>
    public abstract class AlphaMask
    {
        /// <summary>
        /// maskTarget の alpha 以外をいじってはならない。
        /// </summary>
        public abstract void Masking(ITTEngine engine, ITTRenderTexture maskTarget);
    }




    public class TextureToMask : AlphaMask
    {
        ITTRenderTexture MaskTexture;

        public TextureToMask(ITTRenderTexture maskTexture)
        {
            MaskTexture = maskTexture;
        }
        public override void Masking(ITTEngine engine, ITTRenderTexture maskTarget) { engine.MulAlpha(maskTarget, MaskTexture); }
    }
}
