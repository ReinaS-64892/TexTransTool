#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Rs64.TexTransTool
{
    public abstract class AbstractTexTransGroup : TextureTransformer
    {
        public abstract IEnumerable<TextureTransformer> Targets { get; }

        [SerializeField] bool _IsApply;
        public override bool IsApply => _IsApply;
        [SerializeField] bool _IsSelfCallApply;
        public virtual bool IsSelfCallApply => _IsSelfCallApply;

        public override bool IsPossibleApply => PossibleApplyCheck();

        public override bool IsPossibleCompile => PossibleCompileCheck();

        public override void Apply(AvatarDomain AvatarMaterialDomain = null)
        {
            if (!IsPossibleApply) return;
            if (_IsApply) return;
            _IsApply = true;
            foreach (var tf in Targets)
            {
                if (tf == null) continue;
                //Debug.Log(tf.gameObject.name);
                if (tf.ThisEnable == false) continue;
                tf.Apply(AvatarMaterialDomain);
            }
        }
        public void SelfCallApply()
        {
            if (_IsSelfCallApply == true) return;
            _IsSelfCallApply = true;
            Apply();
        }
        public void SelfCallRevart()
        {
            if (_IsSelfCallApply == false) return;
            _IsSelfCallApply = false;
            Revart();
        }
        public override void Revart(AvatarDomain AvatarMaterialDomain = null)
        {
            if (!_IsApply) return;
            _IsApply = false;

            foreach (var tf in Targets.Reverse())
            {
                if (tf == null) continue;
                if (tf.ThisEnable == false) continue;
                tf.Revart(AvatarMaterialDomain);
            }
        }
        public override void Compile()
        {
            try
            {
                foreach (var tf in Targets)
                {
                    if (tf == null) continue;
                    if (tf.ThisEnable == false) continue;
                    tf.Compile();
                    tf.Apply();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                foreach (var tf in Targets.Reverse())
                {
                    if (tf == null) continue;
                    if (tf.ThisEnable == false) continue;
                    tf.Revart();
                }
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
        bool PossibleCompileCheck()
        {
            bool PossibleFlag = true;
            foreach (var tf in Targets)
            {
                if (tf == null) continue;
                if (tf.ThisEnable == false) continue;
                if (!tf.IsPossibleCompile)
                {
                    PossibleFlag = false;
                }
            }
            return PossibleFlag;
        }
    }
}
#endif