using System;
using net.rs64.TexTransCore;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class LevelAdjustmentLayer : AbstractGrabLayer
    {
        internal const string ComponentName = "TTT LevelAdjustmentLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public Level RGB;
        public Level Red;
        public Level Green;
        public Level Blue;

        public override void GetImage(RenderTexture grabSource, RenderTexture writeTarget, IOriginTexture originTexture)
        {
            var mat = MatTemp.GetTempMatShader(SpecialLayerShaders.LevelAdjustmentShader);
            using (TTRt.U(out var tempRt, grabSource.descriptor))
            {

                mat.EnableKeyword("RGB");
                RGB.SetMaterialProperty(mat);
                Graphics.Blit(grabSource, tempRt, mat);
                mat.DisableKeyword("RGB");

                // Graphics.CopyTexture(tempRt, WriteTarget);

                mat.EnableKeyword("Red");
                Red.SetMaterialProperty(mat);
                Graphics.Blit(tempRt, writeTarget, mat);
                mat.DisableKeyword("Red");

                mat.EnableKeyword("Green");
                Green.SetMaterialProperty(mat);
                Graphics.Blit(writeTarget, tempRt, mat);
                mat.DisableKeyword("Green");

                mat.EnableKeyword("Blue");
                Blue.SetMaterialProperty(mat);
                Graphics.Blit(tempRt, writeTarget, mat);
                mat.DisableKeyword("Blue");
            }
        }

        [Serializable]
        public class Level
        {
            [Range(0, 0.99f)] public float InputFloor = 0;
            [Range(0.01f, 1)] public float InputCeiling = 1;

            [Range(0.1f, 9.9f)] public float Gamma = 1;

            [Range(0, 1)] public float OutputFloor = 0;
            [Range(0, 1)] public float OutputCeiling = 1;

            internal void SetMaterialProperty(Material material)
            {
                material.SetFloat("_InputFloor", InputFloor);
                material.SetFloat("_InputCeiling", InputCeiling);
                material.SetFloat("_Gamma", Gamma);
                material.SetFloat("_OutputFloor", OutputFloor);
                material.SetFloat("_OutputCeiling", OutputCeiling);
            }
        }
    }
}
