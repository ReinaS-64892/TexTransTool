#nullable enable
using System;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using UnityEngine;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class LevelAdjustmentLayer : AbstractLayer
    {
        internal const string ComponentName = "TTT LevelAdjustmentLayer";
        internal const string MenuPath = MultiLayerImageCanvas.FoldoutName + "/" + ComponentName;
        public Level RGB = new();
        public Level Red = new();
        public Level Green = new();
        public Level Blue = new();

        internal override LayerObject<ITexTransToolForUnity> GetLayerObject(GenerateLayerObjectContext ctx)
        {
            var domain = ctx.Domain;
            var engine = ctx.Engine;

            domain.LookAt(this);
            domain.LookAt(gameObject);

            var lm = GetAlphaMaskObject(ctx);
            var blKey = engine.QueryBlendKey(BlendTypeKey);
            var la = new LevelAdjustment(RGB.ToTTCoreLevelData(), Red.ToTTCoreLevelData(), Green.ToTTCoreLevelData(), Blue.ToTTCoreLevelData());

            return new GrabBlendingAsLayer<ITexTransToolForUnity>(Visible, lm, Clipping, blKey, la);
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

            public LevelData ToTTCoreLevelData()
            {
                var levelData = new LevelData();
                levelData.InputFloor = InputFloor;
                levelData.InputCeiling = InputCeiling;
                levelData.Gamma = Gamma;
                levelData.OutputFloor = OutputFloor;
                levelData.OutputCeiling = OutputCeiling;
                return levelData;
            }
        }
    }
}
