#if UNITY_EDITOR
using net.rs64.TexTransTool;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas
{
    public class AtlasExcludeTag : MonoBehaviour, ITexTransToolTag
    {
        [HideInInspector, SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;
    }
}
#endif