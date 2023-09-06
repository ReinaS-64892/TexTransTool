#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using System;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Build
{
    [AddComponentMenu("TexTransTool/AvatarDomainDefinition")]
    [RequireComponent(typeof(AbstractTexTransGroup))]
    public class AvatarDomainDefinition : MonoBehaviour, ITexTransToolTag
    {
        public GameObject Avatar;
        public bool GenerateCustomMipMap;
        public AbstractTexTransGroup TexTransGroup => GetComponent<AbstractTexTransGroup>();
        [SerializeField] protected AvatarDomain CacheDomain;

        [SerializeField] bool _IsSelfCallApply;
        public virtual bool IsSelfCallApply => _IsSelfCallApply;
        [HideInInspector, SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;
        public virtual AvatarDomain GetDomain(GameObject avatar, UnityEngine.Object OverrideAssetContainer = null)
        {
            return new AvatarDomain(avatar, true, GenerateCustomMipMap, OverrideAssetContainer);
        }
        public virtual void Apply(GameObject avatar = null, UnityEngine.Object OverrideAssetContainer = null)
        {
            var texTransGroup = TexTransGroup;
            if (texTransGroup == null) return;
            if (texTransGroup.IsApply) return;
            if (avatar == null && Avatar == null) return;
            if (avatar == null) avatar = Avatar;
            TexTransGroupValidationUtils.ValidateTexTransGroup(texTransGroup);
            CacheDomain = GetDomain(avatar, OverrideAssetContainer);
            _IsSelfCallApply = true;
            texTransGroup.Apply(CacheDomain);
            CacheDomain.SaveTexture();
        }

        public virtual void Revert()
        {
            if (_IsSelfCallApply == false) return;
            var texTransGroup = TexTransGroup;
            if (texTransGroup == null) return;
            if (!texTransGroup.IsApply) return;
            _IsSelfCallApply = false;
            CacheDomain.ResetMaterial();
            texTransGroup.Revert(CacheDomain);
            AssetSaveHelper.DeleteAsset(CacheDomain.Asset);
            CacheDomain = null;
        }
    }
}
#endif
