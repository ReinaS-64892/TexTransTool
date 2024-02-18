using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    internal static class SpecialLayerShaders
    {
        public static void Init()
        {
            HSVAdjustmentShader = Shader.Find(HSV_ADJUSTMENT_SHADER);
            LevelAdjustmentShader = Shader.Find(LEVEL_ADJUSTMENT_SHADER);
        }

        public const string HSV_ADJUSTMENT_SHADER = "Hidden/HSVAdjustment";
        public static Shader HSVAdjustmentShader;

        public const string LEVEL_ADJUSTMENT_SHADER = "Hidden/LevelAdjustment";
        public static Shader LevelAdjustmentShader;



    }
}