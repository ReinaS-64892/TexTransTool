
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    public interface IAtlasReMappingReceiver
    {
        void ReMappingReceive(ReceiversUVState uvState, Mesh normalizedOriginalMesh, Mesh atlasMesh);
    }
    public class ReceiversUVState
    {
        public bool WriteIsAtlasTexture;// WriteOriginalUV で書き込んである場合 true になる
        public int? AlreadyOriginWriteable;// 元のUVを書き込んだならそのチャンネルをここに書き込むように
    }
}
