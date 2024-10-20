using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using net.rs64.TexTransTool.MultiLayerImage.LayerData;
using net.rs64.TexTransCoreEngineForUnity.Utils;
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
