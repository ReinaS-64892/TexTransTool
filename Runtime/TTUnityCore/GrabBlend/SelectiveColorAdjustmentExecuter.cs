using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using Unity.Collections;
using UnityEngine;

namespace net.rs64.TexTransCoreEngineForUnity
{
    public class SelectiveColorAdjustmentExecuter : IGrabBlendingExecuter
    {
        public Type ExecutionTarget => typeof(SelectiveColorAdjustment);

        void IGrabBlendingExecuter.GrabExecute(TTCoreEngineForUnity engin, RenderTexture rt, TTGrabBlending grabBlending)
        {
            var gbUnity = (TTGrabBlendingUnityObject)grabBlending.ComputeKey;
            var cs = gbUnity.Compute;
            var selectiveColor = (SelectiveColorAdjustment)grabBlending;

            using (new UsingColoSpace(rt, gbUnity.IsLinerRequired))
            {
                cs.SetFloats(nameof(SelectiveColorAdjustment.RedsCMYK), selectiveColor.RedsCMYK.ToArray());
                cs.SetFloats(nameof(SelectiveColorAdjustment.YellowsCMYK), selectiveColor.YellowsCMYK.ToArray());
                cs.SetFloats(nameof(SelectiveColorAdjustment.GreensCMYK), selectiveColor.GreensCMYK.ToArray());
                cs.SetFloats(nameof(SelectiveColorAdjustment.CyansCMYK), selectiveColor.CyansCMYK.ToArray());
                cs.SetFloats(nameof(SelectiveColorAdjustment.BluesCMYK), selectiveColor.BluesCMYK.ToArray());
                cs.SetFloats(nameof(SelectiveColorAdjustment.MagentasCMYK), selectiveColor.MagentasCMYK.ToArray());
                cs.SetFloats(nameof(SelectiveColorAdjustment.WhitesCMYK), selectiveColor.WhitesCMYK.ToArray());
                cs.SetFloats(nameof(SelectiveColorAdjustment.NeutralsCMYK), selectiveColor.NeutralsCMYK.ToArray());
                cs.SetFloats(nameof(SelectiveColorAdjustment.BlacksCMYK), selectiveColor.BlacksCMYK.ToArray());
                cs.SetFloat(nameof(SelectiveColorAdjustment.IsAbsolute), selectiveColor.IsAbsolute ? 1f : 0f);
                cs.SetTexture(0, "Tex", rt);

                cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);
            }
        }
    }

}
