using System;
using net.rs64.TexTransTool.MultiLayerImage.LayerData;
using UnityEngine;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    [SpecialDataOf(typeof(LevelAdjustmentLayerData))]
    public class LevelAdjustmentLayerDataImporter : ISpecialLayerDataImporter
    {
        void ISpecialLayerDataImporter.CreateSpecial(Action<AbstractLayer, AbstractLayerData> copyFromData, GameObject newLayer, AbstractLayerData specialData)
        {
            var levelAdjustmentLayerData = specialData as LevelAdjustmentLayerData;
            var level = newLayer.AddComponent<LevelAdjustmentLayer>();
            copyFromData(level, levelAdjustmentLayerData);

            level.RGB = Convert(levelAdjustmentLayerData.RGB);
            level.Red = Convert(levelAdjustmentLayerData.Red);
            level.Green = Convert(levelAdjustmentLayerData.Green);
            level.Blue = Convert(levelAdjustmentLayerData.Blue);

            static LevelAdjustmentLayer.Level Convert(LevelAdjustmentLayerData.LevelData levelData)
            {
                var level = new LevelAdjustmentLayer.Level();

                level.InputFloor = levelData.InputFloor;
                level.InputCeiling = levelData.InputCeiling;
                level.Gamma = levelData.Gamma;
                level.OutputFloor = levelData.OutputFloor;
                level.OutputCeiling = levelData.OutputCeiling;

                return level;
            }
        }
    }
}
