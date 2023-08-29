#if UNITY_EDITOR
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public abstract class TextureTransformer : MonoBehaviour, ITexTransToolTag
    {
        public virtual bool ThisEnable => gameObject.activeSelf && enabled;
        public abstract bool IsApply { get; }
        public abstract bool IsPossibleApply { get; }
        [SerializeField] bool _IsSelfCallApply;
        public virtual bool IsSelfCallApply { get => _IsSelfCallApply; protected set => _IsSelfCallApply = value; }
        [HideInInspector,SerializeField] int _saveDataVersion = Utils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;
        public abstract void Apply(AvatarDomain avatarMaterialDomain = null);
        public abstract void Revert(AvatarDomain avatarMaterialDomain = null);
        public virtual void SelfCallApply(AvatarDomain avatarMaterialDomain = null)
        {
            IsSelfCallApply = true;
            Apply(avatarMaterialDomain);
        }
    }
}
#endif
