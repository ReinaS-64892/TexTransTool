using UnityEngine;

namespace net.rs64.TexTransTool.ReferenceResolver
{
    internal abstract class AbstractResolver : MonoBehaviour, ITexTransToolTag
    {
        [HideInInspector, SerializeField] int _saveDataVersion = TexTransBehavior.TTTDataVersion;
        public int SaveDataVersion => _saveDataVersion;

        public abstract void Resolving(ResolverContext avatar);

        internal const string FoldoutName = "Resolver";
    }
}
