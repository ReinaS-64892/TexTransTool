#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

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
            if (!IsPossibleApply) return;
            if (_IsApply) return;
            _IsApply = true;
            foreach (var tf in Targets)
            {
                if (tf == null) continue;
                if (tf.ThisEnable == false) continue;
                tf.Apply(AvatarMaterialDomain);
                EditorUtility.SetDirty(tf);
            }
        }
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
