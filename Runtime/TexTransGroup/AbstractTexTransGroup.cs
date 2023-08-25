#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace Rs64.TexTransTool
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
        public override void Revart(AvatarDomain AvatarMaterialDomain = null)
        {
            if (!_IsApply) return;
            _IsApply = false;
            IsSelfCallApply = false;

            foreach (var tf in Targets.Reverse())
            {
                if (tf == null) continue;
                if (tf.ThisEnable == false) continue;
                tf.Revart(AvatarMaterialDomain);
                EditorUtility.SetDirty(tf);
            }
        }
        // public override void Compile()
        // {
        //     try
        //     {
        //         foreach (var tf in Targets)
        //         {
        //             if (tf == null) continue;
        //             if (tf.ThisEnable == false) continue;


        //             tf.Compile();
        //             tf.Apply();

        //             UnityEditor.EditorUtility.SetDirty(tf);
        //         }
        //     }
        //     catch (System.Exception e)
        //     {
        //         Debug.LogError(e);
        //     }
        //     finally
        //     {
        //         foreach (var tf in Targets.Reverse())
        //         {
        //             if (tf == null) continue;
        //             if (tf.ThisEnable == false) continue;


        //             tf.Revart();
        //             UnityEditor.EditorUtility.SetDirty(tf);
        //         }
        //     }
        // }



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
        // bool PossibleCompileCheck()
        // {
        //     bool PossibleFlag = true;
        //     foreach (var tf in Targets)
        //     {
        //         if (tf == null) continue;
        //         if (tf.ThisEnable == false) continue;
        //         if (!tf.IsPossibleCompile)
        //         {
        //             PossibleFlag = false;
        //         }
        //     }
        //     return PossibleFlag;
        // }
    }
}
#endif