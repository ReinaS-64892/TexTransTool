#if UNITY_EDITOR
using UnityEngine;
using System;
using JetBrains.Annotations;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Build
{
    [AddComponentMenu("TexTransTool/AvatarDomainDefinition")]
    [RequireComponent(typeof(AbstractTexTransGroup))]
    public class AvatarDomainDefinition : MonoBehaviour, ITexTransToolTag
    {
        public GameObject Avatar;
        public AbstractTexTransGroup TexTransGroup => GetComponent<AbstractTexTransGroup>();

        [HideInInspector, SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;

        public virtual void Apply([NotNull] IDomain domain)
        {
            if (domain == null) throw new ArgumentNullException(nameof(domain));
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

            TexTransGroupValidationUtils.ValidateTexTransGroup(texTransGroup);

            texTransGroup.Apply(domain);
        }
    }
}
#endif
