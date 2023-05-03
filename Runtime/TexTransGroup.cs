#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
namespace Rs64.TexTransTool
{
    [AddComponentMenu("TexTransTool/TexTransGroup")]
    public class TexTransGroup : TextureTransformer
    {
        public List<TextureTransformer> TextureTransformers = new List<TextureTransformer>();
        [SerializeField, HideInInspector] bool _IsAppry;
        public override bool IsAppry => _IsAppry;

        public override bool IsPossibleAppry => PossibleAppryCheck();

        public override bool IsPossibleCompile => PossibleCompileCheck();

        public override void Appry()
        {
            if (_IsAppry) return;
            _IsAppry = true;
            foreach (var tf in TextureTransformers)
            {
                if (tf == null) continue;
                tf.Appry();
            }
        }
        public override void Revart()
        {
            if (!_IsAppry) return;
            _IsAppry = false;

            var Revarstfs = new List<TextureTransformer>(TextureTransformers);
            Revarstfs.Reverse();
            foreach (var tf in Revarstfs)
            {
                if (tf == null) continue;
                tf.Revart();
            }
        }
        public override void Compile()
        {
            foreach (var tf in TextureTransformers)
            {
                if (tf == null) continue;
                tf.Compile();
                tf.Appry();
            }

            var Revarstfs = new List<TextureTransformer>(TextureTransformers);
            Revarstfs.Reverse();
            foreach (var tf in Revarstfs)
            {
                if (tf == null) continue;
                tf.Revart();
            }
        }



        bool PossibleAppryCheck()
        {
            bool PossibleFlag = true;
            foreach (var tf in TextureTransformers)
            {
                if (tf == null) continue;
                if (!tf.IsPossibleAppry)
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