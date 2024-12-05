#nullable enable
using System;

namespace net.rs64.TexTransCore
{
    /// <summary>
    ///  ComputeShader を実行できるハンドラー、キーを基にエンジンから与えられる。
    ///  使用し終えたら解放すること
    /// </summary>
    public interface ITTComputeHandler : IDisposable
    {
        /// <summary>
        /// 環境によってサイズが増減する可能性があるので必ずこれを見るように
        /// </summary>
        (uint x, uint y, uint z) WorkGroupSize { get; }
        void Dispatch(uint x, uint y, uint z);

        /// <summary>
        /// この ID は Binding Index である可能性もあるし、独自のハッシュ値のようなものである可能性がある。
        /// </summary>
        int NameToID(string name);

        void UploadConstantsBuffer<T>(int id, ReadOnlySpan<T> bytes) where T : unmanaged;

        void SetTexture(int id, ITTRenderTexture tex);
        void SetStorageBuffer(int id, ITTStorageBuffer bufferHolder);
    }


    public static class ComputeHandlerUtility
    {
        public static void DispatchWithTextureSize(this ITTComputeHandler computeHandler, ITTRenderTexture texture)
        {
            var (x, y, _) = computeHandler.WorkGroupSize;
            computeHandler.Dispatch((uint)((texture.Width + (x - 1)) / x), (uint)((texture.Hight + (y - 1)) / y), 1);
        }
        public static void DispatchWithSize(this ITTComputeHandler computeHandler, (int x, int y) size)
        {
            var (x, y, _) = computeHandler.WorkGroupSize;
            computeHandler.Dispatch((uint)((size.x + (x - 1)) / x), (uint)((size.y + (y - 1)) / y), 1);
        }

        public static ITTStorageBuffer SetStorageBufferFromUpload<TTCE, T>(this TTCE engine, ITTComputeHandler computeHandler, int id, Span<T> data, bool downloadable = false)
        where TTCE : ITexTransDriveStorageBufferHolder
        where T : unmanaged
        {
            var storageBuffer = engine.UploadStorageBuffer(data, downloadable);
            computeHandler.SetStorageBuffer(id, storageBuffer);
            return storageBuffer;
        }
        public static ITTStorageBuffer SetStorageBufferFromAllocate<TTCE>(this TTCE engine, ITTComputeHandler computeHandler, int id, int dataLength, bool downloadable = false)
        where TTCE : ITexTransDriveStorageBufferHolder
        {
            var storageBuffer = engine.AllocateStorageBuffer(dataLength, downloadable);
            computeHandler.SetStorageBuffer(id, storageBuffer);
            return storageBuffer;
        }
    }
}
