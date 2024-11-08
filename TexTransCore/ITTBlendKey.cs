#nullable enable
using System;

namespace net.rs64.TexTransCore
{
    public interface ITTBlendKey { }
    public static class TexTransBlending
    {
        public static void Blending<TTCE>(this TTCE engine, ITTRenderTexture dist, ITTRenderTexture add, ITTBlendKey blendKey)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            if (dist.Width != add.Width || dist.Hight != add.Hight) { throw new ArgumentException(); }
            using (var computeHandler = engine.GetComputeHandler(engine.BlendKey[blendKey]))
            {
                var addTexID = computeHandler.NameToID("AddTex");
                var distTexID = computeHandler.NameToID("DistTex");

                computeHandler.SetTexture(addTexID, add);
                computeHandler.SetTexture(distTexID, dist);

                computeHandler.DispatchWithTextureSize(dist);
            }
        }

        public static void BlendingWithAnySize<TTCE>(this TTCE engine, ITTRenderTexture dist, ITTRenderTexture add, ITTBlendKey blendKey)
        where TTCE : ITexTransCreateTexture, ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            if (dist.Width == add.Width && dist.Hight == add.Hight) { engine.Blending(dist, add, blendKey); return; }

            using var resizeTemp = engine.CreateRenderTexture(dist.Width, dist.Hight);
            engine.BilinearReScaling(resizeTemp, add);
            engine.Blending(dist, resizeTemp, blendKey);
        }
    }

}
