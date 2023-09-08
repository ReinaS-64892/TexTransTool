#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;

namespace net.rs64.TexTransTool
{
    public abstract class AbstractTexTransGroup : TextureTransformer
    {
        public abstract IEnumerable<TextureTransformer> Targets { get; }

        [SerializeField] bool _IsApply;
        public override bool IsApply => _IsApply;

        public override bool IsPossibleApply => PossibleApplyCheck();

        public override void Apply(AvatarDomain AvatarMaterialDomain = null)
        {
            if (!IsPossibleApply)
            {
                Debug.LogWarning("TexTransGroup : このグループ内のどれかがプレビューできる状態ではないため実行できません。");
                return;
            }
            if (_IsApply)
            {
                Debug.LogWarning("TexTransGroup : すでにこのコンポーネントでプレビュー状態のため実行できません。");
                return;
            }
            if (TexTransGroupValidationUtils.SelfCallApplyExists(Targets))
            {
                Debug.LogWarning("TexTransGroup : すでにプレビュー状態のものが存在しているためこのグループのプレビューはできません。すでにプレビューされている物を解除してください。");
                return;
            }
            _IsApply = true;
            try
            {
                foreach (var tf in TextureTransformerFilter(Targets))
                {
                    tf.Apply(AvatarMaterialDomain);
                    EditorUtility.SetDirty(tf);
                }
            }
            catch (Exception ex)
            {
                Revert(AvatarMaterialDomain);
                throw ex;
            }
        }
        public static IEnumerable<TextureTransformer> TextureTransformerFilter(IEnumerable<TextureTransformer> Targets) => Targets.Where(tf => tf != null && tf.ThisEnable);
        public override void Revert(AvatarDomain AvatarMaterialDomain = null)
        {
            if (!_IsApply) return;
            _IsApply = false;
            IsSelfCallApply = false;

            foreach (var tf in Targets.Reverse())
            {
                if (tf == null) continue;
                if (tf.ThisEnable == false) continue;
                tf.Revert(AvatarMaterialDomain);
                EditorUtility.SetDirty(tf);
            }
        }

        bool PossibleApplyCheck()
        {
            bool PossibleFlag = true;
            foreach (var tf in Targets)
            {
                if (tf == null) continue;
                if (tf.ThisEnable == false) continue;
                if (!tf.IsPossibleApply)
                {
                    PossibleFlag = false;
                }
            }
            return PossibleFlag;
        }
    }
}
#endif
