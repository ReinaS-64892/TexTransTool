#if UNITY_EDITOR
using UnityEngine;
#if VRC_BASE
using VRC.SDKBase;
#endif
namespace Rs64.TexTransTool
{
    public abstract class TextureTransformer : MonoBehaviour , ITexTransToolTag
#if VRC_BASE
    , IEditorOnly
#endif
    {
        public virtual bool ThisEnable => gameObject.activeSelf && enabled;
        public abstract bool IsApply { get; }
        public abstract bool IsPossibleApply { get; }
        // public abstract bool IsPossibleCompile { get; }
        [SerializeField] bool _IsSelfCallApply;
        public virtual bool IsSelfCallApply { get => _IsSelfCallApply; protected set => _IsSelfCallApply = value; }
        // public virtual void Compile() { }
        public abstract void Apply(AvatarDomain avatarMaterialDomain = null);
        public abstract void Revart(AvatarDomain avatarMaterialDomain = null);
        public virtual void SelfCallApply(AvatarDomain avatarMaterialDomain = null)
        {
            IsSelfCallApply = true;
            Apply(avatarMaterialDomain);
        }
    }
}
#endif