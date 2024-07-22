
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    public interface IAtlasReMappingReceiver
    {
        void ReMappingReceive(Mesh normalizedOriginalMesh, Mesh atlasMesh);
    }
}
