#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using System;

namespace Rs64.TexTransTool.Bulige
{
    [RequireComponent(typeof(AbstractTexTransGroup))]
    public class AvatarDomainDefinition : MonoBehaviour
    {
        public GameObject Avatar;
        public bool GenereatCustomMipMap;
        [SerializeField] public AbstractTexTransGroup TexTransGroup;
        [SerializeField] protected AvatarDomain CacheDomain;

        [SerializeField] bool _IsSelfCallApply;
        public virtual bool IsSelfCallApply => _IsSelfCallApply;
        public virtual AvatarDomain GetDomain(UnityEngine.Object OverrideAssetContainer = null)
        {
            return new AvatarDomain(Avatar.GetComponentsInChildren<Renderer>(true).ToList(), true, GenereatCustomMipMap, OverrideAssetContainer);
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