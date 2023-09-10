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
        public AbstractTexTransGroup TexTransGroup => GetComponent<AbstractTexTransGroup>();
        [SerializeField] protected AvatarDomain lastDomain;

        [SerializeField] bool _IsSelfCallApply;
        public virtual bool IsSelfCallApply => _IsSelfCallApply;
        [HideInInspector, SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;
        public virtual AvatarDomain GenerateDomain(GameObject avatar, UnityEngine.Object OverrideAssetContainer = null)
        {
            return new AvatarDomain(avatar, true, OverrideAssetContainer);
        }
        public virtual void Apply(GameObject avatar = null, UnityEngine.Object OverrideAssetContainer = null)
        {
            var texTransGroup = TexTransGroup;
            if (texTransGroup == null)
            {
                Debug.LogWarning("AvatarDomainDefinition : texTransGroupが存在しません。通常ではありえないエラーです。");
                return;
            }
            if (!texTransGroup.IsPossibleApply)
            {
                Debug.LogWarning("AvatarDomainDefinition : このグループ内のどれかがプレビューできる状態ではないため実行できません。");
                return;
            }
            if (texTransGroup.IsApply)
            {
                Debug.LogWarning("AvatarDomainDefinition : すでにこのコンポーネントでプレビュー状態のため実行できません。");
                return;
            }
            if (TexTransGroupValidationUtils.SelfCallApplyExists(texTransGroup.Targets))
            {
                Debug.LogWarning("AvatarDomainDefinition : すでにプレビュー状態のものが存在しているためこのグループのプレビューはできません。すでにプレビューされている物を解除してください。");
                return;
            }
            if (avatar == null && Avatar == null) return;
            if (avatar == null) avatar = Avatar;
            TexTransGroupValidationUtils.ValidateTexTransGroup(texTransGroup);
            var Domain = GenerateDomain(avatar, OverrideAssetContainer);

            _IsSelfCallApply = true;

            texTransGroup.Apply(Domain);

            Domain.EditFinish();
            lastDomain = Domain;
        }

        public virtual void Revert()
        {
            if (_IsSelfCallApply == false) return;
            var texTransGroup = TexTransGroup;
            if (texTransGroup == null) return;
            if (!texTransGroup.IsApply) return;
            _IsSelfCallApply = false;

            texTransGroup.Revert();

            lastDomain.Dispose();
            lastDomain = null;
        }
    }
}
#endif
