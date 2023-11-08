#if UNITY_EDITOR
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransTool.Build.AvatarBuildUtils;

namespace net.rs64.TexTransTool.ReferenceResolver
{
    public abstract class AbstractResolver : MonoBehaviour, ITexTransToolTag
    {
        [HideInInspector, SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;

        public abstract void Resolving(ResolverContext avatar);

    }
}
#endif