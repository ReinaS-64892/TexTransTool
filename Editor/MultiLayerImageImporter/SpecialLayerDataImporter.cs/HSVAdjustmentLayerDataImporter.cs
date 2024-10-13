using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using net.rs64.MultiLayerImage.LayerData;
using net.rs64.TexTransUnityCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    [SpecialDataOf(typeof(HSVAdjustmentLayerData))]
    public class HSVAdjustmentLayerDataImporter : ISpecialLayerDataImporter
    {
        void ISpecialLayerDataImporter.CreateSpecial(Action<AbstractLayer, AbstractLayerData> copyFromData, GameObject newLayer, AbstractLayerData specialData)
        {
            var hSVAdjustmentLayerData = specialData as HSVAdjustmentLayerData;
            var HSVAdjustmentLayerComponent = newLayer.AddComponent<HSVAdjustmentLayer>();
            copyFromData(HSVAdjustmentLayerComponent, hSVAdjustmentLayerData);

            HSVAdjustmentLayerComponent.Hue = hSVAdjustmentLayerData.Hue;
            HSVAdjustmentLayerComponent.Saturation = hSVAdjustmentLayerData.Saturation;
            HSVAdjustmentLayerComponent.Value = hSVAdjustmentLayerData.Value;
        }
    }
}
