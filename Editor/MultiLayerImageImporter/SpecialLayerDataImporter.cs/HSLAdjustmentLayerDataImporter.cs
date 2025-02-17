using System;
using net.rs64.TexTransTool.MultiLayerImage.LayerData;
using UnityEngine;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    [SpecialDataOf(typeof(HSLAdjustmentLayerData))]
    public class HSLAdjustmentLayerDataImporter : ISpecialLayerDataImporter
    {
        void ISpecialLayerDataImporter.CreateSpecial(Action<AbstractLayer, AbstractLayerData> copyFromData, GameObject newLayer, AbstractLayerData specialData)
        {
            var hSLAdjustmentLayerData = specialData as HSLAdjustmentLayerData;
            var HSVAdjustmentLayerComponent = newLayer.AddComponent<HSLAdjustmentLayer>();
            copyFromData(HSVAdjustmentLayerComponent, hSLAdjustmentLayerData);

            HSVAdjustmentLayerComponent.Hue = hSLAdjustmentLayerData.Hue;
            HSVAdjustmentLayerComponent.Saturation = hSLAdjustmentLayerData.Saturation;
            HSVAdjustmentLayerComponent.Lightness = hSLAdjustmentLayerData.Lightness;
        }
    }
}
