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
        void UploadStorageBuffer<T>(int id, ReadOnlySpan<T> bytes) where T : unmanaged;
        void SetTexture(int id, ITTRenderTexture tex);
    }


    public static class ComputeHandlerUtility
    {
        public static void DispatchWithTextureSize(this ITTComputeHandler computeHandler, ITTRenderTexture texture)
        {
            var (x, y, _) = computeHandler.WorkGroupSize;
            computeHandler.Dispatch((uint)Math.Max(texture.Width / x, 1), (uint)Math.Max(texture.Hight / y, 1), 1);
        }
    }
}
