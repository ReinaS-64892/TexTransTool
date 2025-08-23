#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.TTMathUtil;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Color = net.rs64.TexTransCore.Color;

namespace net.rs64.TexTransCoreEngineForUnity
{
    internal class TTCEUnity : ITexTransCreateTexture
    , ITexTransCopyRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    , ITexTransDriveStorageBufferHolder
    , ITexTransRenderTextureIO
    {
        public ITTRenderTexture CreateRenderTexture(int width, int height, TexTransCoreTextureChannel channel = TexTransCoreTextureChannel.RGBA)
        {
            return new UnityRenderTexture(width, height, channel);
        }

        public void CopyRenderTexture(ITTRenderTexture target, ITTRenderTexture source)
        {
            if (source.Width != target.Width || source.Hight != target.Hight) { throw new ArgumentException("Texture size is not equal!"); }
            if (target.ContainsChannel != source.ContainsChannel) { throw new ArgumentException("Texture channel is not equal!"); }

            if (source is not UnityRenderTexture urtS) { throw new InvalidOperationException(); }
            if (target is not UnityRenderTexture urtT) { throw new InvalidOperationException(); }
            Graphics.CopyTexture(urtS.RenderTexture, urtT.RenderTexture);
        }

        public void UploadTexture<T>(ITTRenderTexture uploadTarget, ReadOnlySpan<T> bytes, TexTransCoreTextureFormat format) where T : unmanaged
        {
            var tex = new Texture2D(uploadTarget.Width, uploadTarget.Hight, format.ToUnityTextureFormat(uploadTarget.ContainsChannel), false, false);

            using var na = new NativeArray<T>(bytes.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            bytes.CopyTo(na);
            tex.LoadRawTextureData(na);

            tex.Apply();
            if (uploadTarget is not UnityRenderTexture uploadTargetUrt) { throw new InvalidOperationException(); }
            Graphics.CopyTexture(tex, uploadTargetUrt.RenderTexture);
            UnityEngine.Object.DestroyImmediate(tex);
        }
        public void DownloadTexture<T>(Span<T> dataDist, TexTransCoreTextureFormat format, ITTRenderTexture renderTexture) where T : unmanaged
        {
            if (renderTexture is not UnityRenderTexture urt) { throw new InvalidOperationException(); }
            if (urt.RenderTexture.graphicsFormat == format.ToUnityGraphicsFormat(renderTexture.ContainsChannel))
            {
                urt.RenderTexture.DownloadFromRenderTexture(dataDist);
            }
            else
            {
                var cfRt = new RenderTexture(renderTexture.Width, renderTexture.Hight, 0, format.ToUnityGraphicsFormat(renderTexture.ContainsChannel));
                Graphics.Blit(urt.RenderTexture, cfRt);
                cfRt.DownloadFromRenderTexture(dataDist);
            }
        }
        public ITTComputeHandler GetComputeHandler(ITTComputeKey computeKey)
        {
            switch (computeKey)
            {
                case TTGeneralComputeOperator generalComputeOperator: { return new TTUnityComputeHandler(generalComputeOperator.Compute); }
                case TTBlendingComputeShader blendingComputeShader: { return new TTUnityComputeHandler(blendingComputeShader.Compute); }
                case TTGrabBlendingComputeShader grabBlendingComputeShader: { return new TTUnityComputeHandler(grabBlendingComputeShader.Compute); }
                case ComputeKeyHolder holder: { return new TTUnityComputeHandler(holder.ComputeShader); }
            }
            throw new ArgumentException();
        }
        public ITTStorageBuffer AllocateStorageBuffer<T>(int length, bool downloadable = false) where T : unmanaged
        { return new TTUnityComputeHandler.TTUnityStorageBuffer(length, UnsafeUtility.SizeOf<T>(), downloadable); }
        public ITTStorageBuffer UploadStorageBuffer<T>(ReadOnlySpan<T> data, bool downloadable = false) where T : unmanaged
        {
            var stride = UnsafeUtility.SizeOf<T>();
            if((stride % 4) is not 0){throw new InvalidProgramException("must be a multiple of 4 !!!");}
            var length = data.Length * stride;
            var holder = new TTUnityComputeHandler.TTUnityStorageBuffer(length, stride, downloadable);
#if UNITY_EDITOR
            unsafe
            {
                fixed (T* ptr = data)
                {
                    var na = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, data.Length, Allocator.None);
                    NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref na, AtomicSafetyHandle.GetTempMemoryHandle());
                    holder._buffer!.SetData(na);
                }
            }
#else
            using var na = new NativeArray<T>(data.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            data.CopyTo(na.AsSpan());
            holder._buffer!.SetData(na);
#endif

            return holder;
        }

        public void DownloadBuffer<T>(Span<T> dist, ITTStorageBuffer takeToFrom) where T : unmanaged
        {
            using var holder = (TTUnityComputeHandler.TTUnityStorageBuffer)takeToFrom;

            if (holder._buffer is null) { throw new NullReferenceException(); }
            if (holder._downloadable is false) { throw new InvalidOperationException(); }

#if UNITY_EDITOR
            unsafe
            {
                fixed (T* ptr = dist)
                {
                    var na = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, dist.Length, Allocator.None);
                    NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref na, AtomicSafetyHandle.GetTempMemoryHandle());
                    var request = AsyncGPUReadback.RequestIntoNativeArray(ref na, holder._buffer);
                    request.WaitForCompletion();
                }
            }
#else
            var req = AsyncGPUReadback.Request(holder._buffer);
            req.WaitForCompletion();
            req.GetData<T>().AsSpan().CopyTo(dist);

#endif
        }

        public ITexTransStandardComputeKey StandardComputeKey => ComputeObjectUtility.UStdHolder;
        public TExKeyQ GetExKeyQuery<TExKeyQ>() where TExKeyQ : ITTExtraComputeKeyQuery
        {
            if (ComputeObjectUtility.UStdHolder is not TExKeyQ exKeyQ) { throw new ComputeKeyInterfaceIsNotImplementException($"{GetType().Name} is not supported {typeof(TExKeyQ).GetType().Name}."); }
            return exKeyQ;
        }

    }


    internal class UnityRenderTexture : ITTRenderTexture
    {
        internal RenderTexture RenderTexture;
        internal TexTransCoreTextureChannel Channel;
        public UnityRenderTexture(int width, int height, TexTransCoreTextureChannel channel = TexTransCoreTextureChannel.RGBA)
        {
            Channel = channel;
            RenderTexture = TTRt2.Get(width, height, channel);
        }
        public bool IsDepthAndStencil => RenderTexture.depth != 0;

        public int Width => RenderTexture.width;

        public int Hight => RenderTexture.height;

        public string Name { get => RenderTexture.name; set => RenderTexture.name = value; }

        public TexTransCoreTextureChannel ContainsChannel => Channel;

        public void Dispose() { TTRt2.Rel(RenderTexture); }
    }
    internal static class TTCoreEnginTypeUtil
    {
        public static UnityEngine.Color ToUnity(this Color color) { return new(color.R, color.G, color.B, color.A); }
        public static Color ToTTCore(this UnityEngine.Color color) { return new(color.r, color.g, color.b, color.a); }
        public static ColorWOAlpha ToTTCoreWOAlpha(this UnityEngine.Color color) { return new(color.r, color.g, color.b); }
        public static UnityEngine.Color ToUnity(this ColorWOAlpha color, float alpha = 1f) { return new(color.R, color.G, color.B, alpha); }
        public static UnityEngine.Vector2 ToUnity(this System.Numerics.Vector2 vec) { return new(vec.X, vec.Y); }
        public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 vec) { return new(vec.X, vec.Y, vec.Z); }
        public static UnityEngine.Vector4 ToUnity(this System.Numerics.Vector4 vec) { return new(vec.X, vec.Y, vec.Z, vec.W); }
        public static System.Numerics.Vector4 ToSysNum(this UnityEngine.Vector4 vec) { return new(vec.x, vec.y, vec.z, vec.w); }
        public static System.Numerics.Vector3 ToSysNum(this UnityEngine.Vector3 vec) { return new(vec.x, vec.y, vec.z); }
        public static System.Numerics.Vector2 ToSysNum(this UnityEngine.Vector2 vec) { return new(vec.x, vec.y); }
        public static RayIntersect.Ray ToTTCore(this UnityEngine.Ray ray) { return new() { Position = ray.origin.ToSysNum(), Direction = ray.direction.ToSysNum() }; }

        internal static TextureFormat ToUnityTextureFormat(this TexTransCoreTextureFormat format, TexTransCoreTextureChannel channel = TexTransCoreTextureChannel.RGBA)
        {
            switch (format, channel)
            {
                default: throw new ArgumentOutOfRangeException(format.ToString() + "-" + channel.ToString());
                case (TexTransCoreTextureFormat.Byte, TexTransCoreTextureChannel.R): return TextureFormat.R8;
                case (TexTransCoreTextureFormat.Byte, TexTransCoreTextureChannel.RG): return TextureFormat.RG16;
                case (TexTransCoreTextureFormat.Byte, TexTransCoreTextureChannel.RGBA): return TextureFormat.RGBA32;

                case (TexTransCoreTextureFormat.UShort, TexTransCoreTextureChannel.R): return TextureFormat.R16;
                case (TexTransCoreTextureFormat.UShort, TexTransCoreTextureChannel.RG): return TextureFormat.RG32;
                case (TexTransCoreTextureFormat.UShort, TexTransCoreTextureChannel.RGBA): return TextureFormat.RGBA64;

                case (TexTransCoreTextureFormat.Half, TexTransCoreTextureChannel.R): return TextureFormat.RHalf;
                case (TexTransCoreTextureFormat.Half, TexTransCoreTextureChannel.RG): return TextureFormat.RGHalf;
                case (TexTransCoreTextureFormat.Half, TexTransCoreTextureChannel.RGBA): return TextureFormat.RGBAHalf;

                case (TexTransCoreTextureFormat.Float, TexTransCoreTextureChannel.R): return TextureFormat.RFloat;
                case (TexTransCoreTextureFormat.Float, TexTransCoreTextureChannel.RG): return TextureFormat.RGFloat;
                case (TexTransCoreTextureFormat.Float, TexTransCoreTextureChannel.RGBA): return TextureFormat.RGBAFloat;
            }
        }
        internal static (TexTransCoreTextureFormat format, TexTransCoreTextureChannel channel) ToTTCTextureFormat(this TextureFormat format)
        {
            switch (format)
            {
                default: throw new ArgumentOutOfRangeException(format.ToString());
                case TextureFormat.R8: return (TexTransCoreTextureFormat.Byte, TexTransCoreTextureChannel.R);
                case TextureFormat.RG16: return (TexTransCoreTextureFormat.Byte, TexTransCoreTextureChannel.RG);
                case TextureFormat.RGBA32: return (TexTransCoreTextureFormat.Byte, TexTransCoreTextureChannel.RGBA);

                case TextureFormat.R16: return (TexTransCoreTextureFormat.UShort, TexTransCoreTextureChannel.R);
                case TextureFormat.RG32: return (TexTransCoreTextureFormat.UShort, TexTransCoreTextureChannel.RG);
                case TextureFormat.RGBA64: return (TexTransCoreTextureFormat.UShort, TexTransCoreTextureChannel.RGBA);

                case TextureFormat.RHalf: return (TexTransCoreTextureFormat.Half, TexTransCoreTextureChannel.R);
                case TextureFormat.RGHalf: return (TexTransCoreTextureFormat.Half, TexTransCoreTextureChannel.RG);
                case TextureFormat.RGBAHalf: return (TexTransCoreTextureFormat.Half, TexTransCoreTextureChannel.RGBA);

                case TextureFormat.RFloat: return (TexTransCoreTextureFormat.Float, TexTransCoreTextureChannel.R);
                case TextureFormat.RGFloat: return (TexTransCoreTextureFormat.Float, TexTransCoreTextureChannel.RG);
                case TextureFormat.RGBAFloat: return (TexTransCoreTextureFormat.Float, TexTransCoreTextureChannel.RGBA);
            }
        }
        internal static GraphicsFormat ToUnityGraphicsFormat(this TexTransCoreTextureFormat format, TexTransCoreTextureChannel channel = TexTransCoreTextureChannel.RGBA)
        {
            switch (format, channel)
            {
                default: throw new ArgumentOutOfRangeException(format.ToString() + "-" + channel.ToString());
                case (TexTransCoreTextureFormat.Byte, TexTransCoreTextureChannel.R): return GraphicsFormat.R8_UNorm;
                case (TexTransCoreTextureFormat.Byte, TexTransCoreTextureChannel.RG): return GraphicsFormat.R8G8_UNorm;
                case (TexTransCoreTextureFormat.Byte, TexTransCoreTextureChannel.RGBA): return GraphicsFormat.R8G8B8A8_UNorm;

                case (TexTransCoreTextureFormat.UShort, TexTransCoreTextureChannel.R): return GraphicsFormat.R16_UNorm;
                case (TexTransCoreTextureFormat.UShort, TexTransCoreTextureChannel.RG): return GraphicsFormat.R16G16_UNorm;
                case (TexTransCoreTextureFormat.UShort, TexTransCoreTextureChannel.RGBA): return GraphicsFormat.R16G16B16A16_UNorm;

                case (TexTransCoreTextureFormat.Half, TexTransCoreTextureChannel.R): return GraphicsFormat.R16_SFloat;
                case (TexTransCoreTextureFormat.Half, TexTransCoreTextureChannel.RG): return GraphicsFormat.R16G16_SFloat;
                case (TexTransCoreTextureFormat.Half, TexTransCoreTextureChannel.RGBA): return GraphicsFormat.R16G16B16A16_SFloat;

                case (TexTransCoreTextureFormat.Float, TexTransCoreTextureChannel.R): return GraphicsFormat.R32_SFloat;
                case (TexTransCoreTextureFormat.Float, TexTransCoreTextureChannel.RG): return GraphicsFormat.R32G32_SFloat;
                case (TexTransCoreTextureFormat.Float, TexTransCoreTextureChannel.RGBA): return GraphicsFormat.R32G32B32A32_SFloat;
            }
        }
        internal static (TexTransCoreTextureFormat format, TexTransCoreTextureChannel channel) ToTTCTextureFormat(this GraphicsFormat format)
        {
            switch (format)
            {
                default: throw new ArgumentOutOfRangeException(format.ToString());
                case GraphicsFormat.R8_UNorm: return (TexTransCoreTextureFormat.Byte, TexTransCoreTextureChannel.R);
                case GraphicsFormat.R8G8_UNorm: return (TexTransCoreTextureFormat.Byte, TexTransCoreTextureChannel.RG);
                case GraphicsFormat.R8G8B8A8_UNorm: return (TexTransCoreTextureFormat.Byte, TexTransCoreTextureChannel.RGBA);

                case GraphicsFormat.R16_UNorm: return (TexTransCoreTextureFormat.UShort, TexTransCoreTextureChannel.R);
                case GraphicsFormat.R16G16_UNorm: return (TexTransCoreTextureFormat.UShort, TexTransCoreTextureChannel.RG);
                case GraphicsFormat.R16G16B16A16_UNorm: return (TexTransCoreTextureFormat.UShort, TexTransCoreTextureChannel.RGBA);

                case GraphicsFormat.R16_SFloat: return (TexTransCoreTextureFormat.Half, TexTransCoreTextureChannel.R);
                case GraphicsFormat.R16G16_SFloat: return (TexTransCoreTextureFormat.Half, TexTransCoreTextureChannel.RG);
                case GraphicsFormat.R16G16B16A16_SFloat: return (TexTransCoreTextureFormat.Half, TexTransCoreTextureChannel.RGBA);

                case GraphicsFormat.R32_SFloat: return (TexTransCoreTextureFormat.Float, TexTransCoreTextureChannel.R);
                case GraphicsFormat.R32G32_SFloat: return (TexTransCoreTextureFormat.Float, TexTransCoreTextureChannel.RG);
                case GraphicsFormat.R32G32B32A32_SFloat: return (TexTransCoreTextureFormat.Float, TexTransCoreTextureChannel.RGBA);
            }
        }



        public static void DownloadFromRenderTexture<T>(this RenderTexture rt, Span<T> dataSpan) where T : unmanaged
        {
            var (format, channel) = rt.graphicsFormat.ToTTCTextureFormat();
            if (EnginUtil.GetPixelParByte(format, channel) * rt.width * rt.height != dataSpan.Length) { throw new ArgumentException(); }

            var request = AsyncGPUReadback.Request(rt, 0);
            request.WaitForCompletion();
            request.GetData<T>().AsSpan().CopyTo(dataSpan);
        }

    }
}
