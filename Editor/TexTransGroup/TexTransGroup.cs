#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool
{
    [AddComponentMenu("TexTransTool/TTT TexTransGroup")]
    internal class TexTransGroup : TextureTransformer
    {
        public override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;
        public IEnumerable<TextureTransformer> Targets => transform.GetChildren().Select(x => x.GetComponent<TextureTransformer>()).Where(x => x != null);

        public override List<Renderer> GetRenderers => TextureTransformerFilter(Targets).SelectMany(I => I.GetRenderers).ToList();

        public override bool IsPossibleApply => PossibleApplyCheck();

        public override void Apply(IDomain domain)
        {
            if (!IsPossibleApply)
            {
                Debug.LogWarning("TexTransGroup : このグループ内のどれかがプレビューできる状態ではないため実行できません。");
                return;
            }

            domain.ProgressStateEnter("TexTransGroup");

            var targetList = TextureTransformerFilter(Targets).ToArray();
            var count = 0;
            foreach (var tf in targetList)
            {
                count += 1;
                tf.Apply(domain);
                domain.ProgressUpdate(tf.name + " Apply", (float)count / targetList.Length);
            }
            domain.ProgressStateExit();
        }
        public static IEnumerable<TextureTransformer> TextureTransformerFilter(IEnumerable<TextureTransformer> targets) => targets.Where(tf => tf != null && tf.ThisEnable);

        bool PossibleApplyCheck()
        {
            bool possibleFlag = true;
            foreach (var tf in Targets)
            {
                if (tf == null) continue;
                if (tf.ThisEnable == false) continue;
                if (!tf.IsPossibleApply)
                {
                    possibleFlag = false;
                }
            }
            return possibleFlag;
        }

    }
}
#endif
