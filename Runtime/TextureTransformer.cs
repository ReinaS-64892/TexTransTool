#if UNITY_EDITOR
using UnityEngine;
namespace Rs64.TexTransTool
{
    public abstract class TextureTransformer : MonoBehaviour
    {
        public virtual bool ThisEnable => gameObject.activeSelf && enabled;
        public abstract bool IsApply { get; }
        public abstract bool IsPossibleApply { get; }
        public abstract bool IsPossibleCompile { get; }
        public abstract void Compile();
        public abstract void Apply(AvatarDomain avatarMaterialDomain = null);
        public abstract void Revart(AvatarDomain avatarMaterialDomain = null);
    }
}
#endif