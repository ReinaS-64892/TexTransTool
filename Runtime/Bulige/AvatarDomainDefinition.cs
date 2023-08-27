#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using System;

namespace net.rs64.TexTransTool.Bulige
{
    [RequireComponent(typeof(AbstractTexTransGroup))]
    public class AvatarDomainDefinition : MonoBehaviour, ITexTransToolTag
    {
        public GameObject Avatar;
        public bool GenereatCustomMipMap;
        [SerializeField] public AbstractTexTransGroup TexTransGroup;
        [SerializeField] protected AvatarDomain CacheDomain;

        [SerializeField] bool _IsSelfCallApply;
        public virtual bool IsSelfCallApply => _IsSelfCallApply;
        [HideInInspector, SerializeField] int _saveDataVersion = Utils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;
        public virtual AvatarDomain GetDomain(UnityEngine.Object OverrideAssetContainer = null)
        {
            return new AvatarDomain(Avatar, true, GenereatCustomMipMap, OverrideAssetContainer);
        }
        public virtual void SetAvatar(GameObject gameObject)
        {
            Avatar = gameObject;
        }
        protected void Reset()
        {
            TexTransGroup = GetComponent<AbstractTexTransGroup>();
        }
        public virtual void Apply(UnityEngine.Object OverrideAssetContainer = null)
        {
            if (TexTransGroup == null) Reset();
            if (TexTransGroup.IsApply) return;
            if (Avatar == null) return;
            TTGValidationUtili.ValidatTTG(TexTransGroup);
            CacheDomain = GetDomain(OverrideAssetContainer);
            _IsSelfCallApply = true;
            TexTransGroup.Apply(CacheDomain);
            CacheDomain.SaveTexture();
        }

        public virtual void Revart()
        {
            if (_IsSelfCallApply == false) return;
            if (TexTransGroup == null) Reset();
            if (!TexTransGroup.IsApply) return;
            _IsSelfCallApply = false;
            CacheDomain.ResetMaterial();
            TexTransGroup.Revart(CacheDomain);
            AssetSaveHelper.DeletAsset(CacheDomain.Asset);
            CacheDomain = null;
        }
    }
}
#endif