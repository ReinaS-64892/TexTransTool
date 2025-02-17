using System;
using net.rs64.TexTransTool.MultiLayerImage.LayerData;
using net.rs64.TexTransCoreEngineForUnity;
using UnityEngine;

namespace net.rs64.TexTransTool.MultiLayerImage.Importer
{
    [SpecialDataOf(typeof(SolidColorLayerData))]
    public class SolidColorLayerDataImporter : ISpecialLayerDataImporter
    {
        void ISpecialLayerDataImporter.CreateSpecial(Action<AbstractLayer, AbstractLayerData> copyFromData, GameObject newLayer, AbstractLayerData specialData)
        {
            var solidColorLayerData = specialData as SolidColorLayerData;
            var SolidColorLayerComponent = newLayer.AddComponent<SolidColorLayer>();
            copyFromData(SolidColorLayerComponent, solidColorLayerData);

            SolidColorLayerComponent.Color = solidColorLayerData.Color.ToUnity();
        }
    }
}
