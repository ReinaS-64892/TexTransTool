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
    , ITexTransDriveStorageBufferHolder
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
    , ITexTransDriveStorageBufferHolder
    {

        /// <summary>
        /// maskTarget の alpha 以外をいじってはならない。
        /// </summary>
        public abstract void Masking(TTCE engine, ITTRenderTexture maskTarget);
        public abstract void Dispose();
    }

    public class NotMask<TTCE> : AlphaMask<TTCE>
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    , ITexTransDriveStorageBufferHolder
    {
        public NotMask() { }
        public override void Masking(TTCE engine, ITTRenderTexture maskTarget) { }
        public override void Dispose() { }
    }

    public class SolidToMask<TTCE> : AlphaMask<TTCE>
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    , ITexTransDriveStorageBufferHolder
    {
        float _maskValue;
        public SolidToMask(float maskValue) { _maskValue = maskValue; }
        public override void Masking(TTCE engine, ITTRenderTexture maskTarget) { engine.AlphaMultiply(maskTarget, _maskValue); }
        public override void Dispose() { }
    }



    public class DiskToMask<TTCE> : AlphaMask<TTCE>
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    , ITexTransDriveStorageBufferHolder
    {
        float _maskValue;
        ITTDiskTexture _maskTexture;

        public DiskToMask(ITTDiskTexture maskTexture, float maskValue) { _maskTexture = maskTexture; _maskValue = maskValue; }
        public override void Masking(TTCE engine, ITTRenderTexture maskTarget)
        {
            engine.AlphaMultiply(maskTarget, _maskValue);
            using var maskTemp = engine.CreateRenderTexture(maskTarget.Width, maskTarget.Hight);
            engine.LoadTextureWidthAnySize(maskTemp, _maskTexture);
            engine.AlphaMultiplyWithTexture(maskTarget, maskTemp);
        }
        public override void Dispose() { _maskTexture.Dispose(); }
    }



    public class TextureToMask<TTCE> : AlphaMask<TTCE>
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    , ITexTransDriveStorageBufferHolder
    {
        float _maskValue;
        ITTRenderTexture _maskTexture;

        public TextureToMask(ITTRenderTexture maskTexture, float maskValue) { _maskTexture = maskTexture; _maskValue = maskValue; }
        public override void Masking(TTCE engine, ITTRenderTexture maskTarget)
        {
            engine.AlphaMultiply(maskTarget, _maskValue);
            engine.AlphaMultiplyWithTexture(maskTarget, _maskTexture);
        }
        public override void Dispose() { _maskTexture.Dispose(); }
    }
    public class TextureOnlyToMask<TTCE> : AlphaMask<TTCE>
    where TTCE : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    , ITexTransDriveStorageBufferHolder
    {
        ITTRenderTexture _maskTexture;

        public TextureOnlyToMask(ITTRenderTexture maskTexture) { _maskTexture = maskTexture; }
        public override void Masking(TTCE engine, ITTRenderTexture maskTarget) { engine.AlphaMultiplyWithTexture(maskTarget, _maskTexture); }
        public override void Dispose() { _maskTexture.Dispose(); }
    }
}
