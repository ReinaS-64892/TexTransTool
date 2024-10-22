#nullable enable
using System;

namespace net.rs64.TexTransCore
{
    /*
    この... TexTransTool の Core となる TexTransCore は 公開されていても、内部 API です！
    */

    public interface ITexTransCoreEngine
    : ITexTransGetTexture
    , ITexTransLoadTexture
    , ITexTransRenderTextureOperator
    , ITexTransRenderTextureReScaler
    , ITexTranBlending
    {

    }

    public interface ITexTransGetTexture
    {
        /// <summary>
        /// フォーマットなどはエンジン側が決める話です。
        /// 内容は必ず すべてのチャンネルが 0 で初期化されている。アルファも 0 。
        /// 基本的に RGBA の 4チャンネルで Gamma がデフォだけど、エンジン側がいい感じにすることを前提としてリニアにもできるようにしたいね！
        /// </summary>
        ITTRenderTexture CreateRenderTexture(int width, int height, bool mipMap = false, bool depthAndStencil = false);

        /// <summary>
        /// TransTexture などで深度情報として扱うようなものだが、今のところ未実装だ。
        /// フォーマットなどの指定はエンジン側が決める話。
        /// 基本的にはリニア確定で一チャンネルのデータを想定してる。
        /// </summary>
        // ITTDepthRenderTexture CreateDepthRenderTexture(int width, int height);

    }
    public interface ITexTransLoadTexture
    {
        /// <summary>
        /// DiskTexture からロードして writeTarget に向けてソースデータを書き込む。
        /// diskTexture のサイズと同一でないといけない。
        /// </summary>
        void LoadTexture(ITTDiskTexture diskTexture, ITTRenderTexture writeTarget);

        // /// <summary>
        // /// それが同じテクスチャーの改変版などだった場合に 解像度やMipMapなどの設定以外を継承するためにある。
        // /// </summary>
        // void InheritTextureSettings(ITTTexture source, ITTTexture target);
    }

    public interface ITexTransRenderTextureOperator
    {

        /// <summary>
        /// RenderTexture をコピーする。ただし、リサイズなどは行われず、絶対に同じサイズ出ないといけない。
        /// MipMap は使用有無が同じ場合にのみコピーされる。
        /// </summary>
        void CopyRenderTexture(ITTRenderTexture source, ITTRenderTexture target);

        /// <summary>
        /// その RenderTexture を Clear する、特定の色で。 Depth や Stencil などもクリアされます。
        /// カラーはガンマ色空間を想定
        /// </summary>
        void ClearRenderTexture(ITTRenderTexture renderTexture, Color fillColor);

        void FillAlpha(ITTRenderTexture renderTexture, float alpha);
        void MulAlpha(ITTRenderTexture renderTexture, float value);

        /// <summary>
        /// 同じ大きさでないといけない。
        /// </summary>
        void CopyAlpha(ITTRenderTexture source, ITTRenderTexture target);

        /// <summary>
        /// dist に対して add をアルファだけ乗算する。
        /// 同じ大きさでないといけない。
        /// </summary>
        void MulAlpha(ITTRenderTexture dist, ITTRenderTexture add);
    }

    public interface ITexTransRenderTextureReScaler
    {
        /// <summary>
        /// ダウンスケールを行う。同じ解像度ではダメ、あと大きいスケールにもできない。
        /// Keyがない場合は Engin が適当に決めてよい
        /// MipMapの再生成は行われない。
        /// </summary>
        void DownScale(ITTRenderTexture source, ITTRenderTexture target, ITTDownScalingKey? downScalingKey);
        /// <summary>
        /// アップスケールを行う。同じ解像度ではダメ、あと小さいスケールにもできない。
        /// Keyがない場合は Engin が適当に決めてよい
        /// MipMapの再生成は行われない。
        /// </summary>
        void UpScale(ITTRenderTexture source, ITTRenderTexture target, ITTUpScalingKey? upScalingKey);

        /// <summary>
        /// MipMapが存在しないといけない。
        /// Keyがない場合は Engin が適当に決めてよい
        /// </summary>
        void GenerateMipMap(ITTRenderTexture renderTexture, ITTDownScalingKey? downScalingKey);
    }
    public interface ITexTranBlending
    {

        /// <summary>
        /// キーオブジェクトを基に dist を下のレイヤー add を上のレイヤーとして色合成する。
        /// 最終結果は dist に書き込まれる。
        /// サイズが必ず同一である必要がある。
        /// </summary>
        void TextureBlend(ITTRenderTexture dist, ITTRenderTexture add, ITTBlendKey blendKey);

        /// <summary>
        /// GrabTexture の内容を基にいい感じに色調調整などを行う。
        /// 内容の調整は、 GrabCompute 側に仕込まれている。
        /// </summary>
        void GrabBlending(ITTRenderTexture grabTexture, ITTGrabBlending grabBlending);
    }
    public interface ITexTransTransTexture
    {
        // /// <summary>
        // /// ITTTransData を基に変形する。
        // /// transSource は MipMap を保有していないといけないし、 writeTarget は DepthAndStencil を保有している必要がある。
        // /// writeTarget の MipMap の有無はどちらでもよいが、 MipMap の再生成は行われない。
        // /// </summary>
        // void TransTexture(ITTTexture transSource, ITTRenderTexture writeTarget, ITTTransData transData)
    }

    public interface ITTRenderTextureColorSpace
    {
        TexTransCoreTextureFormat DefaultRenderTextureFormat { get; }
        /// <summary>
        ///  レンダーテクスチャーの色空間。基本はガンマであってほしいがどちらかであることを正しく実装するべきで、それも変えれるようにあるべきだが、これはこのコンテキストが始まる時点で定義するべきであるな。
        /// </summary>
        bool RenderTextureColorSpaceIsLinear { get; }
    }
    public enum TexTransCoreTextureFormat
    {
        /// <summary>
        /// いわゆる R8G8B8A8 それが UNROM か SRGB かは それが Linear かどうかによる
        /// </summary>
        RGBA_Byte,
        /// <summary>
        /// いわゆる R16G16B16A16_UINT
        /// </summary>
        RGBA_UShort,
        /// <summary>
        /// いわゆる R32G32B32A32_Float
        /// </summary>
        RGBA_Float,
    }
    public static class EnginUtil
    {
        // こういう細かい単位でどの機能が必要かの明示はあってもうれしいから この範囲ではGenericsを使っていこう

        public static ITTRenderTexture LoadTextureWidthFullScale<TTCE>(this TTCE engine, ITTDiskTexture diskTexture)
        where TTCE : ITexTransGetTexture, ITexTransLoadTexture
        {
            var fullSizeRt = engine.CreateRenderTexture(diskTexture.Width, diskTexture.Hight, diskTexture.MipMap, false);
            engine.LoadTexture(diskTexture, fullSizeRt);
            return fullSizeRt;
        }
        public static void LoadTextureWidthAnySize<TTCE>(this TTCE engine, ITTDiskTexture diskTexture, ITTRenderTexture renderTexture, ITTDownScalingKey? downScalingKey = null, ITTUpScalingKey? upScalingKey = null)
        where TTCE : ITexTransGetTexture
        , ITexTransLoadTexture
        , ITexTransRenderTextureReScaler
        , ITexTransRenderTextureOperator
        {
            using (var sourceSize = LoadTextureWidthFullScale(engine, diskTexture))
                engine.CopyRenderTextureMaybeReScale(sourceSize, renderTexture, downScalingKey, upScalingKey);
        }

        public static void CopyRenderTextureMaybeReScale<TTCE>(this TTCE engine, ITTRenderTexture source, ITTRenderTexture target, ITTDownScalingKey? downScalingKey = null, ITTUpScalingKey? upScalingKey = null)
        where TTCE : ITexTransGetTexture
        , ITexTransRenderTextureReScaler
        , ITexTransRenderTextureOperator
        {
            if (source.Width == target.Width && source.Hight == target.Hight) { engine.CopyRenderTexture(source, target); return; }
            if (source.Width > target.Width && source.Hight > target.Hight) { engine.DownScale(source, target, downScalingKey); return; }
            if (source.Width < target.Width && source.Hight < target.Hight) { engine.UpScale(source, target, upScalingKey); return; }

            throw new NotImplementedException();
        }
    }
}
