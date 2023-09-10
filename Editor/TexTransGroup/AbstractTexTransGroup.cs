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
        public override bool IsApply { get => _IsApply; set => _IsApply = value; }
        public override List<Renderer> GetRenderers => TextureTransformerFilter(Targets).SelectMany(I => I.GetRenderers).ToList();

        public override bool IsPossibleApply => PossibleApplyCheck();

        public override void Apply(IDomain Domain = null)
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

            foreach (var tf in TextureTransformerFilter(Targets))
            {
                tf.Apply(Domain);
                EditorUtility.SetDirty(tf);
            }

        }
        public void Revert()
        {
            _IsApply = false;
            foreach (var tf in TextureTransformerFilter(Targets))
            {
                tf.IsApply = false;
                EditorUtility.SetDirty(tf);
            }
        }
        public override void PreviewRevert()
        {
            Revert();
            base.PreviewRevert();
        }
        public static IEnumerable<TextureTransformer> TextureTransformerFilter(IEnumerable<TextureTransformer> Targets) => Targets.Where(tf => tf != null && tf.ThisEnable);

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
