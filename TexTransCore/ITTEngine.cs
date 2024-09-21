#nullable enable
using System;

namespace net.rs64.TexTransCore
{
    /*
    この... TexTransTool の Core となる TexTransCore は 公開されていても、内部 API です！
    */

    public interface ITTEngine
    {
        /// <summary>
        /// フォーマットなどはエンジン側が決める話ですが、 RGBA の 4チャンネルあることを前提とします。
        /// 内容は必ず すべてのチャンネルが 0 で初期化されている。
        /// </summary>
        ITTRenderTexture CreateRenderTexture(int width, int height, bool mipMap = false, bool depthAndStencil = false);

        /// <summary>
        /// DiskTexture からロードして writeTarget に向けてソースデータを書き込む。
        /// diskTexture のサイズと同一でないといけない。
        /// </summary>
        void LoadTexture(ITTDiskTexture diskTexture, ITTRenderTexture writeTarget);

        // /// <summary>
        // /// それが同じテクスチャーの改変版などだった場合に 解像度やMipMapなどの設定以外を継承するためにある。
        // /// </summary>
        // void InheritTextureSettings(ITTTexture source, ITTTexture target);

        /// <summary>
        /// RenderTexture をコピーする。ただし、リサイズなどは行われず、絶対に同じサイズ出ないといけない。
        /// MipMap は使用有無が同じ場合にのみコピーされる。
        /// </summary>
        void CopyRenderTexture(ITTRenderTexture source, ITTRenderTexture target);

        /// <summary>
        /// その RenderTexture を Clear する、特定の色で。 Depth や Stencil などもクリアされます。
        /// </summary>
        /// <param name="renderTexture"></param>
        /// <param name="fillColor"></param>
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



        /// <summary>
        /// ダウンスケールを行う。同じ解像度ではダメ、あと大きいスケールにもできない。
        /// Keyがない場合は Engin が適当に決めてよい
        /// MipMapの再生成は行われない。
        /// </summary>
        void DownScale(ITTRenderTexture source, ITTRenderTexture target, ITTDownScalingKey? downScalingKey = null);
        /// <summary>
        /// アップスケールを行う。同じ解像度ではダメ、あと小さいスケールにもできない。
        /// Keyがない場合は Engin が適当に決めてよい
        /// MipMapの再生成は行われない。
        /// </summary>
        void UpScale(ITTRenderTexture source, ITTRenderTexture target, ITTUpScalingKey? upScalingKey = null);

        /// <summary>
        /// MipMapが存在しないといけない。
        /// Keyがない場合は Engin が適当に決めてよい
        /// </summary>
        void GenerateMipMap(ITTRenderTexture renderTexture, ITTDownScalingKey? downScalingKey = null);


        /// <summary>
        /// キーオブジェクトを基に dist を下のレイヤー add を上のレイヤーとして色合成する。
        /// 最終結果は dist に書き込まれる。
        /// サイズが必ず同一である必要がある。
        /// </summary>
        void TextureBlend(ITTRenderTexture dist, ITTRenderTexture add, ITTBlendKey blendKey);

        // /// <summary>
        // /// ITTTransData を基に変形する。
        // /// transSource は MipMap を保有していないといけないし、 writeTarget は DepthAndStencil を保有している必要がある。
        // /// writeTarget の MipMap の有無はどちらでもよいが、 MipMap の再生成は行われない。
        // /// </summary>
        // void TransTexture(ITTTexture transSource, ITTRenderTexture writeTarget, ITTTransData transData)

    }

    public static class EnginUtil
    {
        public static ITTRenderTexture LoadTextureWidthFullScale(this ITTEngine engine, ITTDiskTexture diskTexture)
        {
            var fullSizeRt = engine.CreateRenderTexture(diskTexture.Width, diskTexture.Hight, diskTexture.MipMap, false);
            engine.LoadTexture(diskTexture, fullSizeRt);
            return fullSizeRt;
        }
        public static void LoadTextureWidthAnySize(this ITTEngine engine, ITTDiskTexture diskTexture, ITTRenderTexture renderTexture, ITTDownScalingKey? downScalingKey, ITTUpScalingKey? upScalingKey)
        {
            using (var sourceSize = LoadTextureWidthFullScale(engine, diskTexture))
                engine.CopyRenderTextureMaybeReScale(sourceSize, renderTexture, downScalingKey, upScalingKey);
        }

        public static void CopyRenderTextureMaybeReScale(this ITTEngine engine, ITTRenderTexture source, ITTRenderTexture target, ITTDownScalingKey? downScalingKey, ITTUpScalingKey? upScalingKey)
        {
            if (source.Width == target.Width && source.Hight == target.Hight) { engine.CopyRenderTexture(source, target); return; }
            if (source.Width > target.Width && source.Hight > target.Hight) { engine.DownScale(source, target, downScalingKey); return; }
            if (source.Width < target.Width && source.Hight < target.Hight) { engine.UpScale(source, target, upScalingKey); return; }

            throw new NotImplementedException();
        }
    }
}