using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using UnityEngine;

namespace net.rs64.TexTransCoreEngineForUnity
{
    public class HSVAdjustmentExecuter : IGrabBlendingExecuter
    {
        public Type ExecutionTarget => typeof(HSVAdjustment);

        void IGrabBlendingExecuter.GrabExecute(TTCEForUnity engin, RenderTexture rt, TTGrabBlendingUnityObject gbUnity, ITTGrabBlending grabBlending)
        {
            var cs = gbUnity.Compute;
            var hsv = (HSVAdjustment)grabBlending;

            using (new UsingColoSpace(rt, gbUnity.IsLinerRequired))
            {
                cs.SetFloat(nameof(HSVAdjustment.Hue), hsv.Hue);
                cs.SetFloat(nameof(HSVAdjustment.Saturation), hsv.Saturation);
                cs.SetFloat(nameof(HSVAdjustment.Value), hsv.Value);
                cs.SetTexture(0, "Tex", rt);

                cs.Dispatch(0, Mathf.Max(1, rt.width / 32), Mathf.Max(1, rt.height / 32), 1);
            }
        }
    }
}
