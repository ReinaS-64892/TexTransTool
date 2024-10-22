using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using UnityEngine;

namespace net.rs64.TexTransCoreEngineForUnity
{
    public class HSLAdjustmentExecuter : IGrabBlendingExecuter
    {
        public Type ExecutionTarget => typeof(HSLAdjustment);

        void IGrabBlendingExecuter.GrabExecute(TTCEForUnity engin, RenderTexture rt, TTGrabBlendingUnityObject gbUnity, ITTGrabBlending grabBlending)
        {
            var cs = gbUnity.Compute;
            var hsl = (HSLAdjustment)grabBlending;

            using (new UsingColoSpace(rt, gbUnity.IsLinerRequired))
            {
                cs.SetFloat(nameof(HSLAdjustment.Hue), hsl.Hue);
                cs.SetFloat(nameof(HSLAdjustment.Saturation), hsl.Saturation);
                cs.SetFloat(nameof(HSLAdjustment.Lightness), hsl.Lightness);
                cs.SetTexture(0, "Tex", rt);

                cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);
            }
        }
    }
}
