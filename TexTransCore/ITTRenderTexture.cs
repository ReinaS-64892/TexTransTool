#nullable enable
namespace net.rs64.TexTransCore
{
    /// <summary>
    ///  CPU からは操作不能 だが Engin からは操作でき、最終的には ReadBack を行い RamTexture に変換してから圧縮を行い、 DiskTexture になる。
    /// </summary>
    public interface ITTRenderTexture : ITTTexture
    {
        TexTransCoreTextureChannel ContainsChannel { get; }
    }

    public static class TTRenderTextureUtility
    {
        public static bool EqualSize(this ITTTexture l, ITTTexture r)
        {
            if (l.Width != r.Width) return false;
            if (l.Hight != r.Hight) return false;
            return true;
        }
    }
}
