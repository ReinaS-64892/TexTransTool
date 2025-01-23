#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace net.rs64.TexTransCore
{
    /*
    この... TexTransTool の Core となる TexTransCore は 公開されていても、内部 API です！
    */

    public interface ITexTransCoreEngine
    : ITexTransCreateTexture
    , ITexTransLoadTexture
    , ITexTransRenderTextureIO
    , ITexTransCopyRenderTexture
    , ITexTransCloneRenderTexture
    , ITexTransComputeKeyQuery
    , ITexTransGetComputeHandler
    , ITexTransDriveStorageBufferHolder
    , ITexTransRenderTextureUploadToCreate
    { }

    public interface ITexTransCreateTexture
    {
        /// <summary>
        /// フォーマットなどはエンジン側が決める話です。
        /// 内容は必ず すべてのチャンネルが 0 で初期化されている。アルファも 0 。
        /// 基本的に RGBA の 4チャンネルで Gamma がデフォだけど、エンジン側がいい感じにすることを前提としてリニアにもできるようにしたいね！
        /// Depth や MipMap なんてなかった...いいね！
        /// チャンネル数は基本的に RGBA を使用し、Depth用途などの場合に  R だけにするように、 RGBA の場合は適当な形式だが、 R の場合は特別に高めのBit深度の物が割り当てられることがある。
        /// 解像度は 必ず 64 * 64 かそれ以上である必要があり、 実用量が 256 byte で割り切れるような大きさでなければならない。
        /// </summary>
        ITTRenderTexture CreateRenderTexture(int width, int height, TexTransCoreTextureChannel channel = TexTransCoreTextureChannel.RGBA);

    }
    public interface ITexTransLoadTexture
    {
        /// <summary>
        /// DiskTexture からロードして writeTarget に向けてソースデータを書き込む。
        /// diskTexture のサイズと同一でないといけない。
        /// </summary>
        void LoadTexture(ITTRenderTexture writeTarget, ITTDiskTexture diskTexture);

        // /// <summary>
        // /// それが同じテクスチャーの改変版などだった場合に 解像度やMipMapなどの設定以外を継承するためにある。
        // /// </summary>
        // void InheritTextureSettings(ITTTexture source, ITTTexture target);
    }
    public interface ITexTransRenderTextureIO
    {
        // 基本的にパフォーマンスは良くないからうまく使わないといけない
        void UploadTexture<T>(ITTRenderTexture uploadTarget, ReadOnlySpan<T> bytes, TexTransCoreTextureFormat format) where T : unmanaged;
        void DownloadTexture<T>(Span<T> dataDist, TexTransCoreTextureFormat format, ITTRenderTexture renderTexture) where T : unmanaged;
    }
    public interface ITexTransRenderTextureUploadToCreate : ITexTransRenderTextureIO, ITexTransCreateTexture
    {
        /// <summary>
        ///  CloneRenderTexture と同様、まじめにやるなら初期化をバイパスしてアップロードできるようにするよ良きかも。
        /// </summary>
        ITTRenderTexture UploadTexture<T>(int width, int height, TexTransCoreTextureChannel channel, ReadOnlySpan<T> bytes, TexTransCoreTextureFormat format) where T : unmanaged
        {
            var rt = CreateRenderTexture(width, height, channel);
            UploadTexture(rt, bytes, format);
            return rt;
        }
    }
    public interface ITexTransCopyRenderTexture
    {
        /// <summary>
        /// RenderTexture をコピーする。ただし、リサイズなどは行われず、絶対に同じサイズでないといけない。
        /// </summary>
        void CopyRenderTexture(ITTRenderTexture target, ITTRenderTexture source);
    }
    public interface ITexTransCloneRenderTexture : ITexTransCreateTexture, ITexTransCopyRenderTexture
    {
        /// <summary>
        /// source とは別の参照を持つ、内容がコピーされたテクスチャーを生成する。
        /// 実装をまじめにやる気があるならデフォルト実装ではなく、　CreateRenderTexture で行われる初期化をバイパスしてコピーして渡すなどをするとよいかも。
        /// </summary>
        ITTRenderTexture CloneRenderTexture(ITTRenderTexture source)
        {
            var newRt = CreateRenderTexture(source.Width, source.Hight, source.ContainsChannel);
            CopyRenderTexture(newRt, source);
            return newRt;
        }
    }

    public interface ITexTransGetComputeHandler
    {
        ITTComputeHandler GetComputeHandler(ITTComputeKey computeKey);
    }

    public interface ITexTransDriveStorageBufferHolder
    {
        ITTStorageBuffer AllocateStorageBuffer(int length, bool downloadable = false);
        ITTStorageBuffer UploadStorageBuffer<T>(ReadOnlySpan<T> data, bool downloadable = false) where T : unmanaged;
        /// <summary>
        /// 一度ダウンロードしたら二度と使用できない。
        /// 関数内で Dispose() が呼ばれる。
        /// </summary>
        void DownloadBuffer<T>(Span<T> dist, ITTStorageBuffer takeToFrom) where T : unmanaged;
    }

    public interface ITexTransComputeKeyQuery
    {
        ITexTransStandardComputeKey StandardComputeKey { get; }
        TExKeyQ GetExKeyQuery<TExKeyQ>() where TExKeyQ : ITTExtraComputeKeyQuery;
    }

    public interface ITexTransStandardComputeKey
    {
        ITTComputeKey AlphaFill { get; }
        ITTComputeKey AlphaCopy { get; }
        ITTComputeKey AlphaMultiply { get; }
        ITTComputeKey AlphaMultiplyWithTexture { get; }

        ITTComputeKey ColorFill { get; }
        ITTComputeKey ColorMultiply { get; }

        ITTComputeKey GammaToLinear { get; }
        ITTComputeKey LinearToGamma { get; }

        ITTComputeKey Swizzling { get; }

        ITTSamplerKey DefaultSampler { get; }

        ITTComputeKey FillR { get; }
        ITTComputeKey FillRG { get; }
        ITTComputeKey FillROnly { get; }
        ITTComputeKey FillGOnly { get; }
    }
    public interface ITTExtraComputeKeyQuery { }

    public interface ITransTextureComputeKey : ITTExtraComputeKeyQuery
    {
        ITTComputeKey TransMapping { get; }
        ITTComputeKey TransMappingWithDepth { get; }

        ITTComputeKey TransWarpNone { get; }
        ITTComputeKey TransWarpStretch { get; }

        ITTComputeKey DepthRenderer { get; }
        ITTComputeKey CullingDepth { get; }
    }
    public interface IQuayGeneraleComputeKey : ITTExtraComputeKeyQuery
    {
        ITexTransComputeKeyDictionary<string> GenealCompute { get; }
    }
    public interface IBlendingComputeKey : ITTExtraComputeKeyQuery
    {
        ITexTransComputeKeyDictionary<ITTBlendKey> BlendKey { get; }
        ITexTransComputeKeyDictionary<string> GrabBlend { get; }
    }
    public interface ISamplerComputeKey : ITTExtraComputeKeyQuery
    {
        IKeyValueStore<string, ITTSamplerKey> SamplerKey { get; }
        ITexTransComputeKeyDictionary<ITTSamplerKey> ResizingSamplerKey { get; }
        ITexTransComputeKeyDictionary<ITTSamplerKey> TransSamplerKey { get; }
    }
    public interface ITexTransComputeKeyDictionary<TKey> : IKeyValueStore<TKey, ITTComputeKey> { }
    public interface IKeyValueStore<TKey, TValue> { TValue this[TKey key] { get; } }
    public enum TexTransCoreTextureChannel
    {
        R = 1,
        RG = 2,
        // RGB = 3,//3チャンネルは wgpu にて扱えないから禁止
        RGBA = 4,
    }
    public enum TexTransCoreTextureFormat
    {
        //基本的に float とかの類はマイナスが存在する前提

        /// <summary>
        /// 8bit_UNORM
        /// </summary>
        Byte = 0,
        /// <summary>
        /// 16bit_UNORM
        /// </summary>
        UShort = 1,
        /// <summary>
        /// 16bit_Float
        /// </summary>
        Half = 2,
        /// <summary>
        /// 32bit_Float
        /// </summary>
        Float = 3,
    }
    public static class EnginUtil
    {
        //基本的にこれに沿っていないといけない。
        public static int GetPixelParByte(TexTransCoreTextureFormat textureFormat, TexTransCoreTextureChannel channel)
        {
            switch (textureFormat)
            {
                default:
                case TexTransCoreTextureFormat.Byte: { return 1 * (int)channel; }
                case TexTransCoreTextureFormat.UShort:
                case TexTransCoreTextureFormat.Half: { return 2 * (int)channel; }
                case TexTransCoreTextureFormat.Float: { return 4 * (int)channel; }
            }
        }

        // こういう細かい単位でどの機能が必要かの明示はあってもうれしいから この範囲ではGenericsを使っていこう

        public static ITTRenderTexture LoadTextureWidthFullScale<TTCE>(this TTCE engine, ITTDiskTexture diskTexture)
        where TTCE : ITexTransCreateTexture, ITexTransLoadTexture
        {
            var fullSizeRt = engine.CreateRenderTexture(diskTexture.Width, diskTexture.Hight);
            engine.LoadTexture(fullSizeRt, diskTexture);
            return fullSizeRt;
        }
        public static void LoadTextureWidthAnySize<TTCE>(this TTCE engine, ITTRenderTexture renderTexture, ITTDiskTexture diskTexture)
        where TTCE : ITexTransCreateTexture
        , ITexTransLoadTexture
        , ITexTransCopyRenderTexture
        , ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        {
            if (diskTexture.EqualSize(renderTexture))
            {
                engine.LoadTexture(renderTexture, diskTexture);
            }
            else
            {
                using var sourceSizeRt = LoadTextureWidthFullScale(engine, diskTexture);
                engine.DefaultResizing(renderTexture, sourceSizeRt);
            }
        }
    }

    [System.Serializable]
    public class ComputeKeyInterfaceIsNotImplementException : System.Exception
    {
        public ComputeKeyInterfaceIsNotImplementException() { }
        public ComputeKeyInterfaceIsNotImplementException(string message) : base(message) { }
        public ComputeKeyInterfaceIsNotImplementException(string message, System.Exception inner) : base(message, inner) { }
        protected ComputeKeyInterfaceIsNotImplementException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
