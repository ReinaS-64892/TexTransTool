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
        (int x, int y, int z) WorkGroupSize { get; }
        void Dispatch(int x, int y, int z);

        /// <summary>
        /// この ID は Binding Index である可能性もあるし、独自のハッシュ値のようなものである可能性がある。
        /// </summary>
        int NameToID(string name);
        void UploadCBuffer<T>(int id, ReadOnlySpan<T> bytes) where T : unmanaged;
        void SetTexture(int id, ITTRenderTexture tex);
    }
}
