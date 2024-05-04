using UnityEngine;
using UnityEngine.Serialization;

namespace net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject
{
    public interface IAtlasMaterialPostProses { void Proses(Material material); }
    public class TextureReferenceCopy : IAtlasMaterialPostProses
    {
        [FormerlySerializedAs("SousePropertyName")] public string SourcePropertyName;
        public string TargetPropertyName;
        public void Proses(Material material)
        {
            if (!material.HasProperty(SourcePropertyName) || !material.HasProperty(TargetPropertyName)) { return; }
            material.SetTexture(TargetPropertyName, material.GetTexture(SourcePropertyName));
        }
    }
}
