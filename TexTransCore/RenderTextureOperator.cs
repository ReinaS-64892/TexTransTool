#nullable enable
using System;
using System.Numerics;

namespace net.rs64.TexTransCore
{
    public static class RenderTextureOperator
    {
        // 引数の並びについて、
        // target , source の順にすること、

        /// <summary>
        /// Engine が提供するデフォルトのサンプラーを使ってリサイズする。
        /// 同一解像度の場合は例外を投げる
        /// </summary>
        public static void DefaultResizing<TTCE>(this TTCE engine, ITTRenderTexture targetTexture, ITTRenderTexture sourceTexture)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            if (sourceTexture.Width == targetTexture.Width && sourceTexture.Hight == targetTexture.Hight) { throw new ArgumentException(); }

            using var computeHandler = engine.GetComputeHandler(engine.ResizingSamplerKey[engine.StandardComputeKey.DefaultSampler]);

            var targetTexID = computeHandler.NameToID("TargetTex");
            var resizeTargetParmBufId = computeHandler.NameToID("ResizeTargetParm");

            var readTexID = computeHandler.NameToID("ReadTex");
            var readTextureParmBufId = computeHandler.NameToID("ReadTextureParm");

            Span<uint> resizeTargetBuf = stackalloc uint[4];
            resizeTargetBuf[0] = (uint)targetTexture.Width;
            resizeTargetBuf[1] = (uint)targetTexture.Hight;
            Span<uint> readTextureBuf = stackalloc uint[4];
            readTextureBuf[0] = (uint)sourceTexture.Width;
            readTextureBuf[1] = (uint)sourceTexture.Hight;

            computeHandler.UploadConstantsBuffer<uint>(resizeTargetParmBufId, resizeTargetBuf);
            computeHandler.UploadConstantsBuffer<uint>(readTextureParmBufId, readTextureBuf);

            computeHandler.SetTexture(targetTexID, targetTexture);
            computeHandler.SetTexture(readTexID, sourceTexture);

            computeHandler.DispatchWithTextureSize(targetTexture);
        }

        public static void AlphaFill<TTCE>(this TTCE engine, ITTRenderTexture renderTexture, float alpha)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.AlphaFill);

            var texID = computeHandler.NameToID("Tex");
            var gvBufId = computeHandler.NameToID("gv");

            Span<float> gvBuf = stackalloc float[1];
            gvBuf[0] = alpha;
            computeHandler.UploadConstantsBuffer<float>(gvBufId, gvBuf);

            computeHandler.SetTexture(texID, renderTexture);

            computeHandler.DispatchWithTextureSize(renderTexture);
        }
        public static void AlphaMultiply<TTCE>(this TTCE engine, ITTRenderTexture renderTexture, float value)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.AlphaMultiply);

            var texID = computeHandler.NameToID("Tex");
            var gvBufId = computeHandler.NameToID("gv");

            Span<float> gvBuf = stackalloc float[1];
            gvBuf[0] = value;
            computeHandler.UploadConstantsBuffer<float>(gvBufId, gvBuf);

            computeHandler.SetTexture(texID, renderTexture);

            computeHandler.DispatchWithTextureSize(renderTexture);
        }
        /// <summary>
        /// target.a = target.a * source.a
        /// 同じ大きさでないといけない。
        /// </summary>
        public static void AlphaMultiplyWithTexture<TTCE>(this TTCE engine, ITTRenderTexture target, ITTRenderTexture source)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            if (source.Width != target.Width || source.Hight != target.Hight) { throw new ArgumentException(); }
            using var computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.AlphaMultiplyWithTexture);

            var sourceTexID = computeHandler.NameToID("SourceTex");
            var targetTexID = computeHandler.NameToID("TargetTex");

            computeHandler.SetTexture(sourceTexID, source);
            computeHandler.SetTexture(targetTexID, target);

            computeHandler.DispatchWithTextureSize(target);
        }

        /// <summary>
        /// 同じ大きさでないといけない。
        /// </summary>
        public static void AlphaCopy<TTCE>(this TTCE engine, ITTRenderTexture target, ITTRenderTexture source)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            if (source.Width != target.Width || source.Hight != target.Hight) { throw new ArgumentException(); }
            using var computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.AlphaCopy);

            var sourceTexID = computeHandler.NameToID("SourceTex");
            var targetTexID = computeHandler.NameToID("TargetTex");

            computeHandler.SetTexture(sourceTexID, source);
            computeHandler.SetTexture(targetTexID, target);

            computeHandler.DispatchWithTextureSize(target);
        }




        /// <summary>
        /// 一色でそのレンダーテクスチャーを染めます。
        /// </summary>
        public static void ColorFill<TTCE>(this TTCE engine, ITTRenderTexture target, Color color)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.ColorFill);

            var texID = computeHandler.NameToID("Tex");
            var gvBufId = computeHandler.NameToID("gv");

            Span<Color> gvBuf = stackalloc Color[1];
            gvBuf[0] = color;
            computeHandler.UploadConstantsBuffer<Color>(gvBufId, gvBuf);

            computeHandler.SetTexture(texID, target);

            computeHandler.DispatchWithTextureSize(target);
        }
        /// <summary>
        /// その色でレンダーテクスチャーを乗算します
        /// </summary>
        public static void ColorMultiply<TTCE>(this TTCE engine, ITTRenderTexture target, Color color)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.ColorMultiply);

            var texID = computeHandler.NameToID("Tex");
            var gvBufId = computeHandler.NameToID("gv");

            Span<Color> gvBuf = stackalloc Color[1];
            gvBuf[0] = color;
            computeHandler.UploadConstantsBuffer<Color>(gvBufId, gvBuf);

            computeHandler.SetTexture(texID, target);

            computeHandler.DispatchWithTextureSize(target);
        }

        public static void GammaToLinear<TTCE>(this TTCE engine, ITTRenderTexture target)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.GammaToLinear);

            var texID = computeHandler.NameToID("Tex");

            computeHandler.SetTexture(texID, target);
            computeHandler.DispatchWithTextureSize(target);
        }
        public static void LinearToGamma<TTCE>(this TTCE engine, ITTRenderTexture target)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.LinearToGamma);

            var texID = computeHandler.NameToID("Tex");

            computeHandler.SetTexture(texID, target);
            computeHandler.DispatchWithTextureSize(target);
        }
        public static void Swizzling<TTCE>(this TTCE engine, ITTRenderTexture target, SwizzlingChannel r = SwizzlingChannel.R, SwizzlingChannel g = SwizzlingChannel.G, SwizzlingChannel b = SwizzlingChannel.B, SwizzlingChannel a = SwizzlingChannel.A)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.Swizzling);

            var texID = computeHandler.NameToID("Tex");
            var gvBufId = computeHandler.NameToID("gv");

            Span<SwizzlingChannel> gvBuf = stackalloc SwizzlingChannel[4];
            gvBuf[0] = r;
            gvBuf[1] = g;
            gvBuf[2] = b;
            gvBuf[3] = a;
            computeHandler.UploadConstantsBuffer<SwizzlingChannel>(gvBufId, gvBuf);

            computeHandler.SetTexture(texID, target);
            computeHandler.DispatchWithTextureSize(target);
        }

        public enum SwizzlingChannel : uint
        {
            R = 0,
            G = 1,
            B = 2,
            A = 3,
        }

        public static void FillR<TTCE>(this TTCE engine,ITTRenderTexture t, float v)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.FillR);

            var texID = computeHandler.NameToID("Tex");
            var gvBufId = computeHandler.NameToID("gv");

            Span<float> gvBuf = stackalloc float[4];
            gvBuf[0] = v;
            computeHandler.UploadConstantsBuffer<float>(gvBufId, gvBuf);
            computeHandler.SetTexture(texID, t);

            computeHandler.DispatchWithTextureSize(t);
        }
        public static void FillRG<TTCE>(this TTCE engine,ITTRenderTexture t, Vector2 v)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.FillRG);

            var texID = computeHandler.NameToID("Tex");
            var gvBufId = computeHandler.NameToID("gv");

            Span<float> gvBuf = stackalloc float[4];
            gvBuf[0] = v.X;
            gvBuf[1] = v.Y;
            computeHandler.UploadConstantsBuffer<float>(gvBufId, gvBuf);
            computeHandler.SetTexture(texID, t);

            computeHandler.DispatchWithTextureSize(t);
        }
        public static void FillROnly<TTCE>(this TTCE engine,ITTRenderTexture t, float v)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.FillROnly);

            var texID = computeHandler.NameToID("Tex");
            var gvBufId = computeHandler.NameToID("gv");

            Span<float> gvBuf = stackalloc float[4];
            gvBuf[0] = v;
            computeHandler.UploadConstantsBuffer<float>(gvBufId, gvBuf);
            computeHandler.SetTexture(texID, t);

            computeHandler.DispatchWithTextureSize(t);
        }
        public static void FillGOnly<TTCE>(this TTCE engine,ITTRenderTexture t, float v)
        where TTCE : ITexTransComputeKeyQuery, ITexTransGetComputeHandler
        {
            using var computeHandler = engine.GetComputeHandler(engine.StandardComputeKey.FillGOnly);

            var texID = computeHandler.NameToID("Tex");
            var gvBufId = computeHandler.NameToID("gv");

            Span<float> gvBuf = stackalloc float[4];
            gvBuf[0] = v;
            computeHandler.UploadConstantsBuffer<float>(gvBufId, gvBuf);
            computeHandler.SetTexture(texID, t);

            computeHandler.DispatchWithTextureSize(t);
        }

        public static void ClopImage<T>(Span<T> dist, (int x, int y) distSize, Span<T> source, (int x, int y) sourceSize, int pixelParCount) where T : unmanaged
        {
            for (var y = 0; distSize.y > y; y += 1)
                for (var x = 0; distSize.x > x; x += 1)
                {
                    var writePos = (x * pixelParCount) + (y * distSize.x * pixelParCount);
                    var readPos = (x * pixelParCount) + (y * sourceSize.x * pixelParCount);

                    for (var i = 0; pixelParCount > i; i += 1) { dist[writePos + i] = source[readPos + 1]; }
                }
        }
    }
}
