using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    internal class AtlasExcludeTag : MonoBehaviour, ITexTransToolTag
    {
        [HideInInspector, SerializeField] int _saveDataVersion = TexTransBehavior.TTTDataVersion;
        public int SaveDataVersion => _saveDataVersion;
    }
}