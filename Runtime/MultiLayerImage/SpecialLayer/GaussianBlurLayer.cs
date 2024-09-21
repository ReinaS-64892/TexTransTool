using net.rs64.TexTransCore;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class GaussianBlurLayer : AbstractGrabLayer
    {
        internal const string ComponentName = "TTT GaussianBlurLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public float Sigma;
        public override void GetImage(RenderTexture grabSource, RenderTexture writeTarget, IOriginTexture originTexture)
        {
            using (TTRt.U(out var tmpRtR, grabSource.width, grabSource.height, false, false, false, true))
            using (TTRt.U(out var tmpRtT, grabSource.width, grabSource.height, false, false, false, true))
            {
                Graphics.CopyTexture(grabSource, tmpRtR);
                var gbCompute = SpecialLayerShaders.GaussianBluerComputeShader;
                var kernelID = gbCompute.FindKernel("GaussianBluer");

                gbCompute.SetTexture(kernelID, "SourceTexture", tmpRtR);
                gbCompute.SetTexture(kernelID, "TargetTexture", tmpRtT);
                gbCompute.SetFloat("Sigma", Sigma);
                gbCompute.SetInts("TextureSize", grabSource.width, grabSource.height);

                gbCompute.Dispatch(kernelID, grabSource.width / 32, grabSource.height / 32, 1);
                Graphics.CopyTexture(tmpRtT, writeTarget);
            }
        }
    }
}
