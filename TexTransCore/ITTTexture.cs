#nullable enable
namespace net.rs64.TexTransCore
{

    public interface ITTTexture : ITTObject
    {
        int Width { get; }
        int Hight { get; }

        bool MipMap { get; }
    }


}
