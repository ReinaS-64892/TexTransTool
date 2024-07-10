using net.rs64.TexTransTool.Utils;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class UnityGradationMapLayer : AbstractGrabLayer
    {
        internal const string ComponentName = "TTT UnityGradationMapLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;

        public Gradient Gradation;


        public override void GetImage(RenderTexture grabSource, RenderTexture writeTarget, IOriginTexture originTexture)
        {
            var mat = MatTemp.GetTempMatShader(SpecialLayerShaders.LuminanceMappingShader);
            mat.SetTexture("_MapTex", GradientTempTexture.Get(Gradation, 1));

            Graphics.Blit(grabSource, writeTarget, mat);
        }
    }
}
