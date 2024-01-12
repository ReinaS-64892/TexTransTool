#if UNITY_EDITOR
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransTool.Build.AvatarBuildUtils;

namespace net.rs64.TexTransTool.ReferenceResolver
{
    internal abstract class AbstractResolver : MonoBehaviour, ITexTransToolTag
    {
        [HideInInspector, SerializeField] int _saveDataVersion = TexTransBehavior.TTTDataVersion;
        public int SaveDataVersion => _saveDataVersion;

        public abstract void Resolving(ResolverContext avatar);

    }
}
#endif