using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject
{
    public interface IAtlasMaterialPostProses { void Proses(Material material); }
    public class TextureReferenceCopy : IAtlasMaterialPostProses
    {
        public string SousePropertyName;
        public string TargetPropertyName;
        public void Proses(Material material)
        {
            if (!material.HasProperty(SousePropertyName) || !material.HasProperty(TargetPropertyName)) { return; }
            material.SetTexture(TargetPropertyName, material.GetTexture(SousePropertyName));
        }
    }
}
