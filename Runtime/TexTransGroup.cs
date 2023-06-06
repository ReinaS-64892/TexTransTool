#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
#if VRC_BASE
using VRC.SDKBase;
#endif
namespace Rs64.TexTransTool
{
    public class TexTransGroup : TextureTransformer
#if VRC_BASE
    , IEditorOnly
#endif
    {
        public List<TextureTransformer> TextureTransformers = new List<TextureTransformer>();
        [SerializeField, HideInInspector] bool _IsApply;
        public override bool IsApply => _IsApply;

        public override bool IsPossibleApply => PossibleApplyCheck();

        public override bool IsPossibleCompile => PossibleCompileCheck();

        public override void Apply(MaterialDomain AvatarMaterialDomain = null)
        {
            if (!IsPossibleApply) return;
            if (_IsApply) return;
            _IsApply = true;
            foreach (var tf in TextureTransformers)
            {
                if (tf == null) continue;
                if (tf.ThisEnable == false) continue;
                tf.Apply(AvatarMaterialDomain);
            }
        }
        public override void Revart(MaterialDomain AvatarMaterialDomain = null)
        {
            if (!_IsApply) return;
            _IsApply = false;

            var Revarstfs = new List<TextureTransformer>(TextureTransformers);
            Revarstfs.Reverse();
            foreach (var tf in Revarstfs)
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
                foreach (var tf in TextureTransformers)
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
                var Revarstfs = new List<TextureTransformer>(TextureTransformers);
                Revarstfs.Reverse();
                foreach (var tf in Revarstfs)
                {
                    if (tf == null) continue;
                    tf.Revart();
                }
            }
        }



        bool PossibleApplyCheck()
        {
            bool PossibleFlag = true;
            foreach (var tf in TextureTransformers)
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
            foreach (var tf in TextureTransformers)
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